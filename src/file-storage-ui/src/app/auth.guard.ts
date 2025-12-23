import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (typeof window === 'undefined') {
    return false;
  }

  const token = auth.getToken();

  if (token) {
    return true;
  }

  // No token → redirect to login
  router.navigate(['/login']);
  return false;
};
