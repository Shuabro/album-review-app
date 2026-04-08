import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AppOptionsService } from './app-options.service';
import {
  IAlbumSearchRequest,
  IAlbumSearchResult,
  ISaveAlbumRequest,
} from '../../interfaces/AlbumImport';
import { IAlbum } from '../../interfaces/Album';

@Injectable({
  providedIn: 'root',
})
export class AlbumImportService {
  constructor(
    private http: HttpClient,
    private appOptionsService: AppOptionsService
  ) {}

  /** Search for an album by artist name and album name. */
  searchAlbum(request: IAlbumSearchRequest): Observable<IAlbumSearchResult> {
    const url = `${this.appOptionsService.getApiUrl()}/import/search`;
    return this.http.post<IAlbumSearchResult>(url, request);
  }

  /** Save the confirmed album candidate to the library. */
  saveAlbum(request: ISaveAlbumRequest): Observable<IAlbum> {
    const url = `${this.appOptionsService.getApiUrl()}/import/save`;
    return this.http.post<IAlbum>(url, request);
  }
}
