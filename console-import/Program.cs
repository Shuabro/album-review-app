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

            // Get the second user from the database  
            var secondUser = dbContext.Users.Skip(1).FirstOrDefault();
            if (secondUser == null)
            {
                Console.WriteLine("Second user not found in database. Please ensure there are at least 2 users.");
                return;
            }
            string userId = secondUser.Id;

            Console.WriteLine($"Found {albums.Count} albums in spreadsheet.");
            Console.WriteLine($"Processing rankings for user: {secondUser.UserName}\n");

            var allProcessedSongs = new List<string>();
            var allMatchedSongs = new List<string>();
            var allUnmatchedSongs = new List<string>();

            try
            {
                int albumCount = 0;
                foreach (var album in albums)
                {
                    albumCount++;
                    Console.WriteLine($"=== ALBUM {albumCount}/{albums.Count}: {album.AlbumName} ===");
                    
                    // Process this single album
                    var result = importer.ProcessAlbumFromSpreadsheet(album.AlbumName, album.Songs, userId);
                    
                    // Track songs for final summary
                    foreach (var song in album.Songs)
                    {
                        allProcessedSongs.Add($"{album.AlbumName}: {song}");
                    }
                    foreach (var match in result.SongMatches)
                    {
                        if (match.MatchedSong != null)
                        {
                            allMatchedSongs.Add($"{album.AlbumName}: {match.ImportTitle} → {match.MatchedSong.Title}");
                        }
                        else
                        {
                            allUnmatchedSongs.Add($"{album.AlbumName}: {match.ImportTitle}");
                        }
                    }
                    
                    // Display results summary
                    if (result.MatchedAlbum != null)
                    {
                        Console.WriteLine($"\n📊 Final Results: {result.MatchedSongsCount}/{result.TotalSongsCount} songs matched ({result.MatchPercentage:F1}%)");
                    }
                    
                    Console.WriteLine($"\n--- Press any key to continue to next album ({albums.Count - albumCount} remaining) ---");
                    Console.ReadKey();
                    Console.WriteLine("\n");
                }

                // Check existing records before saving
                int existingCount = dbContext.UserSongRankings.Count(r => r.UserId == userId);
                Console.WriteLine($"\n📊 Before import: {existingCount} existing rankings for this user");

                // Save all changes to database
                Console.WriteLine("Saving all rankings to database...");
                int savedCount = dbContext.SaveChanges();
                
                int finalCount = dbContext.UserSongRankings.Count(r => r.UserId == userId);
                Console.WriteLine($"✅ Added {savedCount} new rankings to database!");
                Console.WriteLine($"📊 After import: {finalCount} total rankings for this user");
                
                // Check which songs from database don't have rankings
                var totalSongsInDb = dbContext.Songs.Count();
                var songsWithRankings = dbContext.UserSongRankings
                    .Where(r => r.UserId == userId)
                    .Select(r => r.SongId)
                    .ToHashSet();
                
                var songsWithoutRankings = dbContext.Songs
                    .Include(s => s.Album)
                    .Where(s => !songsWithRankings.Contains(s.Id))
                    .Select(s => new { s.Title, AlbumTitle = s.Album.Title })
                    .ToList();
                
                Console.WriteLine($"\n📀 Total songs in database: {totalSongsInDb}");
                Console.WriteLine($"🎵 Songs with rankings: {songsWithRankings.Count}");
                Console.WriteLine($"❓ Songs without rankings: {songsWithoutRankings.Count}");
                
                if (songsWithoutRankings.Any())
                {
                    Console.WriteLine($"\n🔍 SONGS WITHOUT RANKINGS ({songsWithoutRankings.Count}):");
                    foreach (var song in songsWithoutRankings)
                    {
                        Console.WriteLine($"  • {song.AlbumTitle}: {song.Title}");
                    }
                }
                
                // Final summary
                Console.WriteLine($"\n=== FINAL SUMMARY ===");
                Console.WriteLine($"📋 Total songs from spreadsheet: {allProcessedSongs.Count}");
                Console.WriteLine($"✅ Songs matched and saved: {allMatchedSongs.Count}");
                Console.WriteLine($"❌ Songs not matched: {allUnmatchedSongs.Count}");
                Console.WriteLine($"💾 Database records created: {savedCount}");
                
                if (allUnmatchedSongs.Any())
                {
                    Console.WriteLine($"\n🔍 UNMATCHED SONGS ({allUnmatchedSongs.Count}):");
                    foreach (var unmatched in allUnmatchedSongs)
                    {
                        Console.WriteLine($"  • {unmatched}");
                    }
                }
                
                if (allProcessedSongs.Count != savedCount)
                {
                    Console.WriteLine($"\n⚠️  MISMATCH: Expected {allProcessedSongs.Count} total, but saved {savedCount}");
                    Console.WriteLine($"   Missing: {allProcessedSongs.Count - savedCount} song(s)");
                }
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