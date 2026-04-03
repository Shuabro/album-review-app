using backend.Models;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.DataAccess
{
	public class AlbumDataAccess
	{
		private readonly AppDbContext _dbContext;
		public AlbumDataAccess(AppDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<List<Album>> GetAlbumsWithArtistAsync()
		{
			return await _dbContext.Albums
				.Include(a => a.Artist)
				.ToListAsync();
		}
	}
}
