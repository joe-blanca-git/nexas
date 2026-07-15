import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
import { StateUtil } from '../../../../core/utils/UserState.util';
import { UserLogedModel } from '../../../../core/models/userLoged.model';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  loginForm: FormGroup;
  submitted = false;
  showPassword = false;
  isLoading = false;

  private authService = inject(AuthService);
  private authUtil = inject(AuthService).authUtil;
  private stateUtil = inject(StateUtil);
  private toastService = inject(ToastService);

  constructor(private fb: FormBuilder, private router: Router) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  get f() { return this.loginForm.controls; }

  onSubmit() {    
    this.submitted = true;

    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading = true;
    const { email, password } = this.loginForm.getRawValue();
    this.loginForm.disable();

    this.authService.logIn(email, password).subscribe({
      next: (successResponse) => this.processSuccessfulLogin(successResponse),
      error: (errorResponse) => this.processFailedLogin(errorResponse)
    });
  }

  async processSuccessfulLogin(responseLogin: any) {
    
    try{
      const isTeacher = responseLogin.user?.roles?.some((r: any) => r.value === 'Teacher');
      const hasSystem1 = responseLogin.systemIds?.includes(1);

      if (!isTeacher || !hasSystem1) {
        this.toastService.error('Acesso não permitido. Portal exclusivo para professores.');
        this.isLoading = false;
        this.loginForm.enable();
        return;
      }

      await this.authUtil.saveCookieAuth(responseLogin);
      await this.stateUtil.saveUser(responseLogin);
      sessionStorage.setItem('nexas_user', JSON.stringify(responseLogin));
      this.router.navigate(['/home']);

    }catch(error){
      console.error('Erro ao processar login:', error);
      this.isLoading = false;
      this.loginForm.enable();
    }
  }

  async processFailedLogin(error: any) {
    console.error('Erro ao processar login:', error);
    this.isLoading = false;
    this.loginForm.enable();
  }
}
