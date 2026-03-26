using backend.DataAccess;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumController : ControllerBase
    {
        private readonly AlbumDataAccess _albumDataAccess;
        public AlbumController(AlbumDataAccess albumDataAccess)
        {
            _albumDataAccess = albumDataAccess;
        }

        [HttpGet]
        public async Task<ActionResult<List<Album>>> GetAllAlbums()
        {
            var albums = await _albumDataAccess.GetAllAlbumsAsync();
            return Ok(albums);
        }

        // [HttpGet("{id}")]
        // public async Task<ActionResult<Album>> GetAlbumById(int id)
        // {
        //     var album = await _albumDataAccess.GetAlbumByIdAsync(id);
        //     if (album == null)
        //         return NotFound();
        //     return Ok(album);
        // }
    }
}