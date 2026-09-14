import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-recovery-password',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './recovery-password.component.html',
  styleUrl: './recovery-password.component.scss'
})
export class RecoveryPasswordComponent {
  recoveryForm: FormGroup;
  submitted = false;
  emailSent = false; // To show success feedback when form is submitted
  isLoading = false;

  constructor(private fb: FormBuilder, private router: Router, private authService: AuthService) {
    this.recoveryForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  // Getter for easy validation checks in HTML
  get f() { return this.recoveryForm.controls; }

  onSubmit() {
    this.submitted = true;

    if (this.recoveryForm.invalid) {
      return;
    }

    this.isLoading = true;
    const { email } = this.recoveryForm.getRawValue();
    this.recoveryForm.disable();

    this.authService.forgotPassword(email).subscribe({
      next: () => {
        this.emailSent = true;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Erro ao solicitar recuperação de senha', err);
        this.isLoading = false;
        this.recoveryForm.enable();
      }
    });
  }
}
