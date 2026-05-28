import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';

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

  constructor(private fb: FormBuilder, private router: Router) {
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

    console.log('Recovery Password Payload:', this.recoveryForm.value);
    // Simulate sending recovery link
    this.emailSent = true;
  }
}
