import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSelectModule } from '@angular/material/select';
import { AlbumImportService } from '../../services/album-import.service';
import { AlertService } from '../../services/alert.service';
import {
  IAlbumCandidate,
  IAlbumSearchResult,
  ISaveAlbumRequest,
} from '../../../interfaces/AlbumImport';
import { Genre } from '../../../enums/Genre';

@Component({
  selector: 'app-add-album',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatDividerModule,
    MatSelectModule,
  ],
  templateUrl: './add-album.html',
  styleUrl: './add-album.css',
})
export class AddAlbum {
  searchForm: FormGroup;

  // --- State ---
  loading = false;
  searchSubmitted = false;
  searchResult: IAlbumSearchResult | null = null;
  selectedCandidate: IAlbumCandidate | null = null;
  saveInProgress = false;
  saveSuccess = false;
  errorMessage: string | null = null;

  readonly genres = [
    { value: Genre.Country, label: 'Country' },
    { value: Genre.Rock, label: 'Rock' },
    { value: Genre.Folk, label: 'Folk' },
    { value: Genre.Bluegrass, label: 'Bluegrass' },
  ];

  selectedGenre: Genre | null = null;

  constructor(
    private fb: FormBuilder,
    private albumImportService: AlbumImportService,
    private alertService: AlertService
  ) {
    this.searchForm = this.fb.group({
      artistName: ['', [Validators.required, Validators.minLength(1)]],
      albumName: ['', [Validators.required, Validators.minLength(1)]],
    });
  }

  get artistName() {
    return this.searchForm.get('artistName');
  }

  get albumName() {
    return this.searchForm.get('albumName');
  }

  onSearch(): void {
    if (this.searchForm.invalid) {
      this.searchForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.searchSubmitted = true;
    this.searchResult = null;
    this.selectedCandidate = null;
    this.saveSuccess = false;
    this.errorMessage = null;

    const request = {
      artistName: this.artistName?.value.trim(),
      albumName: this.albumName?.value.trim(),
    };

    this.albumImportService.searchAlbum(request).subscribe({
      next: (result) => {
        this.searchResult = result;
        this.selectedCandidate = result.bestMatch;
        this.loading = false;
      },
      error: (err) => {
        console.error('Search failed:', err);
        this.errorMessage = 'Search failed. Please check your input and try again.';
        this.loading = false;
      },
    });
  }

  onSelectCandidate(candidate: IAlbumCandidate): void {
    this.selectedCandidate = candidate;
    this.saveSuccess = false;
    this.errorMessage = null;
  }

  onSave(): void {
    if (!this.selectedCandidate) return;

    this.saveInProgress = true;
    this.errorMessage = null;

    const request: ISaveAlbumRequest = {
      candidate: this.selectedCandidate,
      genre: this.selectedGenre,
    };

    this.albumImportService.saveAlbum(request).subscribe({
      next: () => {
        this.saveSuccess = true;
        this.saveInProgress = false;
        this.alertService.success(`"${this.selectedCandidate?.title}" added to your library!`);
        // TODO: Navigate to album detail or dashboard after a short delay.
        //       e.g. this.router.navigate(['/albums']);
      },
      error: (err) => {
        console.error('Save failed:', err);
        this.errorMessage = 'Could not save the album. Please try again.';
        this.alertService.error('Could not save the album. Please try again.');
        this.saveInProgress = false;
      },
    });
  }

  onReset(): void {
    this.searchForm.reset();
    this.searchSubmitted = false;
    this.searchResult = null;
    this.selectedCandidate = null;
    this.saveSuccess = false;
    this.saveInProgress = false;
    this.errorMessage = null;
  }

  /** Converts milliseconds to an m:ss display string. */
  formatDuration(ms: number): string {
    const totalSeconds = Math.round(ms / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }
}
