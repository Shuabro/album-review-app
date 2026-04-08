using System.Text.Json.Serialization;

namespace backend.Dtos;

// ---------------------------------------------------------------------------
// Release-Group search  (GET /ws/2/release-group?query=...&fmt=json)
// Used in Step 1: search for the logical album
// ---------------------------------------------------------------------------

public class MbReleaseGroupSearchResponse
{
    [JsonPropertyName("release-groups")]
    public List<MbReleaseGroup> ReleaseGroups { get; set; } = [];
}

public class MbReleaseGroup
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>MusicBrainz relevance score 0–100.</summary>
    [JsonPropertyName("score")]
    public int Score { get; set; }

    [JsonPropertyName("primary-type")]
    public string? PrimaryType { get; set; }

    /// <summary>ISO date string, may be partial e.g. "2015" or "2015-05".</summary>
    [JsonPropertyName("first-release-date")]
    public string? FirstReleaseDate { get; set; }

    [JsonPropertyName("artist-credit")]
    public List<MbArtistCredit> ArtistCredit { get; set; } = [];
}

public class MbArtistCredit
{
    /// <summary>The name as it appears credited on this release (may differ from artist.name).</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("artist")]
    public MbArtist? Artist { get; set; }

    /// <summary>Join phrase between credits e.g. " &amp; ", " feat. ".</summary>
    [JsonPropertyName("joinphrase")]
    public string? JoinPhrase { get; set; }
}

public class MbArtist
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

// ---------------------------------------------------------------------------
// Release browse  (GET /ws/2/release?release-group={id}&inc=recordings&fmt=json)
// Used in Step 2: resolve the best pressing and get its track listing
// ---------------------------------------------------------------------------

public class MbReleaseSearchResponse
{
    [JsonPropertyName("releases")]
    public List<MbRelease> Releases { get; set; } = [];
}

public class MbRelease
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>ISO date string, may be partial.</summary>
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("disambiguation")]
    public string? Disambiguation { get; set; }

    /// <summary>Indicates whether cover art is registered for this release.</summary>
    [JsonPropertyName("cover-art-archive")]
    public MbCoverArtArchive? CoverArtArchive { get; set; }

    [JsonPropertyName("media")]
    public List<MbMedium> Media { get; set; } = [];

    /// <summary>Derived: total tracks across all media.</summary>
    [JsonIgnore]
    public int TotalTrackCount => Media.Sum(m => m.TrackCount);
}

public class MbCoverArtArchive
{
    /// <summary>True if a front cover image is registered on Cover Art Archive.</summary>
    [JsonPropertyName("front")]
    public bool Front { get; set; }

    [JsonPropertyName("artwork")]
    public bool Artwork { get; set; }
}

public class MbMedium
{
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("track-count")]
    public int TrackCount { get; set; }

    [JsonPropertyName("tracks")]
    public List<MbTrack> Tracks { get; set; } = [];
}

public class MbTrack
{
    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Duration in milliseconds. Null if MB doesn't have it.</summary>
    [JsonPropertyName("length")]
    public int? Length { get; set; }

    /// <summary>
    /// The recording linked to this track. Contains the MB recording ID,
    /// which will be needed for future songwriter lookup via the Works API.
    /// </summary>
    [JsonPropertyName("recording")]
    public MbRecording? Recording { get; set; }
}

public class MbRecording
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}
