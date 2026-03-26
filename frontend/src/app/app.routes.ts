import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Albums } from './pages/albums/albums';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'home', component: Home },
  { path: 'albums', component: Albums },
  { path: '**', redirectTo: '' }
];
