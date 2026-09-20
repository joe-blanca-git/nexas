import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { NavbarComponent } from '../../../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { ToastService } from '../../../../core/services/toast.service';
import {
  AppRoleModel,
  ApplicationEndUserModel,
  ApplicationModel,
  ApplicationsService
} from '../../../../core/services/applications.service';
import {
  applicationGradient,
  applicationInitials,
  applicationStatusLabel,
  maskApiKey
} from '../../../../core/utils/application-display.util';
import { CreateEndUserModalComponent } from './components/create-end-user-modal/create-end-user-modal.component';
import { ManageRolesModalComponent } from './components/manage-roles-modal/manage-roles-modal.component';
import { EditUserRolesModalComponent } from './components/edit-user-roles-modal/edit-user-roles-modal.component';

@Component({
  selector: 'app-application-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    NavbarComponent,
    FooterComponent,
    CreateEndUserModalComponent,
    ManageRolesModalComponent,
    EditUserRolesModalComponent
  ],
  templateUrl: './application-detail.component.html',
  styleUrl: './application-detail.component.scss'
})
export class ApplicationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly applicationsService = inject(ApplicationsService);
  private readonly toastService = inject(ToastService);

  loading = signal(true);
  app = signal<ApplicationModel | null>(null);

  regenerateConfirmOpen = signal(false);
  regenerating = signal(false);

  editingGoogleClientId = signal(false);
  googleClientIdInput = signal('');
  savingGoogleClientId = signal(false);

  usersLoading = signal(true);
  users = signal<ApplicationEndUserModel[]>([]);

  roles = signal<AppRoleModel[]>([]);

  createUserModalOpen = signal(false);
  manageRolesModalOpen = signal(false);
  editRolesUser = signal<ApplicationEndUserModel | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.loading.set(false);
      this.usersLoading.set(false);
      return;
    }

    this.applicationsService.getById(id).subscribe({
      next: (app) => {
        this.app.set(app);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });

    this.loadUsers(id);

    this.applicationsService.getRoles(id).subscribe({
      next: (roles) => this.roles.set(roles),
      error: () => {}
    });
  }

  private loadUsers(id: string): void {
    this.usersLoading.set(true);
    this.applicationsService.getUsers(id).subscribe({
      next: (users) => {
        this.users.set(users);
        this.usersLoading.set(false);
      },
      error: () => {
        this.usersLoading.set(false);
      }
    });
  }

  initials(name: string): string {
    return applicationInitials(name);
  }

  statusLabel(status: string): string {
    return applicationStatusLabel(status);
  }

  formatDate(iso: string | null): string {
    return iso ? new Date(iso).toLocaleDateString('pt-BR') : '—';
  }

  gradientFor(app: ApplicationModel): string {
    return applicationGradient(app);
  }

  maskedApiKey(apiKey: string | null | undefined): string {
    return maskApiKey(apiKey);
  }

  userInitials(user: ApplicationEndUserModel): string {
    return applicationInitials(user.fullName || user.email);
  }

  async copyApiKey(): Promise<void> {
    const app = this.app();
    if (!app) return;

    try {
      await navigator.clipboard.writeText(app.apiKey);
      this.toastService.success('Chave de API copiada.');
    } catch {
      this.toastService.error('Não foi possível copiar a chave. Copie manualmente.');
    }
  }

  goBack(): void {
    this.router.navigate(['/applications']);
  }

  openRegenerateConfirm(): void {
    this.regenerateConfirmOpen.set(true);
  }

  closeRegenerateConfirm(): void {
    if (this.regenerating()) return;
    this.regenerateConfirmOpen.set(false);
  }

  confirmRegenerate(): void {
    const current = this.app();
    if (!current || this.regenerating()) return;

    this.regenerating.set(true);
    this.applicationsService.regenerateApiKey(current.id).subscribe({
      next: (updated) => {
        this.regenerating.set(false);
        this.regenerateConfirmOpen.set(false);
        this.app.set(updated);
        this.toastService.success('Nova chave de API gerada.');
      },
      error: () => {
        this.regenerating.set(false);
        this.toastService.error('Não foi possível gerar uma nova chave agora. Tente novamente.');
      }
    });
  }

  startEditGoogleClientId(): void {
    this.googleClientIdInput.set(this.app()?.googleClientId || '');
    this.editingGoogleClientId.set(true);
  }

  cancelEditGoogleClientId(): void {
    this.editingGoogleClientId.set(false);
  }

  saveGoogleClientId(): void {
    const current = this.app();
    if (!current || this.savingGoogleClientId()) return;

    this.savingGoogleClientId.set(true);
    const value = this.googleClientIdInput().trim() || null;

    this.applicationsService.updateGoogleClientId(current.id, value).subscribe({
      next: (updated) => {
        this.savingGoogleClientId.set(false);
        this.editingGoogleClientId.set(false);
        this.app.set(updated);
        this.toastService.success(value ? 'Login com Google configurado.' : 'Login com Google desabilitado.');
      },
      error: () => {
        this.savingGoogleClientId.set(false);
        this.toastService.error('Não foi possível salvar agora. Tente novamente.');
      }
    });
  }

  openCreateUserModal(): void {
    this.createUserModalOpen.set(true);
  }

  closeCreateUserModal(): void {
    this.createUserModalOpen.set(false);
  }

  onUserCreated(): void {
    this.toastService.success('Usuário criado com sucesso.');
    const current = this.app();
    if (current) {
      this.loadUsers(current.id);
    }
  }

  openManageRolesModal(): void {
    this.manageRolesModalOpen.set(true);
  }

  closeManageRolesModal(): void {
    this.manageRolesModalOpen.set(false);
  }

  onRolesChanged(roles: AppRoleModel[]): void {
    this.roles.set(roles);
    const current = this.app();
    if (current) {
      this.loadUsers(current.id);
    }
  }

  openEditRolesModal(user: ApplicationEndUserModel): void {
    this.editRolesUser.set(user);
  }

  closeEditRolesModal(): void {
    this.editRolesUser.set(null);
  }

  onUserRolesSaved(): void {
    this.editRolesUser.set(null);
    const current = this.app();
    if (current) {
      this.loadUsers(current.id);
    }
  }
}
