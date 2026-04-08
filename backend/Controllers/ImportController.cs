using backend.Dtos;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportController : ControllerBase
{
    private readonly AlbumImportService _importService;

    public ImportController(AlbumImportService importService)
    {
        _importService = importService;
    }

    /// <summary>
    /// Searches MusicBrainz for the best album match and returns it for user review.
    /// The returned candidate's ExternalId is a resolved MusicBrainz release ID.
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<AlbumSearchResultDto>> Search([FromBody] AlbumSearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ArtistName) || string.IsNullOrWhiteSpace(request.AlbumName))
            return BadRequest(new { message = "Artist name and album name are required." });

        var result = await _importService.SearchAlbumAsync(request);

        if (result.BestMatch.ExternalId == string.Empty)
            return NotFound(new { message = "No matching album found. Try adjusting your search terms." });

        return Ok(result);
    }

    /// <summary>
    /// Saves the confirmed album candidate to the library.
    /// Creates the artist (if new), album, and track listing.
    /// </summary>
    [HttpPost("save")]
    public async Task<IActionResult> Save([FromBody] SaveAlbumRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Candidate.ExternalId))
            return BadRequest(new { message = "A valid candidate must be selected before saving." });

        var album = await _importService.SaveAlbumAsync(request);
        return Ok(album);
    }
}
