import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IAlbum } from '../../interfaces/Album';
import { catchError, Subject, throwError } from 'rxjs';
import { AppOptionsService } from './app-options.service';

@Injectable({
  providedIn: 'root',
})
export class AlbumService {
  $albums= new Subject<IAlbum[]>();

  constructor(
    private http: HttpClient,
    private appOptionsService: AppOptionsService
  ) {}

  private GetAlbums() {
    let url = `${this.appOptionsService.getApiUrl()}/Album`;
    return this.http.get<IAlbum[]>(url).pipe(
      catchError((error) => {
        console.error('Error fetching albums:', error);
        // Transform the error into a user-friendly message
        return throwError(() => new Error('Failed to fetch albums. Please try again later.'));
      })
    );
  }

  getAlbums() {
    const observable = this.GetAlbums().subscribe(
      (albums: IAlbum[]) => {
        observable.unsubscribe();
        console.log('Albums fetched successfully:', albums);
        if (albums && albums.length > 0) {
          this.$albums.next(albums);  
        }
      }
    )
  }
}
