import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { AuthService } from '../services/auth.service';

export const AuthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  if (authService.getAccessToken()) {
    return true;
  }

  // Sauvegarder l’URL pour y revenir après login
  sessionStorage.setItem('redirectAfterLogin', window.location.pathname);

  // Rediriger vers Microsoft login
  return false;
};
