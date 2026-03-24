import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    CommonModule
  ],
  templateUrl: './home.html',
  styleUrls: ['./home.css'],
})
export class Home {
  dashboardTitle = 'Dashboard';
  dashboardSubtitle = 'Your country album review hub';

  nextReview = {
    cover: 'https://via.placeholder.com/120',
    title: 'Golden Hour',
    artist: 'Kacey Musgraves',
    rating: 9.5,
    reviewDate: '2026-03-25',
  };

  recentlyReviewed = [
    {
      cover: 'https://via.placeholder.com/80',
      title: 'Traveller',
      artist: 'Chris Stapleton',
      rating: 9.2,
    },
    {
      cover: 'https://via.placeholder.com/80',
      title: 'Red',
      artist: 'Taylor Swift',
      rating: 8.8,
    },
    {
      cover: 'https://via.placeholder.com/80',
      title: 'Fearless',
      artist: 'Taylor Swift',
      rating: 8.5,
    },
  ];

  quickStats = [
    { label: 'Total Albums', value: 42, icon: 'album' },
    { label: 'Avg. Score', value: 8.7, icon: 'star' },
    { label: 'Top Artist', value: 'Chris Stapleton', icon: 'person' },
    { label: 'Best Album', value: 'Golden Hour', icon: 'emoji_events' },
  ];

  latestNotes = [
    { album: 'Red', note: 'Great storytelling and production.' },
    { album: 'Traveller', note: 'Raw, soulful vocals.' },
    { album: 'Golden Hour', note: 'Dreamy, modern country-pop.' },
  ];
}
