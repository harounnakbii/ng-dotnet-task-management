import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { RippleModule } from 'primeng/ripple';
import { AppFloatingConfigurator } from '../../../../layout/component/app.floatingconfigurator';
import { AuthService } from '@/core/services/auth.service';
import { ErrorMessages } from '@/shared/Models/ErrorMessages';
import { CommonModule } from '@angular/common';
import { MessageModule } from 'primeng/message';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [ButtonModule,
       CheckboxModule,
        InputTextModule,
         PasswordModule,
          RouterModule,
           RippleModule,
            AppFloatingConfigurator,
             ReactiveFormsModule,
             MessageModule,
              CommonModule],
    templateUrl: './login.component.html',
})
export class Login implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loginForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  showPassword = false;

  constructor() {
    this.loginForm = this.fb.group({
      username: ['', [Validators.required]],
      password: ['', [Validators.required]],
      rememberMe: [false]
    });
  }

  ngOnInit(): void {
    this.authService.isAuthenticated$.subscribe(isAuth => {
      if (isAuth) {
      //  this.router.navigate(['/']);
      }
    });
  }

  onSubmit() {
    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const credentials = {
      username: this.loginForm.value.username,
      password: this.loginForm.value.password
    };

    this.authService.login(credentials).subscribe({
      next: (success) => {
        this.isLoading = false;
        if (success) {
          this.router.navigate(['/']);
        } else {
          this.errorMessage = 'Invalid username or password';
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'Login failed. Please try again.';
        console.error('Login error:', error);
      }
    });
  }

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  get username() {
    return this.loginForm.get('username');
  }

  get password() {
    return this.loginForm.get('password');
  }

    getFieldErrorMessage(fieldName: string): string {
      const field = this.loginForm.get(fieldName);
      if (!field?.errors) {
        return '';
      }
      // Traiter les erreurs par priorité
      const errorTypes = Object.keys(field.errors);
  
      for (const errorType of errorTypes) {
        const message: string = errorType === 'required'
          ? (this.errorMessages[errorType] as any)[fieldName] as string
          : this.errorMessages[errorType] as string;
  
        if (message) {
          return message;
        }
      }
  
      // Message par défaut si aucune erreur spécifique trouvée
      return this.defaultMessages.generic;
    }
  
  
  
    // Méthodes de gestion des erreurs
    isFieldInvalid(fieldName: string): boolean {
      const field = this.loginForm.get(fieldName);
      return !!(field && field.invalid && (field.dirty || field.touched));
    }
  
    private readonly errorMessages: ErrorMessages  = {
      required: {
        username: 'The username is required',
        password: 'The password is required'
      }
    };
  
    private readonly defaultMessages = {
      required: 'This field is required',
      generic: 'This field contains some errors'
    };
  
}