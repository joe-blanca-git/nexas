import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AppRoleModel, ApplicationsService } from '../../../../../../core/services/applications.service';
import { ToastService } from '../../../../../../core/services/toast.service';

@Component({
  selector: 'app-manage-roles-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './manage-roles-modal.component.html',
  styleUrl: './manage-roles-modal.component.scss'
})
export class ManageRolesModalComponent {
  @Input() appId!: string;
  @Input() roles: AppRoleModel[] = [];

  @Output() closed = new EventEmitter<void>();
  @Output() rolesChanged = new EventEmitter<AppRoleModel[]>();

  private readonly fb = inject(FormBuilder);
  private readonly applicationsService = inject(ApplicationsService);
  private readonly toastService = inject(ToastService);

  creating = signal(false);
  roleIdPendingDelete = signal<string | null>(null);
  deletingRoleId = signal<string | null>(null);

  readonly form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]]
  });

  get name() {
    return this.form.get('name');
  }

  onBackdropClick(): void {
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

    this.creating.set(true);
    const name = this.form.getRawValue().name.trim();

    this.applicationsService.createRole(this.appId, name).subscribe({
      next: (role) => {
        this.creating.set(false);
        this.form.reset();
        const updated = [...this.roles, role].sort((a, b) => a.name.localeCompare(b.name));
        this.roles = updated;
        this.rolesChanged.emit(updated);
      },
      error: (err) => {
        this.creating.set(false);
        const message = err?.status === 400
          ? 'Já existe um papel com esse nome nessa aplicação.'
          : 'Não foi possível criar o papel. Tente novamente.';
        this.toastService.error(message);
      }
    });
  }

  askDelete(roleId: string): void {
    this.roleIdPendingDelete.set(roleId);
  }

  cancelDelete(): void {
    this.roleIdPendingDelete.set(null);
  }

  confirmDelete(roleId: string): void {
    this.deletingRoleId.set(roleId);
    this.applicationsService.deleteRole(this.appId, roleId).subscribe({
      next: () => {
        this.deletingRoleId.set(null);
        this.roleIdPendingDelete.set(null);
        const updated = this.roles.filter((r) => r.id !== roleId);
        this.roles = updated;
        this.rolesChanged.emit(updated);
        this.toastService.success('Papel removido.');
      },
      error: () => {
        this.deletingRoleId.set(null);
        this.toastService.error('Não foi possível remover o papel agora. Tente novamente.');
      }
    });
  }
}
