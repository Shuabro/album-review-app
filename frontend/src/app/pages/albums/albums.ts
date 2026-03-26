import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

type Album = {
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
};

type AlbumSort = 'artist' | 'newest' | 'highestRated' ;

@Component({
  selector: 'app-albums',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './albums.html',
  styleUrl: './albums.css',
})
export class Albums implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/Album';

  albums: Album[] = [];
  isLoading = false;
  errorMessage = '';
  searchQuery = '';
  sortBy: AlbumSort = 'artist';

  ngOnInit(): void {
    this.loadAlbums();
  }

  get visibleAlbums(): Album[] {
    const query = this.searchQuery.trim().toLowerCase();

    let filtered = this.albums;
    if (query) {
      filtered = this.albums.filter((album) => {
        const title = album.title.toLowerCase();
        const artist = this.getArtistName(album).toLowerCase();
        return title.includes(query) || artist.includes(query);
      });
    }

    return [...filtered].sort((a, b) => {
      switch (this.sortBy) {
        case 'newest':
          return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
        case 'highestRated':
          return (b.rating ?? 0) - (a.rating ?? 0);
        case 'artist':
        default:
          return this.getArtistName(a).localeCompare(this.getArtistName(b));
      }
    });
  }

  loadAlbums(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.http.get<Album[]>(this.apiUrl).subscribe({
      next: (albums) => {
        this.albums = albums;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load albums right now. Please try again.';
        this.isLoading = false;
      },
    });
  }

  getArtistName(album: Album): string {
    return album.artist?.name?.trim() || 'Unknown artist';
  }

  getCoverUrl(path?: string | null): string | null {
    if (!path) {
      return null;
    }

    if (path.startsWith('http://') || path.startsWith('https://')) {
      return path;
    }

    const normalized = path.replace(/\\/g, '/');
    return normalized.startsWith('/') ? normalized : `/${normalized}`;
  }

  trackByAlbumId(_: number, album: Album): number {
    return album.id;
  }

  onAlbumOpen(album: Album): void {
    // Placeholder for route navigation to album detail.
    console.log('Open album', album.id);
  }

}
