import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';  
import { environment } from 'src/environments/environment';
import { RegisterDto, RegisterResponse } from '@/shared/Models/user.model';

@Injectable({
  providedIn: 'root'
})
export class RegisterService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/users`;

  /**
   * Register a new user
   * @param registerDto User registration data
   * @returns Observable<RegisterResponse>
   */
  register(registerDto: RegisterDto): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/register`, registerDto)
      .pipe(
        map(response => {
          console.log('User registered successfully:', response);
          return response;
        }),
        catchError(this.handleError)
      );
  }

  /**
   * Handle HTTP errors
   * @param error HttpErrorResponse
   * @returns Observable<never>
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An error occurred during registration';

    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Server-side error
      if (error.status === 400 && error.error?.message) {
        errorMessage = error.error.message;
      } else if (error.status === 409) {
        errorMessage = 'Username or email already exists';
      } else if (error.status === 0) {
        errorMessage = 'Unable to connect to the server. Please check your connection.';
      } else {
        errorMessage = `Server error: ${error.status} - ${error.message}`;
      }
    }

    console.error('Registration error:', error);
    return throwError(() => new Error(errorMessage));
  }
}