
using backend.Enums;
namespace backend.Models;

public class Album
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ArtistId { get; set; }
    public int? ReleaseYear { get; set; }
    public string? CoverImageUrl { get; set; }
    public int Rating { get; set; }
    public int ReviewCount { get; set; }
    public Genre? Genre { get; set; }    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}