import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

// Custom validator for password complexity
export function passwordStrengthValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (!value) {
    return null;
  }
  
  const hasUpperCase = /[A-Z]+/.test(value);
  const hasNumeric = /[0-9]+/.test(value);
  const hasSpecial = /[^A-Za-z0-9]+/.test(value);

  const passwordValid = hasUpperCase && hasNumeric && hasSpecial;

  if (!passwordValid) {
    return {
      passwordStrength: {
        hasUpperCase,
        hasNumeric,
        hasSpecial
      }
    };
  }
  return null;
}

// Custom validator for matching passwords
export function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');
  
  if (password && confirmPassword && password.value !== confirmPassword.value) {
    // Also set error on the confirmPassword control directly for easier UI binding
    confirmPassword.setErrors({ passwordMismatch: true });
    return { passwordMismatch: true };
  } else if (confirmPassword && confirmPassword.errors && confirmPassword.errors['passwordMismatch']) {
    // Clean up the error if they now match
    delete confirmPassword.errors['passwordMismatch'];
    if (!Object.keys(confirmPassword.errors).length) {
      confirmPassword.setErrors(null);
    }
  }
  
  return null;
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent {
  readonly form: FormGroup;
  showPassword = false;
  showConfirmPassword = false;
  submitting = false;
  submitted = false;

  constructor(private readonly fb: FormBuilder) {
    this.form = this.fb.group({
      password: ['', [
        Validators.required, 
        Validators.minLength(6),
        passwordStrengthValidator
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: passwordMatchValidator });
  }

  get password() { return this.form.get('password'); }
  get confirmPassword() { return this.form.get('confirmPassword'); }

  togglePasswordVisibility(): void { this.showPassword = !this.showPassword; }
  toggleConfirmPasswordVisibility(): void { this.showConfirmPassword = !this.showConfirmPassword; }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    
    // Simulate API call
    setTimeout(() => {
      this.submitting = false;
      this.submitted = true;
    }, 1000);
  }
}
