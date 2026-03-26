namespace backend.Models;

public class Artist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // Navigation property
    public ICollection<Album>? Albums { get; set; }
}