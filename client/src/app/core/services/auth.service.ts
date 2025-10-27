import { Injectable, inject } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { map, catchError } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Router } from '@angular/router';

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface TokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  refresh_token?: string;
  scope: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private oauthService = inject(OAuthService);
  private router = inject(Router);

  private isAuthenticatedSubject$ = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject$.asObservable();

  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_INFO_KEY = 'user_info';

  constructor() {
    this.configure();
    this.checkAuthentication();
  }

  private configure() {
    const authConfig = environment.oidc;
    this.oauthService.configure(authConfig);
  }

  private checkAuthentication() {
    const token = this.getAccessToken();
    if (token) {
      this.isAuthenticatedSubject$.next(true);
    }
  }

  login(credentials: LoginCredentials): Observable<boolean> {
    const authConfig = environment.oidc;
    const body = new URLSearchParams();
    body.set('grant_type', 'password');
    body.set('username', credentials.username);
    body.set('password', credentials.password);
    body.set('client_id', authConfig.clientId!);
    body.set('scope', authConfig.scope!);

    const headers = new HttpHeaders({
      'Content-Type': 'application/x-www-form-urlencoded'
    });

    return this.http.post<TokenResponse>(
      authConfig.tokenEndpoint!,
      body.toString(),
      { headers }
    ).pipe(
      map(response => {
        this.storeTokens(response);
        this.isAuthenticatedSubject$.next(true);
        this.loadUserInfo();
        return true;
      }),
      catchError(error => {
        console.error('Login failed', error);
        this.isAuthenticatedSubject$.next(false);
        return of(false);
      })
    );
  }

  private storeTokens(response: TokenResponse) {
    localStorage.setItem(this.TOKEN_KEY, response.access_token);
    if (response.refresh_token) {
      localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refresh_token);
    }
  }

  private loadUserInfo() {
    const authConfig = environment.oidc;
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${this.getAccessToken()}`
    });

    this.http.get(`${authConfig.issuer}/connect/userinfo`, { headers })
      .subscribe({
        next: (userInfo) => {
          localStorage.setItem(this.USER_INFO_KEY, JSON.stringify(userInfo));
        },
        error: (error) => console.error('Failed to load user info', error)
      });
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_INFO_KEY);
    this.isAuthenticatedSubject$.next(false);
    this.router.navigate(['/auth/login']);
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getUserInfo(): any {
    const userInfo = localStorage.getItem(this.USER_INFO_KEY);
    return userInfo ? JSON.parse(userInfo) : null;
  }

  hasValidToken(): boolean {
    return !!this.getAccessToken();
  }

  refreshToken(): Observable<boolean> {
    const authConfig = environment.oidc;
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return of(false);
    }

    const body = new URLSearchParams();
    body.set('grant_type', 'refresh_token');
    body.set('refresh_token', refreshToken);
    body.set('client_id', authConfig.clientId!);

    const headers = new HttpHeaders({
      'Content-Type': 'application/x-www-form-urlencoded'
    });

    return this.http.post<TokenResponse>(
      authConfig.tokenEndpoint!,
      body.toString(),
      { headers }
    ).pipe(
      map(response => {
        this.storeTokens(response);
        this.isAuthenticatedSubject$.next(true);
        return true;
      }),
      catchError(error => {
        console.error('Token refresh failed', error);
        this.logout();
        return of(false);
      })
    );
  }
}