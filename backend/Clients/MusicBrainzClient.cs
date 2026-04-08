using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using backend.Dtos;

namespace backend.Clients;

public class MusicBrainzClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // MB enforces a strict 1 req/sec rate limit.
    // This semaphore ensures we never fire two requests simultaneously.
    private static readonly SemaphoreSlim _rateLimiter = new(1, 1);

    public MusicBrainzClient(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    /// <summary>
    /// Step 1: Search for release-groups matching the artist and album name.
    /// Returns up to <paramref name="limit"/> candidates, ordered by MB relevance score.
    /// </summary>
    public async Task<List<MbReleaseGroup>> SearchReleaseGroupsAsync(
        string artistName,
        string albumName,
        int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(artistName) || string.IsNullOrWhiteSpace(albumName))
            return new List<MbReleaseGroup>();

        var normalizedArtist = artistName.Trim().ToLowerInvariant();
        var normalizedAlbum = albumName.Trim().ToLowerInvariant();
        var cacheKey = $"mb:search:{normalizedArtist}:{normalizedAlbum}:{limit}";

        if (_cache.TryGetValue(cacheKey, out List<MbReleaseGroup>? cached))
            return cached;

        var query = $"artist:\"{artistName.Trim()}\" AND releasegroup:\"{albumName.Trim()}\" AND primarytype:Album";
        var url = $"release-group?query={Uri.EscapeDataString(query)}&fmt=json&limit={limit}";

        var response = await SendWithRateLimitAsync(url);
        var result = JsonSerializer.Deserialize<MbReleaseGroupSearchResponse>(response, _jsonOptions);
        var groups = result?.ReleaseGroups ?? new List<MbReleaseGroup>();

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        _cache.Set(cacheKey, groups, cacheOptions);

        return groups;
    }

    /// <summary>
    /// Step 2: Given a release-group ID, fetch all official releases under it
    /// (with track listings) and return the best one using strategy:
    /// Official → prefer CD format → prefer US country → earliest date.
    /// </summary>
    public async Task<MbRelease?> GetBestReleaseAsync(string releaseGroupId)
    {
        var cacheKey = $"mb:releases:{releaseGroupId}";
        if (_cache.TryGetValue(cacheKey, out MbRelease? cachedRelease))
            return cachedRelease;

        var url = $"release?release-group={Uri.EscapeDataString(releaseGroupId)}&inc=recordings&status=official&fmt=json";

        var response = await SendWithRateLimitAsync(url);
        var result = JsonSerializer.Deserialize<MbReleaseSearchResponse>(response, _jsonOptions);

        var releases = result?.Releases ?? new List<MbRelease>();
        var best = PickBestRelease(releases);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };
        _cache.Set(cacheKey, best, cacheOptions);

        return best;
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private async Task<string> SendWithRateLimitAsync(string relativeUrl, CancellationToken cancellationToken = default)
{
    await _rateLimiter.WaitAsync(cancellationToken);

    try
    {
        var httpResponse = await _httpClient.GetAsync(relativeUrl, cancellationToken);

        if ((int)httpResponse.StatusCode == 503)
        {
            var body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"MusicBrainz rate-limited the request. Status: 503. Body: {body}");
        }

        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadAsStringAsync(cancellationToken);
    }
    finally
    {
        await Task.Delay(4000, cancellationToken);
        _rateLimiter.Release();
    }
}

    /// <summary>
    /// Picks the canonical release from a list using the agreed strategy:
    /// 1. Official status only (already filtered by the API query)
    /// 2. Prefer CD format
    /// 3. Prefer US country
    /// 4. Earliest date as tiebreaker
    /// 5. Must have tracks (skip releases with empty media)
    /// </summary>
    private static MbRelease? PickBestRelease(List<MbRelease> releases)
    {
        var withTracks = releases
            .Where(r => r.Media.Any(m => m.Tracks.Count > 0))
            .ToList();

        if (withTracks.Count == 0)
            return releases.FirstOrDefault();

        return withTracks
            .OrderBy(r => IsCD(r) ? 0 : 1)
            .ThenBy(r => IsUS(r) ? 0 : 1)
            .ThenBy(r => ParseYear(r.Date))
            .FirstOrDefault();
    }

    private static bool IsCD(MbRelease r) =>
        r.Media.Any(m => string.Equals(m.Format, "CD", StringComparison.OrdinalIgnoreCase));

    private static bool IsUS(MbRelease r) =>
        string.Equals(r.Country, "US", StringComparison.OrdinalIgnoreCase);

    private static int ParseYear(string? date)
    {
        if (string.IsNullOrWhiteSpace(date)) return int.MaxValue;
        // Date can be "1990", "1990-02", or "1990-02-27" — we only need the year
        if (int.TryParse(date.AsSpan(0, Math.Min(4, date.Length)), out var year))
            return year;
        return int.MaxValue;
    }
}
