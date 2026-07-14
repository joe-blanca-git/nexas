import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { ToastService, ToastConfig } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrls: ['./toast.component.scss']
})
export class ToastComponent implements OnInit, OnDestroy {
  toast: ToastConfig | null = null;
  private subscription: Subscription = new Subscription();
  private timeout: any;

  constructor(private toastService: ToastService) {}

  ngOnInit(): void {
    this.subscription = this.toastService.toastState$.subscribe((toast) => {
      this.toast = toast;
      
      if (this.timeout) {
        clearTimeout(this.timeout);
      }
      
      if (toast.duration) {
        this.timeout = setTimeout(() => {
          this.close();
        }, toast.duration);
      }
    });
  }

  close(): void {
    this.toast = null;
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    if (this.timeout) {
      clearTimeout(this.timeout);
    }
  }
}
