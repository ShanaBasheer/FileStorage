import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { authGuard } from './auth.guard';

export const routes: Routes = [

  // LOGIN PAGE
  {
    path: 'login',
    component: LoginComponent
  },

  // HOME (Protected)
  {
    path: 'home',
    loadComponent: () =>
      import('./home/home.component').then(m => m.HomeComponent),
    canActivate: [authGuard]
  },

  // FILE LIST (Protected)
  {
    path: 'file-list',
    loadComponent: () =>
      import('./file-list/file-list.component').then(m => m.FileListComponent),
    canActivate: [authGuard],
    runGuardsAndResolvers: 'always'
  },

  // UPLOAD (Protected)
  {
    path: 'upload',
    loadComponent: () =>
      import('./upload/upload.component').then(m => m.UploadComponent),
    canActivate: [authGuard],
    runGuardsAndResolvers: 'always'
  },

  // DEFAULT ROUTE → HOME (NOT LOGIN)
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full'
  },

  // WILDCARD → HOME
  {
    path: '**',
    redirectTo: 'home'
  }
];
