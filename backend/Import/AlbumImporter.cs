using System.Drawing;
using System.Drawing.Imaging;
using OfficeOpenXml;
using backend.Data;
using backend.Models;
using backend.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Import
{
    public class AlbumImporter
    {
        private readonly AppDbContext _context;
        public AlbumImporter(AppDbContext context)
        {
            _context = context;
        }

        public void Import(string excelPath)
        {
            Console.WriteLine("Using database connection: " + _context.Database.GetDbConnection().ConnectionString);
            // If the path is not rooted, assume it's relative to the backend directory
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string resolvedPath = excelPath;
            if (!Path.IsPathRooted(excelPath))
            {
                resolvedPath = Path.Combine(baseDir, excelPath);
            }
            resolvedPath = Path.GetFullPath(resolvedPath);
            Console.WriteLine($"Attempting to open Excel file at: {resolvedPath}");
            if (!File.Exists(resolvedPath))
            {
                Console.WriteLine("ERROR: File not found at the specified path.");
                return;
            }
            // Use resolvedPath for further processing
            // Set EPPlus license for version 8+
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(new FileInfo(resolvedPath));
            var worksheet = package.Workbook.Worksheets["Country"];
            if (worksheet == null)
            {
                Console.WriteLine("Worksheet 'Country' not found in the Excel file.");
                Console.WriteLine("Available worksheets:");
                foreach (var ws in package.Workbook.Worksheets)
                {
                    Console.WriteLine("- '" + ws.Name + "'");
                }
                return;
            }
            var images = worksheet.Drawings.OfType<OfficeOpenXml.Drawing.ExcelPicture>().ToList();
            // Find the last used column in row 2
            int lastCol = worksheet.Dimension.End.Column;
            // Find the first column to process (F = 6)
            int firstCol = 6;
            // Start from lastCol and work backwards to F
            for (int col = lastCol; col >= firstCol; col--)
            {
                if (string.IsNullOrWhiteSpace(worksheet.Cells[2, col].Text))
                    continue;
                string albumTitle = worksheet.Cells[2, col].Text.Trim();
                string artistName = worksheet.Cells[3, col].Text.Trim();
                int rating = worksheet.Cells[1, col].GetValue<int>();

                // Try to find an image in row 4, current column
                string coverImagePath = string.Empty;
                var image = images.FirstOrDefault(img => img.From.Row == 3 && img.From.Column == col - 1); // EPPlus is 0-based
                if (image != null)
                {
                    using var imgStream = new MemoryStream(image.Image.ImageBytes);
                    using var img = Image.FromStream(imgStream);

                    var format = img.RawFormat;
                    var ext = "png";

                    if (ImageFormat.Jpeg.Equals(format)) ext = "jpg";
                    else if (ImageFormat.Gif.Equals(format)) ext = "gif";
                    else if (ImageFormat.Bmp.Equals(format)) ext = "bmp";
                    else if (ImageFormat.Png.Equals(format)) ext = "png";

                    var safeAlbumTitle = SanitizeFileName(albumTitle);
                    var safeArtistName = SanitizeFileName(artistName);

                    var fileName = $"{safeAlbumTitle}_{safeArtistName}.{ext}";

                    var relativeDir = Path.Combine("Data", "AlbumCovers");
                    var absoluteDir = Path.Combine(AppContext.BaseDirectory, relativeDir);

                    Directory.CreateDirectory(absoluteDir);

                    var savePath = Path.Combine(absoluteDir, fileName);

                    img.Save(savePath, format);
                    Console.WriteLine($"Saved album cover image to: {savePath}");

                    coverImagePath = Path.Combine(relativeDir, fileName);
                }

                static string SanitizeFileName(string value)
                {
                    if (string.IsNullOrWhiteSpace(value))
                        return "unknown";

                    foreach (var c in Path.GetInvalidFileNameChars())
                    {
                        value = value.Replace(c, '_');
                    }

                    return value.Replace(' ', '_').Trim('_');
                }
                var artist = _context.Artists
                    .FirstOrDefault(a => a.Name.ToLower() == artistName.ToLower());
                if (artist == null)
                {
                    artist = new Artist { Name = artistName };
                    _context.Artists.Add(artist);
                    _context.SaveChanges(); // Save to get Artist Id
                }

                // Check if album already exists
                var existingAlbum = _context.Albums
                    .FirstOrDefault(a => a.Title.ToLower() == albumTitle.ToLower() && a.ArtistId == artist.Id);
                if (existingAlbum != null)
                {
                    Console.WriteLine($"Album '{albumTitle}' by '{artistName}' already exists, skipping...");
                    continue;
                }

                var album = new Album
                {
                    Title = albumTitle,
                    ArtistId = artist.Id,
                    CoverImageUrl = coverImagePath,
                    Genre = Genre.Country,
                    CreatedAt = DateTime.UtcNow
                };
                // Get review count from row 25 for this album/column
                string reviewCountStr = worksheet.Cells[25, col].Text.Trim();
                int reviewCount = 0;
                int.TryParse(reviewCountStr, out reviewCount);
                album.ReviewCount = reviewCount;
                _context.Albums.Add(album);
                _context.SaveChanges(); // Save album to get its Id
                Console.WriteLine($"Added album: {albumTitle} by {artistName}");

                var songsToAdd = new List<Song>();
                var rankingsToAdd = new List<UserSongRanking>();
                
                // Get a valid user ID from the database
                var firstUser = _context.Users.FirstOrDefault();
                if (firstUser == null)
                {
                    Console.WriteLine("No users found in database. Skipping rankings for this album.");
                    continue;
                }
                string userId = firstUser.Id;
                for (int row = 5; row <= 24; row++)
                {
                    string songTitle = worksheet.Cells[row, col].Text.Trim();
                    if (string.IsNullOrWhiteSpace(songTitle))
                    {
                        // Stop processing songs for this album when an empty cell is reached
                        break;
                    }
                    int rank = row - 4; // Row 5 = rank 1, etc.
                    var song = new Song
                    {
                        Title = songTitle,
                        AlbumId = album.Id,
                    };
                    songsToAdd.Add(song);
                }

                // Batch add songs
                _context.Songs.AddRange(songsToAdd);
                _context.SaveChanges(); // Save to get song IDs

                // Create rankings after songs have IDs
                for (int i = 0; i < songsToAdd.Count; i++)
                {
                    var ranking = new UserSongRanking
                    {
                        UserId = userId,
                        SongId = songsToAdd[i].Id,
                        AlbumId = album.Id,
                        Rank = i + 1
                    };
                    rankingsToAdd.Add(ranking);
                }

                // Batch add rankings
                _context.UserSongRankings.AddRange(rankingsToAdd);
            }
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Import failed: " + ex.ToString());
                if (ex.Data != null && ex.Data.Count > 0)
                {
                    Console.WriteLine("Exception Data:");
                    foreach (var key in ex.Data.Keys)
                    {
                        Console.WriteLine($"  {key}: {ex.Data[key]}");
                    }
                }
            }
        }
    }
}
