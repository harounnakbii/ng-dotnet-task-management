
// src/app/core/validators/async-validators.ts
import { Injectable, inject } from '@angular/core';
import { AbstractControl, AsyncValidatorFn, ValidationErrors } from '@angular/forms';
import { Observable, of, timer } from 'rxjs';
import { map, catchError, switchMap, first, distinctUntilChanged, debounceTime } from 'rxjs/operators';
import { UserValidationService } from '../services/user.validation.service';

@Injectable({
    providedIn: 'root'
})
export class UserValidator {
    private userValidationService = inject(UserValidationService);

    usernameAvailability(minLength: number = 3): AsyncValidatorFn {
        return (control: AbstractControl): Observable<ValidationErrors | null> => {
            if (!control.value || control.value.trim().length < minLength) {
                return of(null);
            }

            // Debounce de 500ms pour éviter trop de requêtes
            return of(control.value).pipe(
                debounceTime(800), // Attendre 800ms après la dernière frappe
                distinctUntilChanged(), // Ne vérifier que si la valeur a changé
                switchMap(() => this.userValidationService.checkUsernameAvailability(control.value)),
                map(isAvailable => isAvailable ? null : { usernameTaken: true }),
                catchError(() => of(null)),
                first()
            );
        };
    }

    emailAvailability(minLength: number = 3): AsyncValidatorFn {
        return (control: AbstractControl): Observable<ValidationErrors | null> => {
            if (!control.value || control.value.trim().length < minLength) {
                return of(null);
            }
            return timer(500).pipe(
                debounceTime(800), // Attendre 800ms après la dernière frappe
                distinctUntilChanged(), // Ne vérifier que si la valeur a changé
                switchMap(() => this.userValidationService.checkEmailAvailability(control.value)),
                map(isAvailable => isAvailable ? null : { emailTaken: true }),
                catchError(() => of(null)),
                first()
            );
        };
    }
}