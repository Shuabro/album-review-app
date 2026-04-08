namespace backend.Dtos;

// ---------------------------------------------------------------------------
// Inbound — what the frontend sends to the backend
// ---------------------------------------------------------------------------

/// <summary>
/// Mirrors the frontend's IAlbumSearchRequest.
/// Received by POST /api/import/search.
/// </summary>
public class AlbumSearchRequest
{
    public string ArtistName { get; set; } = string.Empty;
    public string AlbumName { get; set; } = string.Empty;
}

/// <summary>
/// Mirrors the frontend's ISaveAlbumRequest.
/// Received by POST /api/import/save.
/// </summary>
public class SaveAlbumRequest
{
    public AlbumCandidateDto Candidate { get; set; } = new();
    public int? Genre { get; set; }
}

// ---------------------------------------------------------------------------
// Outbound — what the backend sends back to the frontend
// ---------------------------------------------------------------------------

/// <summary>
/// Mirrors the frontend's ITrack.
/// </summary>
public class TrackDto
{
    public int Position { get; set; }
    public string Title { get; set; } = string.Empty;
    /// <summary>Duration in milliseconds. Null if MusicBrainz doesn't have it.</summary>
    public int? DurationMs { get; set; }
}

/// <summary>
/// Mirrors the frontend's IAlbumCandidate.
/// Represents one search result — a resolved MusicBrainz release.
/// </summary>
public class AlbumCandidateDto
{
    /// <summary>MusicBrainz release ID (not release-group ID).</summary>
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public int? ReleaseYear { get; set; }
    /// <summary>Full ISO date string e.g. "1990-02-27". More precise than ReleaseYear alone.</summary>
    public string? ReleaseDate { get; set; }
    public string? Country { get; set; }
    public string? Format { get; set; }
    public string? CoverImageUrl { get; set; }
    /// <summary>FuzzySharp-derived confidence score 0–100.</summary>
    public int? Score { get; set; }
    public int? TrackCount { get; set; }
    public string? Disambiguation { get; set; }
    public List<TrackDto> Tracks { get; set; } = [];
    /// <summary>
    /// True when ExternalId is a resolved MusicBrainz release ID.
    /// False for otherMatches candidates whose ExternalId is still a release-group ID.
    /// SaveAlbumAsync will resolve the release if this is false.
    /// </summary>
    public bool ReleaseResolved { get; set; }
}

/// <summary>
/// Mirrors the frontend's IAlbumSearchResult.
/// Returned by POST /api/import/search.
/// </summary>
public class AlbumSearchResultDto
{
    public AlbumCandidateDto BestMatch { get; set; } = new();
    /// <summary>
    /// Additional candidates ranked by score.
    /// TODO: populated when otherMatches support is added to the search flow.
    /// </summary>
    public List<AlbumCandidateDto> OtherMatches { get; set; } = [];
}
