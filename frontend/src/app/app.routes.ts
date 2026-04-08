import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Albums } from './pages/albums/albums';
import { LoginComponent } from './pages/login/login';
import { AddAlbum } from './pages/add-album/add-album';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', component: Home, canActivate: [authGuard] },
  { path: 'home', component: Home, canActivate: [authGuard] },
  { path: 'albums', component: Albums, canActivate: [authGuard] },
  { path: 'albums/add', component: AddAlbum, canActivate: [authGuard] },
  { path: 'login', component: LoginComponent },
  { path: '**', redirectTo: '' }
];
