import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
import { AuthUtil } from '../../../../core/auth/auth.util';
import { ToastService } from '../../../../core/services/toast.service';
import { loadGoogleIdentityScript } from '../../../../core/utils/google-identity.util';
import { passwordStrengthValidator } from '../reset-password/reset-password.component';

declare var google: any;

export const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('senha');
  const confirmPassword = control.get('confirmarSenha');
  if (password && confirmPassword && password.value !== confirmPassword.value) {
    confirmPassword.setErrors({ ...confirmPassword.errors, passwordMismatch: true });
    return { passwordMismatch: true };
  }
  if (confirmPassword && confirmPassword.hasError('passwordMismatch')) {
    const errors = { ...confirmPassword.errors };
    delete errors['passwordMismatch'];
    confirmPassword.setErrors(Object.keys(errors).length ? errors : null);
  }
  return null;
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent implements OnInit {
  readonly form: FormGroup;

  showPassword = false;
  showConfirmPassword = false;
  submitting = false;

  private authService = inject(AuthService);
  private authUtil = inject(AuthUtil);
  private toastService = inject(ToastService);
  private router = inject(Router);

  constructor(private readonly fb: FormBuilder) {
    this.form = this.fb.group({
      nome: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      telefone: ['', [Validators.required]],
      senha: ['', [Validators.required, Validators.minLength(6), passwordStrengthValidator]],
      confirmarSenha: ['', [Validators.required]]
    }, { validators: passwordMatchValidator });
  }

  ngOnInit(): void {
    loadGoogleIdentityScript()
      .then(() => {
        google.accounts.id.initialize({
          client_id: '889143178707-pd63f02d0htkifq62m61n0ondro9g987.apps.googleusercontent.com',
          callback: this.handleGoogleCredentialResponse.bind(this)
        });
        google.accounts.id.renderButton(
          document.getElementById('google-btn-wrapper-register'),
          { theme: 'outline', size: 'large', type: 'standard', width: '100%', text: 'signup_with' } // customizing the google button
        );
      })
      .catch((err) => console.error('Falha ao carregar cadastro do Google', err));
  }

  handleGoogleCredentialResponse(response: any): void {
    if (response.credential) {
      this.submitting = true;
      this.authService.googleLogin(response.credential).subscribe({
        next: (res: any) => {
          this.authUtil.saveCookieAuth(res);
          this.authService.rehydrateUserState().then(() => {
            this.submitting = false;
            this.toastService.success('Autenticação com Google realizada com sucesso!');
            this.router.navigate(['/']);
          });
        },
        error: (err: any) => {
          console.error('Erro de login Google', err);
          this.submitting = false;
          this.toastService.error('Falha ao autenticar com Google. Tente novamente.');
        }
      });
    }
  }

  get nome() { return this.form.get('nome'); }
  get email() { return this.form.get('email'); }
  get telefone() { return this.form.get('telefone'); }
  get senha() { return this.form.get('senha'); }
  get confirmarSenha() { return this.form.get('confirmarSenha'); }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    
    // Extract everything except confirmarSenha for the payload
    const { confirmarSenha, ...payload } = this.form.value;
    
    this.authService.register(payload).subscribe({
      next: (res: any) => {
        this.submitting = false;
        this.toastService.success('Conta criada com sucesso! Faça login para continuar.');
        this.router.navigate(['/login']);
      },
      error: (err: any) => {
        console.error('Erro no cadastro', err);
        this.submitting = false;
        
        let errorMsg = 'Falha ao criar conta. Verifique os dados e tente novamente.';
        if (err.error && err.error.message) {
          errorMsg = err.error.message;
        } else if (Array.isArray(err.error)) {
          errorMsg = err.error[0]?.description || errorMsg;
        }
        
        this.toastService.error(errorMsg);
      }
    });
  }
}
