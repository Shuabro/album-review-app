using FuzzySharp;
using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Drawing.Imaging;

namespace backend.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CloudinaryService _cloudinary;
    private readonly IWebHostEnvironment _env;

    public AdminController(AppDbContext db, CloudinaryService cloudinary, IWebHostEnvironment env)
    {
        _db = db;
        _cloudinary = cloudinary;
        _env = env;
    }

    /// <summary>
    /// Migrates images from Data/AlbumCovers to Cloudinary and updates DB URLs.
    /// </summary>
    [HttpPost("migrate-covers")]
    public async Task<IActionResult> MigrateCovers()
    {
        var coversDir = Path.Combine(_env.ContentRootPath, "Data", "AlbumCovers");
        if (!Directory.Exists(coversDir))
            return NotFound(new { message = "AlbumCovers directory not found" });

        var albums = await _db.Albums.Include(a => a.Artist).ToListAsync();
        var results = new List<object>();

        // Gather all files in the covers directory
        var allFiles = Directory.GetFiles(coversDir);

        foreach (var album in albums)
        {
            try
            {
                var expectedFilename = backend.Utils.MediaNaming.GeneratePublicId(album.Title, album.Artist?.Name ?? string.Empty);
                var bestMatch = allFiles
                    .Select(f => new {
                        Path = f,
                        Score = Fuzz.Ratio(expectedFilename.ToLowerInvariant(), Path.GetFileNameWithoutExtension(f).ToLowerInvariant())
                    })
                    .OrderByDescending(x => x.Score)
                    .FirstOrDefault();

                if (bestMatch == null || bestMatch.Score <= 50)
                {
                    results.Add(new
                    {
                        album.Id,
                        album.Title,
                        ArtistName = album.Artist?.Name,
                        status = "file-not-found",
                        expectedFilename,
                        bestScore = bestMatch?.Score ?? 0
                    });
                    continue;
                }

                using var img = Image.FromFile(bestMatch.Path);
                var format = img.RawFormat;
                var ext = "png";
                if (ImageFormat.Jpeg.Equals(format)) ext = "jpg";
                else if (ImageFormat.Gif.Equals(format)) ext = "gif";
                else if (ImageFormat.Bmp.Equals(format)) ext = "bmp";
                else if (ImageFormat.Png.Equals(format)) ext = "png";

                using var ms = new MemoryStream();
                if (ext == "jpg") img.Save(ms, ImageFormat.Jpeg);
                else if (ext == "gif") img.Save(ms, ImageFormat.Gif);
                else if (ext == "bmp") img.Save(ms, ImageFormat.Bmp);
                else img.Save(ms, ImageFormat.Png);

                var bytes = ms.ToArray();
                var url = await _cloudinary.UploadCoverArtAsync(bytes, expectedFilename);

                if (!string.IsNullOrWhiteSpace(url))
                {
                    album.CoverImageUrl = url;
                    await _db.SaveChangesAsync();
                    results.Add(new
                    {
                        album.Id,
                        album.Title,
                        ArtistName = album.Artist?.Name,
                        status = "uploaded",
                        url,
                        matchedFile = Path.GetFileName(bestMatch.Path),
                        matchScore = bestMatch.Score
                    });
                }
                else
                {
                    results.Add(new
                    {
                        album.Id,
                        album.Title,
                        ArtistName = album.Artist?.Name,
                        status = "upload-failed"
                    });
                }
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    album.Id,
                    album.Title,
                    ArtistName = album.Artist?.Name,
                    status = "error",
                    error = ex.Message
                });
            }
        }

        return Ok(new
        {
            migrated = results.Count(r => ((dynamic)r).status == "uploaded"),
            details = results
        });
    }
}