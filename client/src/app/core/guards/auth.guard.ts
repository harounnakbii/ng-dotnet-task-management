import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const AuthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.getAccessToken()) {
    return true;
  }

  // Sauvegarder l’URL pour y revenir après login
  sessionStorage.setItem('redirectAfterLogin', window.location.pathname);

  router.navigate(['/auth/login']);

  // Rediriger vers Microsoft login
  return false;
};
