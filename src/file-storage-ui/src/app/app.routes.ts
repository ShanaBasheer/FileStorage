import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { authGuard } from './auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent
  },

  {
    path: 'file-list',
    loadComponent: () =>
      import('./file-list/file-list.component').then(m => m.FileListComponent),
    canActivate: [authGuard]
  },

  {
    path: 'upload',
    loadComponent: () =>
      import('./upload/upload.component').then(m => m.UploadComponent),
    canActivate: [authGuard]
  },

  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },

  {
    path: '**',
    redirectTo: 'login'
  }
  
];
