	using backend.Models;

	namespace backend.DataAccess
	{
		public class AlbumDataAccess : BaseDataAccess
		{
			public AlbumDataAccess(IConfiguration configuration) : base(configuration) { }

		public async Task<List<Album>> GetAllAlbumsAsync()
		{
			var albums = new List<Album>();
			using (var conn = GetSqlConnection())
			{
				await conn.OpenAsync();
				var sql = @"
					SELECT
						a.Id,
						a.Title,
						a.ArtistId,
						a.ReleaseYear,
						a.CoverImageUrl,
						a.Rating,
						a.ReviewCount,
						a.Genre,
						a.CreatedAt,
						ar.Name AS ArtistName
					FROM Albums a
					LEFT JOIN Artists ar ON ar.Id = a.ArtistId";
				using (var cmd = GetSqlCommand(sql, conn))
				using (var reader = await cmd.ExecuteReaderAsync())
				{
					while (await reader.ReadAsync())
					{
						albums.Add(new Album
						{
							Id = reader.GetInt32(0),
							Title = reader.GetString(1),
							ArtistId = reader.GetInt32(2),
							ReleaseYear = reader.IsDBNull(3) ? null : reader.GetInt32(3),
							CoverImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
							Rating = reader.GetInt32(5),
							ReviewCount = reader.GetInt32(6),
							Genre = reader.IsDBNull(7) ? null : (backend.Enums.Genre?)reader.GetInt32(7),
							CreatedAt = reader.GetDateTime(8),
							Artist = reader.IsDBNull(9)
								? null
								: new Artist
								{
									Id = reader.GetInt32(2),
									Name = reader.GetString(9)
								}
						});
					}
				}
			}
			return albums;
		}
		}
	}
