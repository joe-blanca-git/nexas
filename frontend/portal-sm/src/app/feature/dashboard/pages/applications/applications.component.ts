import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { NavbarComponent } from '../../../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { ToastService } from '../../../../core/services/toast.service';
import { ApplicationModel, ApplicationsService } from '../../../../core/services/applications.service';
import { ApplicationFormModalComponent } from './components/application-form-modal/application-form-modal.component';
import {
  applicationGradient,
  applicationInitials,
  applicationStatusLabel,
  maskApiKey
} from '../../../../core/utils/application-display.util';

@Component({
  selector: 'app-applications',
  standalone: true,
  imports: [CommonModule, RouterModule, NavbarComponent, FooterComponent, ApplicationFormModalComponent],
  templateUrl: './applications.component.html',
  styleUrl: './applications.component.scss'
})
export class ApplicationsComponent implements OnInit {
  private readonly applicationsService = inject(ApplicationsService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  loading = signal(true);
  applications = signal<ApplicationModel[]>([]);

  showCreateModal = signal(false);

  deleteTarget = signal<ApplicationModel | null>(null);
  deleting = signal(false);

  regenerateTarget = signal<ApplicationModel | null>(null);
  regenerating = signal(false);

  ngOnInit(): void {
    this.applicationsService.getAll().subscribe({
      next: (apps) => {
        this.applications.set(apps);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Não foi possível carregar suas aplicações.');
      }
    });
  }

  initials(name: string): string {
    return applicationInitials(name);
  }

  statusLabel(status: string): string {
    return applicationStatusLabel(status);
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString('pt-BR');
  }

  gradientFor(app: ApplicationModel): string {
    return applicationGradient(app);
  }

  maskedApiKey(apiKey: string | null | undefined): string {
    return maskApiKey(apiKey);
  }

  async copyApiKey(apiKey: string): Promise<void> {
    try {
      await navigator.clipboard.writeText(apiKey);
      this.toastService.success('Chave de API copiada.');
    } catch {
      this.toastService.error('Não foi possível copiar a chave. Copie manualmente.');
    }
  }

  onNewApplication(): void {
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  onApplicationCreated(app: ApplicationModel): void {
    this.applications.update((apps) => [app, ...apps]);
  }

  onManage(app: ApplicationModel): void {
    this.router.navigate(['/applications', app.id]);
  }

  openDeleteConfirm(app: ApplicationModel): void {
    this.deleteTarget.set(app);
  }

  closeDeleteConfirm(): void {
    if (this.deleting()) return;
    this.deleteTarget.set(null);
  }

  confirmDelete(): void {
    const app = this.deleteTarget();
    if (!app || this.deleting()) return;

    this.deleting.set(true);
    this.applicationsService.delete(app.id).subscribe({
      next: () => {
        this.deleting.set(false);
        this.deleteTarget.set(null);
        this.applications.update((apps) => apps.filter((a) => a.id !== app.id));
        this.toastService.success(`Aplicação "${app.name}" excluída.`);
      },
      error: () => {
        this.deleting.set(false);
        this.toastService.error('Não foi possível excluir a aplicação agora. Tente novamente.');
      }
    });
  }

  openRegenerateConfirm(app: ApplicationModel): void {
    this.regenerateTarget.set(app);
  }

  closeRegenerateConfirm(): void {
    if (this.regenerating()) return;
    this.regenerateTarget.set(null);
  }

  confirmRegenerate(): void {
    const app = this.regenerateTarget();
    if (!app || this.regenerating()) return;

    this.regenerating.set(true);
    this.applicationsService.regenerateApiKey(app.id).subscribe({
      next: (updated) => {
        this.regenerating.set(false);
        this.regenerateTarget.set(null);
        this.applications.update((apps) => apps.map((a) => (a.id === updated.id ? updated : a)));
        this.toastService.success(`Nova chave de API gerada para "${updated.name}".`);
      },
      error: () => {
        this.regenerating.set(false);
        this.toastService.error('Não foi possível gerar uma nova chave agora. Tente novamente.');
      }
    });
  }
}
