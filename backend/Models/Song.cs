namespace backend.Models;

public class Song
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int AlbumId { get; set; }
    public int TrackNumber { get; set; }
    public int? DurationMs { get; set; }

    // Navigation property
    public Album? Album { get; set; }
}