import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError, delay } from 'rxjs/operators';
import { environment } from 'src/environments/environment';


@Injectable({
  providedIn: 'root'
})
export class UserValidationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/users`;

  checkUsernameAvailability(username: string): Observable<boolean> {
    return this.http.get<{ isAvailable: boolean }>(`${this.apiUrl}/check-username/${username}`)
      .pipe(
        map(response => response.isAvailable),
        catchError(() => of(true)) // En cas d'erreur, considérer comme disponible
      );
  }

  checkEmailAvailability(email: string): Observable<boolean> {
    return this.http.get<{ isAvailable: boolean }>(`${this.apiUrl}/check-email/${email}`)
      .pipe(
        map(response => response.isAvailable),
        catchError(() => of(true))
      );
  }
}