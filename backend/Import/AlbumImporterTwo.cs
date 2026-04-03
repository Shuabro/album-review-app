using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using backend.Models;
using backend.Data;

namespace backend.Import
{
    public class AlbumImporterTwo
    {
        private readonly AppDbContext _dbContext;

        public AlbumImporterTwo(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Step 1: Load all songs and prepare normalized lookup
        public Dictionary<string, Song> LoadAndNormalizeSongs()
        {
            var songs = _dbContext.Songs.ToList();
            var normalizedSongLookup = songs
                .GroupBy(song => NormalizeTitle(song.Title))
                .ToDictionary(
                    group => group.Key,
                    group => group.First() // Take first song if there are duplicates
                );
            return normalizedSongLookup;
        }

        // Load all albums and prepare normalized lookup
        public Dictionary<string, Album> LoadAndNormalizeAlbums()
        {
            var albums = _dbContext.Albums.ToList();
            var normalizedAlbumLookup = albums
                .GroupBy(album => NormalizeTitle(album.Title))
                .ToDictionary(
                    group => group.Key,
                    group => group.First() // Take first album if there are duplicates
                );
            return normalizedAlbumLookup;
        }

        // Helper: Normalize song title
        public static string NormalizeTitle(string title)
        {
            var normalized = title.ToLowerInvariant();
            normalized = Regex.Replace(normalized, @"[^\w\s]", "");
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            return normalized;
        }
        // Step 2: Normalize incoming song titles from import data
        public List<string> NormalizeImportTitles(IEnumerable<string> importTitles)
        {
            return importTitles
                .Select(title => NormalizeTitle(title))
                .ToList();
        }

        // Match album names to database albums
        public Album? FuzzyMatchAlbum(string albumName, Dictionary<string, Album> normalizedAlbumLookup, int threshold = 90)
        {
            var normalizedAlbumName = NormalizeTitle(albumName);
            
            string? bestMatch = null;
            int bestScore = 0;
            Album? matchedAlbum = null;

            foreach (var dbEntry in normalizedAlbumLookup)
            {
                int score = FuzzySharp.Fuzz.Ratio(normalizedAlbumName, dbEntry.Key);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = dbEntry.Key;
                    matchedAlbum = dbEntry.Value;
                }
            }

            return bestScore >= threshold ? matchedAlbum : null;
        }

        // Step 3: Fuzzy match import titles to database songs
        public class MatchResult
        {
            public required string ImportTitle { get; set; }
            public string? MatchedDbTitle { get; set; }
            public int Score { get; set; }
            public Song? MatchedSong { get; set; }
            public Album? MatchedAlbum { get; set; }
        }

        public List<MatchResult> FuzzyMatchImportTitles(
            IEnumerable<string> importTitles,
            Dictionary<string, Song> normalizedSongLookup,
            Album? matchedAlbum = null,
            int threshold = 90)
        {
            var results = new List<MatchResult>();

            foreach (var importTitle in importTitles)
            {
                string? bestMatch = null;
                int bestScore = 0;
                Song? matchedSong = null;

                foreach (var dbEntry in normalizedSongLookup)
                {
                    int score = FuzzySharp.Fuzz.Ratio(importTitle, dbEntry.Key);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = dbEntry.Key;
                        matchedSong = dbEntry.Value;
                    }
                }

                // Prioritize songs from the matched album if available
                if (matchedAlbum != null)
                {
                    var albumSongs = normalizedSongLookup.Values.Where(s => s.AlbumId == matchedAlbum.Id);
                    foreach (var song in albumSongs)
                    {
                        var normalizedSongTitle = NormalizeTitle(song.Title);
                        int score = FuzzySharp.Fuzz.Ratio(importTitle, normalizedSongTitle);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMatch = normalizedSongTitle;
                            matchedSong = song;
                        }
                    }
                }

                if (bestScore >= threshold)
                {
                    results.Add(new MatchResult
                    {
                        ImportTitle = importTitle,
                        MatchedDbTitle = matchedSong?.Title,
                        Score = bestScore,
                        MatchedSong = matchedSong,
                        MatchedAlbum = matchedAlbum
                    });
                }
                else
                {
                    results.Add(new MatchResult
                    {
                        ImportTitle = importTitle,
                        MatchedDbTitle = null,
                        Score = bestScore,
                        MatchedSong = null,
                        MatchedAlbum = matchedAlbum
                    });
                }
            }

            return results;
        }

        // New method: Process single album from spreadsheet
        public class AlbumProcessResult
        {
            public required string AlbumName { get; set; }
            public Album? MatchedAlbum { get; set; }
            public List<MatchResult> SongMatches { get; set; } = new();
            public int MatchedSongsCount => SongMatches.Count(m => m.MatchedSong != null);
            public int TotalSongsCount => SongMatches.Count;
            public double MatchPercentage => TotalSongsCount > 0 ? (MatchedSongsCount * 100.0) / TotalSongsCount : 0;
        }

        public AlbumProcessResult ProcessAlbumFromSpreadsheet(string albumName, List<string> rankedSongs, string userId)
        {
            var result = new AlbumProcessResult { AlbumName = albumName };

            // Step 1: Find the album in database
            var normalizedAlbumLookup = LoadAndNormalizeAlbums();
            var matchedAlbum = FuzzyMatchAlbum(albumName, normalizedAlbumLookup);
            result.MatchedAlbum = matchedAlbum;

            if (matchedAlbum == null)
            {
                Console.WriteLine($"❌ Album '{albumName}' not found in database");
                return result;
            }

            Console.WriteLine($"✅ Album Match: '{albumName}' → '{matchedAlbum.Title}'");

            // Step 2: Get all songs for this album from database
            var albumSongs = _dbContext.Songs.Where(s => s.AlbumId == matchedAlbum.Id).ToList();
            var normalizedAlbumSongs = albumSongs
                .GroupBy(song => NormalizeTitle(song.Title))
                .ToDictionary(
                    group => group.Key,
                    group => group.First()
                );

            // Step 3: Fuzzy match each ranked song from spreadsheet
            for (int i = 0; i < rankedSongs.Count; i++)
            {
                var importTitle = rankedSongs[i];
                var normalizedImportTitle = NormalizeTitle(importTitle);
                
                string? bestMatch = null;
                int bestScore = 0;
                Song? matchedSong = null;

                // Only check songs from this specific album
                foreach (var dbEntry in normalizedAlbumSongs)
                {
                    int score = FuzzySharp.Fuzz.Ratio(normalizedImportTitle, dbEntry.Key);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = dbEntry.Key;
                        matchedSong = dbEntry.Value;
                    }
                }

                // Determine if song should be matched based on score and user input
                Song? finalMatchedSong = null;
                if (bestScore >= 80)
                {
                    // Auto-accept high confidence matches
                    finalMatchedSong = matchedSong;
                    Console.WriteLine($"  ✅ #{i + 1}: '{importTitle}' → '{matchedSong?.Title}' ({bestScore}%)");
                }
                else if (bestScore >= 40 && matchedSong != null)
                {
                    // Prompt user for low confidence matches
                    Console.Write($"  ❓ #{i + 1}: '{importTitle}' → '{matchedSong.Title}' ({bestScore}%) - Accept? (Y/n): ");
                    var userInput = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    
                    if (userInput == 'Y' || userInput == 'y' || userInput == '\r')
                    {
                        finalMatchedSong = matchedSong;
                        Console.WriteLine($"     ✅ User approved match");
                    }
                    else
                    {
                        Console.WriteLine($"     ❌ User rejected match");
                    }
                }
                else
                {
                    // Auto-reject very low matches
                    Console.WriteLine($"  ❌ #{i + 1}: '{importTitle}' - No good match found (best: {bestScore}%)");
                }

                var matchResult = new MatchResult
                {
                    ImportTitle = importTitle,
                    MatchedDbTitle = finalMatchedSong?.Title,
                    Score = bestScore,
                    MatchedSong = finalMatchedSong,
                    MatchedAlbum = matchedAlbum
                };

                result.SongMatches.Add(matchResult);

                // Step 4: Save ranking if song matched
                if (finalMatchedSong != null)
                {
                    var ranking = new UserSongRanking
                    {
                        UserId = userId,
                        SongId = finalMatchedSong.Id,
                        AlbumId = matchedAlbum.Id,
                        Rank = i + 1 // Spreadsheet rank (1-based)
                    };
                    _dbContext.UserSongRankings.Add(ranking);
                }
            }

            return result;
        }
    }
}
