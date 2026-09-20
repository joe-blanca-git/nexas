import { Injectable, signal } from '@angular/core';

export type ToastType = 'info' | 'success' | 'warning' | 'error';
export interface ToastConfig {
  message: string;
  type: ToastType;
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private toastSignal = signal<ToastConfig | null>(null);

  get currentToast() {
    return this.toastSignal();
  }

  show(message: string, type: ToastType = 'info', duration: number = 4000): void {
    this.toastSignal.set({ message, type });
    setTimeout(() => {
      // Only clear if the current toast is the same one we just showed
      if (this.toastSignal()?.message === message) {
        this.clear();
      }
    }, duration);
  }

  success(message: string, duration?: number): void {
    this.show(message, 'success', duration);
  }

  error(message: string, duration?: number): void {
    this.show(message, 'error', duration);
  }

  info(message: string, duration?: number): void {
    this.show(message, 'info', duration);
  }

  warning(message: string, duration?: number): void {
    this.show(message, 'warning', duration);
  }

  clear(): void {
    this.toastSignal.set(null);
  }
}
