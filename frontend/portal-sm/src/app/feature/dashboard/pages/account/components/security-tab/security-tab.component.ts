import { Component, OnDestroy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../../../core/auth/auth.service';
import { ToastService } from '../../../../../../core/services/toast.service';
import { StateUtil } from '../../../../../../core/utils/UserState.util';

const RESEND_COOLDOWN_SECONDS = 60;

@Component({
  selector: 'app-security-tab',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './security-tab.component.html',
  styleUrl: './security-tab.component.scss'
})
export class SecurityTabComponent implements OnDestroy {
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly stateUtil = inject(StateUtil);

  sending = signal(false);
  cooldown = signal(0);
  private cooldownTimer?: ReturnType<typeof setInterval>;

  get email(): string {
    return this.stateUtil.user?.email ?? '';
  }

  ngOnDestroy(): void {
    if (this.cooldownTimer) {
      clearInterval(this.cooldownTimer);
    }
  }

  requestPasswordReset(): void {
    if (this.sending() || this.cooldown() > 0) return;

    if (!this.email) {
      this.toastService.error('Não foi possível identificar seu e-mail. Faça login novamente.');
      return;
    }

    this.sending.set(true);
    this.authService.forgotPassword(this.email).subscribe({
      next: () => {
        this.sending.set(false);
        this.toastService.success('Enviamos um e-mail com um link para você cadastrar uma nova senha.');
        this.startCooldown();
      },
      error: () => {
        this.sending.set(false);
        this.toastService.error('Não foi possível enviar o e-mail agora. Tente novamente em instantes.');
      }
    });
  }

  private startCooldown(): void {
    this.cooldown.set(RESEND_COOLDOWN_SECONDS);
    this.cooldownTimer = setInterval(() => {
      this.cooldown.update((seconds) => {
        if (seconds <= 1) {
          clearInterval(this.cooldownTimer);
          return 0;
        }
        return seconds - 1;
      });
    }, 1000);
  }
}
