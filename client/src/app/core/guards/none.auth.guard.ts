import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { AuthService } from '../services/auth.service';

export const NoneAuthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  return true;
  if (!authService.getAccessToken()) {
    return true;
  }
  // Rediriger vers Microsoft login
  return false;
};
