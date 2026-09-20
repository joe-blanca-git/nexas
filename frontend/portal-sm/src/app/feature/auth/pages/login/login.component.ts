import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
import { AuthUtil } from '../../../../core/auth/auth.util';
import { ToastService } from '../../../../core/services/toast.service';
import { loadGoogleIdentityScript } from '../../../../core/utils/google-identity.util';

declare var google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  readonly form: FormGroup;

  showPassword = false;
  submitting = false;

  private authService = inject(AuthService);
  private authUtil = inject(AuthUtil);
  private router = inject(Router);
  private toastService = inject(ToastService);

  constructor(private readonly fb: FormBuilder) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    loadGoogleIdentityScript()
      .then(() => {
        google.accounts.id.initialize({
          client_id: '889143178707-pd63f02d0htkifq62m61n0ondro9g987.apps.googleusercontent.com',
          callback: this.handleGoogleCredentialResponse.bind(this)
        });
        google.accounts.id.renderButton(
          document.getElementById('google-btn-wrapper'),
          { theme: 'outline', size: 'large', type: 'standard', width: '100%' } // customizing the google button
        );
      })
      .catch((err) => console.error('Falha ao carregar login do Google', err));
  }

  handleGoogleCredentialResponse(response: any): void {
    if (response.credential) {
      this.submitting = true;
      this.authService.googleLogin(response.credential).subscribe({
        next: (res: any) => {
          this.authUtil.saveCookieAuth(res);
          this.authService.rehydrateUserState().then(() => {
            this.submitting = false;
            this.router.navigate(['/']);
          });
        },
        error: (err: any) => {
          console.error('Erro de login Google', err);
          this.submitting = false;
          this.toastService.error('Falha no login com Google. Tente novamente.');
        }
      });
    }
  }

  get email() {
    return this.form.get('email');
  }

  get password() {
    return this.form.get('password');
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    const { email, password } = this.form.value;

    this.authService.logIn(email, password).subscribe({
      next: (res: any) => {
        // Save the cookie
        this.authUtil.saveCookieAuth(res);
        // Rehydrate state with claims
        this.authService.rehydrateUserState().then(() => {
          this.submitting = false;
          // Navigate to dashboard
          this.router.navigate(['/']);
        });
      },
      error: (err: any) => {
        console.error('Erro de login', err);
        this.submitting = false;
        
        let errorMsg = 'Falha no login. Verifique suas credenciais.';
        if (err.error && err.error.message) {
          errorMsg = err.error.message;
        } else if (err.status === 401 || err.status === 400) {
          errorMsg = 'Usuário ou senha incorretos.';
        }
        
        this.toastService.error(errorMsg);
      }
    });
  }
}
