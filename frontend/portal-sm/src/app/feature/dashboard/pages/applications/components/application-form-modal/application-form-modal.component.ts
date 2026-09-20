import { Component, EventEmitter, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApplicationModel, ApplicationsService } from '../../../../../../core/services/applications.service';
import { ToastService } from '../../../../../../core/services/toast.service';

@Component({
  selector: 'app-application-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './application-form-modal.component.html',
  styleUrl: './application-form-modal.component.scss'
})
export class ApplicationFormModalComponent {
  @Output() closed = new EventEmitter<void>();
  @Output() created = new EventEmitter<ApplicationModel>();

  private readonly fb = inject(FormBuilder);
  private readonly applicationsService = inject(ApplicationsService);
  private readonly toastService = inject(ToastService);

  submitting = signal(false);
  createdApp = signal<ApplicationModel | null>(null);
  copied = signal(false);

  readonly form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    description: [''],
    urlDomain: [''],
    urlLogo: [''],
    primaryColor: ['#38BDF8'],
    secondaryColor: ['#A78BFA'],
    status: ['active']
  });

  get name() {
    return this.form.get('name');
  }

  onBackdropClick(): void {
    if (this.submitting()) return;
    this.close();
  }

  close(): void {
    this.closed.emit();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    const raw = this.form.getRawValue();

    this.applicationsService
      .create({
        name: raw.name,
        description: raw.description || null,
        urlDomain: raw.urlDomain || null,
        urlLogo: raw.urlLogo || null,
        primaryColor: raw.primaryColor || null,
        secondaryColor: raw.secondaryColor || null,
        status: raw.status
      })
      .subscribe({
        next: (app) => {
          this.submitting.set(false);
          this.createdApp.set(app);
          this.created.emit(app);
        },
        error: () => {
          this.submitting.set(false);
          this.toastService.error('Não foi possível criar a aplicação. Tente novamente.');
        }
      });
  }

  async copyApiKey(): Promise<void> {
    const app = this.createdApp();
    if (!app) return;

    try {
      await navigator.clipboard.writeText(app.apiKey);
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    } catch {
      this.toastService.error('Não foi possível copiar a chave. Copie manualmente.');
    }
  }
}
