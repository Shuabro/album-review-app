using System.Drawing;
using System.Drawing.Imaging;
using FuzzySharp;
using backend.Clients;
using backend.Data;
using backend.Dtos;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AlbumImportService
{
    private readonly MusicBrainzClient _mbClient;
    private readonly AppDbContext _dbContext;
    private readonly CloudinaryService _cloudinary;
    private readonly IHttpClientFactory _httpClientFactory;

    public AlbumImportService(
        MusicBrainzClient mbClient,
        AppDbContext dbContext,
        CloudinaryService cloudinary,
        IHttpClientFactory httpClientFactory)
    {
        _mbClient = mbClient;
        _dbContext = dbContext;
        _cloudinary = cloudinary;
        _httpClientFactory = httpClientFactory;
    }

    // -----------------------------------------------------------------------
    // Search  (Step 1)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Searches MusicBrainz for release-groups matching the request,
    /// scores them with FuzzySharp, resolves the best match to a specific
    /// release, and returns a ranked result.
    /// </summary>
    public async Task<AlbumSearchResultDto> SearchAlbumAsync(AlbumSearchRequest request)
    {
        var releaseGroups = await _mbClient.SearchReleaseGroupsAsync(
            request.ArtistName,
            request.AlbumName);

        if (releaseGroups.Count == 0)
            return new AlbumSearchResultDto();

        // Score and rank all candidates using FuzzySharp token sort ratio,
        // which handles word-order differences gracefully (e.g. "Real World, Here in the" vs "Here in the Real World").
        var scored = releaseGroups
            .Select(rg => new
            {
                ReleaseGroup = rg,
                Score = ComputeScore(rg, request.ArtistName, request.AlbumName)
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        var bestGroup = scored[0].ReleaseGroup;
        var bestScore = scored[0].Score;

        // Resolve the best release-group to a specific release (gets us the release ID and format).
        var bestRelease = await _mbClient.GetBestReleaseAsync(bestGroup.Id);

        var bestMatch = MapToCandidate(bestGroup, bestRelease, bestScore);

        // TODO: resolve releases for otherMatches when the UI supports selection.
        // For now return them as lightweight candidates without release resolution
        // to avoid additional MB API calls.
        var otherMatches = scored
            .Skip(1)
            .Select(x => MapToCandidate(x.ReleaseGroup, null, x.Score))
            .ToList();

        return new AlbumSearchResultDto
        {
            BestMatch = bestMatch,
            OtherMatches = otherMatches
        };
    }

    // -----------------------------------------------------------------------
    // Save  (Step 2)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Persists the confirmed album candidate to the database.
    /// Upserts the artist, creates the album, and saves the track listing.
    /// </summary>
    public async Task<Album> SaveAlbumAsync(SaveAlbumRequest request)
    {
        var candidate = request.Candidate;

        // If the candidate is an unresolved otherMatch (ExternalId is a release-group ID),
        // resolve it now to get the real release ID, tracks, and cover art info.
        if (!candidate.ReleaseResolved)
        {
            var resolvedRelease = await _mbClient.GetBestReleaseAsync(candidate.ExternalId);
            if (resolvedRelease is not null)
            {
                // Find the release-group from MB to re-map cleanly
                var releaseGroups = await _mbClient.SearchReleaseGroupsAsync(
                    candidate.ArtistName, candidate.Title, limit: 1);
                var group = releaseGroups.FirstOrDefault();
                if (group is not null)
                {
                    candidate = MapToCandidate(group, resolvedRelease, candidate.Score ?? 0);
                }
                else
                {
                    // Fallback: just update the ExternalId and tracks from the resolved release
                    candidate.ExternalId = resolvedRelease.Id;
                    candidate.CoverImageUrl = resolvedRelease.CoverArtArchive?.Front == true
                        ? $"https://coverartarchive.org/release/{resolvedRelease.Id}/front"
                        : null;
                    candidate.Tracks = resolvedRelease.Media
                        .SelectMany(m => m.Tracks)
                        .Select(t => new TrackDto { Position = t.Position, Title = t.Title, DurationMs = t.Length })
                        .ToList();
                }
            }
        }

        // Duplicate check — prevent saving the same release twice
        var existingAlbum = await _dbContext.Albums
            .Include(a => a.Artist)
            .FirstOrDefaultAsync(a =>
                a.Title.ToLower() == candidate.Title.ToLower() &&
                a.Artist!.Name.ToLower() == candidate.ArtistName.ToLower());
        if (existingAlbum is not null)
            return existingAlbum;

        // Upsert artist — find by name (case-insensitive) or create new
        var artist = await _dbContext.Artists
            .FirstOrDefaultAsync(a => a.Name.ToLower() == candidate.ArtistName.ToLower());

        if (artist is null)
        {
            artist = new Artist { Name = candidate.ArtistName };
            _dbContext.Artists.Add(artist);
            await _dbContext.SaveChangesAsync();
        }

        // Download cover art from coverartarchive.org and upload to Cloudinary using
        // the legacy Title_Artist naming method for publicId.
        if (candidate.CoverImageUrl != null)
        {
            Console.WriteLine($"[CoverArt] Attempting to process cover art for {candidate.Title} by {candidate.ArtistName}");
            Console.WriteLine($"[CoverArt] candidate.CoverImageUrl: {candidate.CoverImageUrl}");
        }
        else
        {
            Console.WriteLine($"[CoverArt] No cover art URL for {candidate.Title} by {candidate.ArtistName}");
        }
        string? coverImageUrl = candidate.CoverImageUrl is not null
            ? await DownloadAndUploadCoverArtAsync(candidate.CoverImageUrl,
                backend.Utils.MediaNaming.GeneratePublicId(candidate.Title, candidate.ArtistName))
            : null;

        var album = new Album
        {
            Title = candidate.Title,
            ArtistId = artist.Id,
            ReleaseYear = candidate.ReleaseYear,
            CoverImageUrl = coverImageUrl,
            Genre = request.Genre.HasValue ? (backend.Enums.Genre)request.Genre.Value : null,
            Rating = 0,
            ReviewCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Albums.Add(album);
        await _dbContext.SaveChangesAsync();

        // Save track listing
        if (candidate.Tracks.Count > 0)
        {
            var songs = candidate.Tracks.Select(t => new Song
            {
                Title = t.Title,
                AlbumId = album.Id,
                TrackNumber = t.Position,
                DurationMs = t.DurationMs
            }).ToList();

            _dbContext.Songs.AddRange(songs);
            await _dbContext.SaveChangesAsync();
        }

        // Return with artist populated for the frontend response
        album.Artist = artist;
        return album;
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Computes a composite FuzzySharp score for a release-group candidate.
    /// Blends title similarity (60%) and artist similarity (40%).
    /// TokenSortRatio handles word-order differences.
    /// </summary>
    private static int ComputeScore(MbReleaseGroup rg, string artistQuery, string albumQuery)
    {
        var titleScore = Fuzz.TokenSortRatio(
            albumQuery.ToLowerInvariant(),
            rg.Title.ToLowerInvariant());

        var artistName = rg.ArtistCredit.FirstOrDefault()?.Artist?.Name ?? string.Empty;
        var artistScore = Fuzz.TokenSortRatio(
            artistQuery.ToLowerInvariant(),
            artistName.ToLowerInvariant());

        return (int)(titleScore * 0.6 + artistScore * 0.4);
    }

    /// <summary>
    /// Maps a MusicBrainz release-group (and optionally its resolved release)
    /// to an <see cref="AlbumCandidateDto"/>.
    /// </summary>
    private static AlbumCandidateDto MapToCandidate(
        MbReleaseGroup releaseGroup,
        MbRelease? release,
        int score)
    {
        var artistName = releaseGroup.ArtistCredit.FirstOrDefault()?.Artist?.Name
                         ?? string.Empty;

        // Parse the year from the partial date string (e.g. "1990", "1990-02-27")
        var rawDate = release?.Date ?? releaseGroup.FirstReleaseDate;
        int? releaseYear = null;
        if (!string.IsNullOrWhiteSpace(rawDate) &&
            int.TryParse(rawDate.AsSpan(0, Math.Min(4, rawDate.Length)), out var year))
        {
            releaseYear = year;
        }

        // Tracks are empty at search time — populated at confirm time via GetBestReleaseAsync.
        // At that point the caller should re-map with the full release track list.
        var tracks = release?.Media
            .SelectMany(m => m.Tracks)
            .Select(t => new TrackDto
            {
                Position = t.Position,
                Title = t.Title,
                DurationMs = t.Length
            })
            .ToList() ?? [];

        // Only set cover art URL if MB confirms front art exists for this specific release.
        string? coverImageUrl = null;
        if (release?.CoverArtArchive?.Front == true)
            coverImageUrl = $"https://coverartarchive.org/release/{release.Id}/front";

        return new AlbumCandidateDto
        {
            ExternalId = release?.Id ?? releaseGroup.Id,
            Title = releaseGroup.Title,
            ArtistName = artistName,
            ReleaseYear = releaseYear,
            ReleaseDate = rawDate,
            Country = release?.Country,
            Format = release?.Media.FirstOrDefault()?.Format,
            CoverImageUrl = coverImageUrl,
            Score = score,
            TrackCount = release?.TotalTrackCount ?? null,
            Disambiguation = release?.Disambiguation ?? null,
            Tracks = tracks,
            ReleaseResolved = release is not null
        };
    }

    /// <summary>
    /// Downloads cover art from coverartarchive.org and uploads it to Cloudinary.
    /// Returns the Cloudinary URL, or null if either step fails.
    /// Cover art is optional — failure here never prevents the album from saving.
    /// </summary>
    private async Task<string?> DownloadAndUploadCoverArtAsync(string remoteUrl, string publicId)
    {
        try
        {
            Console.WriteLine($"[CoverArt] Downloading {remoteUrl}");
            var client = _httpClientFactory.CreateClient("CoverArt");
            using var response = await client.GetAsync(remoteUrl);
            Console.WriteLine($"[CoverArt] Download response: {(int)response.StatusCode} {response.StatusCode}");

            if (!response.IsSuccessStatusCode) return null;

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
            Console.WriteLine($"[CoverArt] Content-Type: {contentType}");
            if (!contentType.StartsWith("image/")) return null;

            var bytes = await response.Content.ReadAsByteArrayAsync();
            Console.WriteLine($"[CoverArt] Downloaded {bytes.Length / 1024}KB, converting to JPEG...");

            // Convert to JPEG using System.Drawing
            using var inputStream = new MemoryStream(bytes);
            using var image = Image.FromStream(inputStream);
            using var jpegStream = new MemoryStream();
            var jpegEncoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 85L); // 85% quality
            image.Save(jpegStream, jpegEncoder, encoderParams);
            var jpegBytes = jpegStream.ToArray();

            Console.WriteLine($"[CoverArt] Converted to JPEG ({jpegBytes.Length / 1024}KB), uploading to Cloudinary as {publicId}...");
            var url = await _cloudinary.UploadCoverArtAsync(jpegBytes, publicId);
            Console.WriteLine($"[CoverArt] Cloudinary URL: {url ?? "null (upload failed)"}");
            return url;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CoverArt] Failed for {publicId}: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }
}
