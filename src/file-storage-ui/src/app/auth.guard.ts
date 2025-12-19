import { Injectable } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.getToken();

  if (!token) {
    return router.parseUrl('/login');  // ✅ correct redirect
  }

  return true;
};


// export const authGuard: CanActivateFn = () => {
//   const auth = inject(AuthService);
//   const router = inject(Router);

//   const token = auth.getToken();
//   if (token) {
//     return true; // ✅ allow access
//   } else {
//     router.navigate(['/login']); // ✅ redirect if not logged in
//     return false;
//   }
// };
