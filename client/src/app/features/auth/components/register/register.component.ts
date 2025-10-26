import { Component, inject, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { RippleModule } from 'primeng/ripple';
import { AppFloatingConfigurator } from '../../../../layout/component/app.floatingconfigurator';
import { AuthService } from '@/core/services/auth.service';
import { CommonModule } from '@angular/common';
import { MessageModule } from 'primeng/message';
import { UserValidator } from '../../validators/user.validator';
import { RegisterService } from '../../services/user.register.service';
import { RegisterDto } from '@/shared/Models/user.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    ButtonModule,
    CheckboxModule,
    InputTextModule,
    PasswordModule,
    RippleModule,
    AppFloatingConfigurator,
    ReactiveFormsModule,
    CommonModule,
    MessageModule
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class Register implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private asyncValidators = inject(UserValidator);
  private registerService = inject(RegisterService);

  registerForm: FormGroup;
  isLoading = false;
  successMessage = '';
  errorMessage = '';
  showPassword = false;

  constructor() {
    const USERNAME_MIN_LENGTH = 3;
    const EMAIL_MIN_LENGTH = 3;

    this.registerForm = this.fb.group({
      name: ['',
        {
          validators: [
            Validators.required,
            Validators.minLength(USERNAME_MIN_LENGTH),
            Validators.maxLength(50),
            this.noWhitespaceValidator()
          ],
          asyncValidators: [this.asyncValidators.usernameAvailability(USERNAME_MIN_LENGTH)],
          updateOn: 'blur' // Valider uniquement quand l'utilisateur quitte le champ
        }
      ],
      email: ['',
        {
          validators: [
            Validators.required,
            Validators.email,
            this.emailValidator
          ],
          asyncValidators: [this.asyncValidators.emailAvailability(EMAIL_MIN_LENGTH)],
          updateOn: 'blur'
        }
      ],
      confirmEmail: ['', [
        Validators.required,
        Validators.email
      ]],
      password: ['', [
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(100),
        this.passwordStrengthValidator()
      ]],
      confirmPassword: ['', [
        Validators.required
      ]],
      rememberMe: [false]
    }, {
      validators: [
        this.emailMatchValidator(),
        this.passwordMatchValidator()
      ]
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
    // Reset messages
    this.errorMessage = '';
    this.successMessage = '';

    // Validate form
    if (this.registerForm.invalid) {
      Object.keys(this.registerForm.controls).forEach(key => {
        this.registerForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.isLoading = true;

    // Prepare registration data
    const registerDto: RegisterDto = {
      name: this.registerForm.value.name,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password,
      confirmPassword: this.registerForm.value.confirmPassword
    };

    // Call registration service
    this.registerService.register(registerDto).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.successMessage = 'Registration successful! Redirecting to login...';
        
        console.log('User registered:', response);

        // Optionally, auto-login the user
        // Or redirect to login page after a delay
        setTimeout(() => {
          this.router.navigate(['/auth/login'], {
            queryParams: { registered: 'true', username: response.name }
          });
        }, 2000);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.message || 'Registration failed. Please try again.';
        console.error('Registration error:', error);
      }
    });
  }

  getFieldErrorMessage(fieldName: string): string {
    const field = this.registerForm.get(fieldName);

    if (!field?.errors) {
      if (fieldName === 'confirmEmail' && this.registerForm.errors?.['emailMismatch']) {
        return this.errorMessages.emailMismatch;
      }
      if (fieldName === 'confirmPassword' && this.registerForm.errors?.['passwordMismatch']) {
        return this.errorMessages.passwordMismatch;
      }
      return '';
    }

    const errorTypes = Object.keys(field.errors);

    for (const errorType of errorTypes) {
      if (errorType === 'required') {
        const message = (this.errorMessages.required as any)[fieldName];
        if (message) return message;
      } else {
        const message = (this.errorMessages as any)[errorType];
        if (message) {
          if (errorType === 'minlength') {
            return message.replace('{min}', field.errors[errorType].requiredLength);
          }
          if (errorType === 'maxlength') {
            return message.replace('{max}', field.errors[errorType].requiredLength);
          }
          return message;
        }
      }
    }

    return this.defaultMessages.generic;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    const fieldInvalid = !!(field && field.invalid && (field.dirty || field.touched));

    if (fieldName === 'confirmEmail' && this.registerForm.errors?.['emailMismatch']) {
      return !!(field && (field.dirty || field.touched));
    }
    if (fieldName === 'confirmPassword' && this.registerForm.errors?.['passwordMismatch']) {
      return !!(field && (field.dirty || field.touched));
    }

    return fieldInvalid;
  }

  // Vérifier si un champ est en cours de validation asynchrone
  isFieldValidating(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return field?.pending ?? false;
  }

  // ============ CUSTOM VALIDATORS ============

  emailValidator = (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) {
      return null;
    }

    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

    if (!emailRegex.test(control.value)) {
      return { invalidEmail: true };
    }

    return null;
  };

  noWhitespaceValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      const isWhitespace = (control.value || '').trim().length === 0;
      return isWhitespace ? { whitespace: true } : null;
    };
  }

  passwordStrengthValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      const password = control.value;
      const errors: ValidationErrors = {};

      if (!/[A-Z]/.test(password)) {
        errors['noUpperCase'] = true;
      }

      if (!/[a-z]/.test(password)) {
        errors['noLowerCase'] = true;
      }

      if (!/[0-9]/.test(password)) {
        errors['noNumber'] = true;
      }

      if (!/[!@#$%^&*(),.?":{}|<>]/.test(password)) {
        errors['noSpecialChar'] = true;
      }

      return Object.keys(errors).length > 0 ? errors : null;
    };
  }

  emailMatchValidator(): ValidatorFn {
    return (formGroup: AbstractControl): ValidationErrors | null => {
      const email = formGroup.get('email')?.value;
      const confirmEmail = formGroup.get('confirmEmail')?.value;

      if (!email || !confirmEmail) {
        return null;
      }

      return email === confirmEmail ? null : { emailMismatch: true };
    };
  }

  passwordMatchValidator(): ValidatorFn {
    return (formGroup: AbstractControl): ValidationErrors | null => {
      const password = formGroup.get('password')?.value;
      const confirmPassword = formGroup.get('confirmPassword')?.value;

      if (!password || !confirmPassword) {
        return null;
      }

      return password === confirmPassword ? null : { passwordMismatch: true };
    };
  }

  // ============ ERROR MESSAGES ============

  private readonly errorMessages: any = {
    required: {
      username: 'The username is required',
      password: 'The password is required',
      email: 'The email is required',
      confirmEmail: 'Please confirm your email',
      confirmPassword: 'Please confirm your password'
    },
    invalidEmail: 'The email format is not valid',
    minlength: 'This field must be at least {min} characters long',
    maxlength: 'This field must not exceed {max} characters',
    whitespace: 'This field cannot contain only spaces',
    noUpperCase: 'Password must contain at least one uppercase letter',
    noLowerCase: 'Password must contain at least one lowercase letter',
    noNumber: 'Password must contain at least one number',
    noSpecialChar: 'Password must contain at least one special character (!@#$%^&*...)',
    emailMismatch: 'Emails do not match',
    passwordMismatch: 'Passwords do not match',
    usernameTaken: 'This username is already taken',
    emailTaken: 'This email is already registered'
  };

  private readonly defaultMessages = {
    required: 'This field is required',
    generic: 'This field contains some errors'
  };
}