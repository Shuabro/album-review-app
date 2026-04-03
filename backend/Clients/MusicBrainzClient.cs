using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class MusicBrainzClient
{
    private readonly HttpClient _httpClient;

    public MusicBrainzClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // public async Task<List<AlbumSearchResult>> SearchAlbumsAsync(string albumName, string artistName)
    // {
    //     if (string.IsNullOrWhiteSpace(albumName) || string.IsNullOrWhiteSpace(artistName))
    //         return new List<AlbumSearchResult>();

    //     var query = $"artist:\"{artistName.Trim()}\" AND releasegroup:\"{albumName.Trim()}\" AND primarytype:Album";
    //     var url = $"https://musicbrainz.org/ws/2/release-group?query={Uri.EscapeDataString(query)}&fmt=json";

    //     var response = await _httpClient.GetAsync(url);
    //     response.EnsureSuccessStatusCode();

    //     var content = await response.Content.ReadAsStringAsync();

    //     var result = JsonSerializer.Deserialize<MusicBrainzSearchResponse>(
    //         content,
    //         new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    //     return result?.ReleaseGroups ?? new List<AlbumSearchResult>();
    // }

    // public async Task<AlbumDetails> GetAlbumDetailsAsync(string releaseGroupId)
    // {
    //     var response = await _httpClient.GetAsync($"https://musicbrainz.org/ws/2/release-group/{releaseGroupId}?inc=releases&fmt=json");
    //     response.EnsureSuccessStatusCode();

    //     var content = await response.Content.ReadAsStringAsync();
    //     return JsonSerializer.Deserialize<AlbumDetails>(content);
    // }
}