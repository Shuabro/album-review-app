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
import { IAlbum } from '../../../interfaces/Album';
import { AlbumService } from '../../services/album-service';
import { Subscription } from 'rxjs';

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
  albumSubscription: Subscription | undefined;

  constructor(
              private albumService: AlbumService
             ) {} 

  albums: IAlbum[] = [];
  isLoading = false;
  errorMessage = '';
  searchQuery = '';
  sortBy: AlbumSort = 'artist';

  ngOnInit(): void {
    this.albumSubscription = this.albumService.$albums.subscribe({
      next: (albums: IAlbum[]) => {
        this.albums = albums;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.message || 'Failed to load albums.';
        this.isLoading = false;
      }
    });
    this.albumService.getAlbums();
  }
  
  ngOnDestroy(): void {
    this.albumSubscription?.unsubscribe();
  }

  get visibleAlbums(): IAlbum[] {
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

    this.albumService.getAlbums();
     
  }

  getArtistName(album: IAlbum): string {
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

  trackByAlbumId(_: number, album: IAlbum): number {
    return album.id;
  }

  onAlbumOpen(album: IAlbum): void {
    // Placeholder for route navigation to album detail.
    console.log('Open album', album.id);
  }

}
