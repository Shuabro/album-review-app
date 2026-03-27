import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AppOptionsService {
  private readonly albumApiUrl: string = '/api';

  getApiUrl(): string {
    return this.albumApiUrl;
  }
}