import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService, User } from '../../services/auth.service';
import { Observable } from 'rxjs';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatDividerModule, RouterLink],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.css']
})
export class SidebarComponent {
  currentUser$: Observable<User | null>;
  sidebarOpen = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.currentUser$ = this.authService.currentUser$;
  }

  getCurrentUserName(): string {
    let userName = 'User';
    this.currentUser$.subscribe(user => {
      if (user?.firstName) {
        userName = user.firstName;
      }
    });
    return userName;
  }

  toggleSidebar() {
    this.sidebarOpen = !this.sidebarOpen;
  }

  closeSidebar() {
    this.sidebarOpen = false;
  }

  // Navigation actions
  addAlbum() {
    console.log('➕ Sidebar: Add Album clicked - would navigate to MusicBrainz search');
  }

  myReviews() {
    console.log('📝 Sidebar: My Reviews clicked - would show user reviews');
  }

  myTopAlbums() {
    console.log('⭐ Sidebar: My Top Albums clicked - would show top rated albums');
  }

  browseAlbums() {
    console.log('🎵 Sidebar: Browse Albums clicked - would show all albums');
  }

  search() {
    console.log('🔍 Sidebar: Search clicked - would show search page');
  }

  myStats() {
    console.log('📊 Sidebar: My Stats clicked - would show user statistics');
  }

  settings() {
    console.log('⚙️ Sidebar: Settings clicked - would show settings page');
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}