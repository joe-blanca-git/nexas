import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors, AsyncValidatorFn } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
import { Observable, of, timer } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  registerForm: FormGroup;
  submitted: boolean = false;
  showPassword = false;
  showConfirmPassword = false;
  isLoading = false;
  isCheckingEmail = false;

  private authService = inject(AuthService);
  private toastService = inject(ToastService);

  constructor(private fb: FormBuilder, private router: Router) {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email], [this.emailValidator()]],
      password: ['', [
        Validators.required, 
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._\-#^()*+=\[\]{}|\\:<>,?~/`~]).{6,}$/)
      ]],
      confirmPassword: ['', [Validators.required]],
      birthDate: ['', [Validators.required]],
    }, {
      validators: this.passwordMatchValidator
    });
  }

  // Validador customizado para verificar disponibilidade do e-mail
  emailValidator(): AsyncValidatorFn {
    return (control: AbstractControl): Observable<ValidationErrors | null> => {
      if (!control.value) {
        return of(null);
      }
      
      this.isCheckingEmail = true;
      return timer(500).pipe(
        switchMap(() => this.authService.checkEmail(control.value)),
        map((res: any) => {
          this.isCheckingEmail = false;
          // Se a API retornar indícios de que o e-mail está em uso
          if (res === true || res?.exists || res?.inUse) {
            return { emailTaken: true };
          }
          return null; // OK
        }),
        catchError((err) => {
          this.isCheckingEmail = false;
          if (err?.error?.message?.toLowerCase().includes('já existe') || err?.error?.message?.toLowerCase().includes('already exists')) {
             return of({ emailTaken: true });
          }
          return of(null);
        })
      );
    };
  }

  // Validador customizado para verificar se as senhas coincidem
  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { mismatch: true };
  }

  get f() { return this.registerForm.controls; }

  onSubmit() {
    this.submitted = true;

    if (this.registerForm.invalid) {
      return;
    }

    this.isLoading = true;
    const formVal = this.registerForm.getRawValue();
    this.registerForm.disable();

    let isoBirthDate = formVal.birthDate;
    if (isoBirthDate) {
      try {
        isoBirthDate = new Date(isoBirthDate).toISOString();
      } catch (e) {
        console.error(e);
      }
    }

    const payload = {
      idSystem: 2,
      name: formVal.name,
      email: formVal.email,
      password: formVal.password,
      birthDate: isoBirthDate
    };

    this.authService.registerSystemUser(payload).subscribe({
      next: () => {
         this.toastService.success('Cadastro realizado com sucesso!', 5000);
         this.router.navigate(['/auth/login']);
      },
      error: (err) => {
         // O interceptor de erro já exibe o toast com a mensagem,
         // então apenas removemos o loading state.
         this.isLoading = false;
         this.registerForm.enable();
      }
    });
  }
}
