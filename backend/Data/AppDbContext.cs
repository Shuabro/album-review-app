using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Song> Songs => Set<Song>();

     protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Album>()
            .HasOne<Artist>()
            .WithMany()
            .HasForeignKey(a => a.ArtistId);

        modelBuilder.Entity<Song>()
            .HasOne<Album>()
            .WithMany()
            .HasForeignKey(s => s.AlbumId);
    }
}