import { Genre } from '../enums/Genre';

/** A single track on an album candidate. */
export interface ITrack {
  position: number;
  title: string;
  /** Duration in milliseconds as returned by MusicBrainz. */
  durationMs?: number | null;
}

/** Request payload sent to the backend when searching for an album. */
export interface IAlbumSearchRequest {
  artistName: string;
  albumName: string;
}

/** A single candidate album returned from the search (MusicBrainz-style). */
export interface IAlbumCandidate {
  externalId: string;            // release MBID
  title: string;                 // release title
  artistName: string;
  releaseYear?: number | null;
  releaseDate?: string | null;   // helpful because MB often has full date
  country?: string | null;       // useful for disambiguation
  label?: string | null;         // useful for disambiguation
  format?: string | null;        // CD, Digital Media, etc.
  coverImageUrl?: string | null;
  score?: number | null;         // your backend-generated confidence
  trackCount?: number | null;
  disambiguation?: string | null;
  tracks?: ITrack[];
  /**
   * True when externalId is a resolved MusicBrainz release ID.
   * False for otherMatches where externalId is still a release-group ID.
   * The backend resolves it automatically during save if false.
   */
  releaseResolved?: boolean;
}

/** Full response from the album search endpoint. */
export interface IAlbumSearchResult {
  bestMatch: IAlbumCandidate;
  /** TODO: populate when the backend returns additional ranked candidates. */
  otherMatches?: IAlbumCandidate[];
}

/** Request payload sent to the backend when the user confirms saving an album. */
export interface ISaveAlbumRequest {
  candidate: IAlbumCandidate;
  genre?: Genre | null;
}
