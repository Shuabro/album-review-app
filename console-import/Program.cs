using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using backend.Import;
using backend.Data;
using backend.Models;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;

namespace ConsoleImport
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure DbContext with connection options
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql("Host=localhost;Database=AlbumReviewDb;Username=postgres;Password=Montezuma1969")
                .Options;

            AppDbContext? dbContext = null;
            AlbumImporterTwo? importer = null;
            try
            {
                dbContext = new AppDbContext(options);
                importer = new AlbumImporterTwo(dbContext);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database or importer: {ex.Message}");
                return;
            }

            // Extract album data from spreadsheet
            var albums = ExtractAlbumsFromSpreadsheet("minorrankings.xlsx");

            // Get a valid user ID from the database  
            var firstUser = dbContext.Users.FirstOrDefault();
            if (firstUser == null)
            {
                Console.WriteLine("No users found in database. Please run UserSeeder first.");
                return;
            }
            string userId = firstUser.Id;

            Console.WriteLine($"Found {albums.Count} albums in spreadsheet.");
            Console.WriteLine($"Processing rankings for user: {firstUser.UserName}\n");

            try
            {
                int albumCount = 0;
                foreach (var album in albums)
                {
                    albumCount++;
                    Console.WriteLine($"=== ALBUM {albumCount}/{albums.Count}: {album.AlbumName} ===");
                    
                    // Process this single album
                    var result = importer.ProcessAlbumFromSpreadsheet(album.AlbumName, album.Songs, userId);
                    
                    // Display results summary
                    if (result.MatchedAlbum != null)
                    {
                        Console.WriteLine($"\n📊 Final Results: {result.MatchedSongsCount}/{result.TotalSongsCount} songs matched ({result.MatchPercentage:F1}%)");
                    }
                    
                    Console.WriteLine($"\n--- Press any key to continue to next album ({albums.Count - albumCount} remaining) ---");
                    Console.ReadKey();
                    Console.WriteLine("\n");
                }

                // Save all changes to database
                Console.WriteLine("Saving all rankings to database...");
                int savedCount = dbContext.SaveChanges();
                Console.WriteLine($"✅ Saved {savedCount} rankings to database!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during album processing: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static List<(string AlbumName, List<string> Songs)> ExtractAlbumsFromSpreadsheet(string filePath)
        {
            var albums = new List<(string AlbumName, List<string> Songs)>();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Excel file not found: {filePath}");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    throw new Exception("No worksheets found in the spreadsheet.");
                }

                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null || worksheet.Dimension == null)
                {
                    throw new Exception("No worksheet found or worksheet is empty in the spreadsheet.");
                }

                Console.WriteLine($"Found worksheet: '{worksheet.Name}' with {worksheet.Dimension.End.Column} columns and {worksheet.Dimension.End.Row} rows");

                // Iterate through columns to get each album
                for (int col = 2; col <= worksheet.Dimension.End.Column; col++) // Starting from column 2 (B)
                {
                    var albumName = worksheet.Cells[3, col].Text.Trim(); // Album names are in row 3
                    
                    if (string.IsNullOrEmpty(albumName))
                        continue;

                    var songNames = new List<string>();
                    
                    // Get songs from rows 8-22 (#1-#15 rankings)
                    for (int row = 8; row <= 22; row++)
                    {
                        var songName = worksheet.Cells[row, col].Text.Trim();
                        if (!string.IsNullOrEmpty(songName))
                        {
                            songNames.Add(songName);
                        }
                    }

                    if (songNames.Any())
                    {
                        albums.Add((albumName, songNames));
                    }
                }
            }

            return albums;
        }
    }
}