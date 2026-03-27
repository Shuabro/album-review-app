export interface IAlbum {
  id: number;
  title: string;
  artistId: number;
  releaseYear?: number | null;
  coverImageUrl?: string | null;
  rating: number;
  reviewCount: number;
  genre?: number | null;
  createdAt: string;
  artist?: {
    id: number;
    name: string;
  } | null;
}