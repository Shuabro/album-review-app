import { Routes } from '@angular/router';
import { Home } from './pages/home/home';
import { Albums } from './pages/albums/albums';
import { LoginComponent } from './pages/login/login';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'home', component: Home },
  { path: 'albums', component: Albums },
  { path: 'login', component: LoginComponent },
  { path: '**', redirectTo: '' }
];
