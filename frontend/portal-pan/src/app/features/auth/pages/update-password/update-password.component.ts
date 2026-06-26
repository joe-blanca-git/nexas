import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';

@Component({
  selector: 'app-update-password',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './update-password.component.html',
  styleUrl: './update-password.component.scss'
})
export class UpdatePasswordComponent implements OnInit {
  updateForm: FormGroup;
  submitted = false;
  passwordUpdated = false; // Controls display of success view
  isLoading = false;

  showPassword = false;
  showConfirmPassword = false;

  email: string | null = null;
  token: string | null = null;

  constructor(
    private fb: FormBuilder, 
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService
  ) {
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

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || null;
      this.token = params['token'] || null;
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

    if (!this.email || !this.token) {
      console.error('Faltam parâmetros de email ou token na URL');
      return;
    }

    this.isLoading = true;
    const { password: newPassword } = this.updateForm.getRawValue();
    this.updateForm.disable();

    this.authService.resetPassword(this.email, this.token, newPassword).subscribe({
      next: () => {
        this.passwordUpdated = true;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Erro ao redefinir a senha', err);
        this.isLoading = false;
        this.updateForm.enable();
      }
    });
  }
}
