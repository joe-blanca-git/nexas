import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-update-password',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './update-password.component.html',
  styleUrl: './update-password.component.scss'
})
export class UpdatePasswordComponent {
  updateForm: FormGroup;
  submitted = false;
  passwordUpdated = false; // Controls display of success view

  showPassword = false;
  showConfirmPassword = false;

  constructor(private fb: FormBuilder, private router: Router) {
    this.updateForm = this.fb.group({
      password: ['', [
        Validators.required, 
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._\-#^()*+=\[\]{}|\\:<>,?~/`~]).{6,}$/)
      ]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  // Custom password matching validator
  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { mismatch: true };
  }

  // Getter for easy validation checks in HTML
  get f() { return this.updateForm.controls; }

  onSubmit() {
    this.submitted = true;

    if (this.updateForm.invalid) {
      return;
    }

    console.log('Update Password Payload:', this.updateForm.value);
    // Simulate updating password successfully
    this.passwordUpdated = true;
  }
}
