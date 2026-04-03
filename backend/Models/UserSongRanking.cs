using backend.Models;
using Microsoft.AspNetCore.Identity;

namespace backend.Models
{
    public class UserSongRanking
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // FK to Identity user
        public int SongId { get; set; }
        public int AlbumId { get; set; } // New property for direct album reference
        public int Rank { get; set; }

        public ApplicationUser? User { get; set; } // Navigation property
        public Song? Song { get; set; } // Navigation property
        public Album? Album { get; set; } // Navigation property
    }
}
