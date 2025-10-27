import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { AuthService } from '../services/auth.service';

export const NoneAuthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router)
 
  if (!authService.getAccessToken()) {
    return true;
  }
  
  router.navigate(['/']);

  return false;
};
