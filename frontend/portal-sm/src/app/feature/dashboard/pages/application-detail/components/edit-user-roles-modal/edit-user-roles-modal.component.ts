import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { forkJoin, Observable, of } from 'rxjs';
import {
  AppRoleModel,
  ApplicationEndUserModel,
  ApplicationsService
} from '../../../../../../core/services/applications.service';
import { ToastService } from '../../../../../../core/services/toast.service';

@Component({
  selector: 'app-edit-user-roles-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './edit-user-roles-modal.component.html',
  styleUrl: './edit-user-roles-modal.component.scss'
})
export class EditUserRolesModalComponent implements OnInit {
  @Input() appId!: string;
  @Input() user!: ApplicationEndUserModel;
  @Input() roles: AppRoleModel[] = [];

  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly applicationsService = inject(ApplicationsService);
  private readonly toastService = inject(ToastService);

  saving = signal(false);

  readonly form: FormGroup = this.fb.group({});

  ngOnInit(): void {
    const currentRoleIds = new Set(this.user.roles.map((r) => r.id));
    for (const role of this.roles) {
      this.form.addControl(role.id, this.fb.control(currentRoleIds.has(role.id)));
    }
  }

  onBackdropClick(): void {
    if (this.saving()) return;
    this.close();
  }

  close(): void {
    this.closed.emit();
  }

  onSubmit(): void {
    const raw = this.form.getRawValue();
    const originalRoleIds = new Set(this.user.roles.map((r) => r.id));

    const toAssign = this.roles.filter((r) => raw[r.id] && !originalRoleIds.has(r.id)).map((r) => r.id);
    const toRemove = this.roles.filter((r) => !raw[r.id] && originalRoleIds.has(r.id)).map((r) => r.id);

    if (toAssign.length === 0 && toRemove.length === 0) {
      this.close();
      return;
    }

    this.saving.set(true);

    const calls: Observable<unknown>[] = [
      ...toAssign.map((roleId) => this.applicationsService.assignRoleToUser(this.appId, this.user.id, roleId)),
      ...toRemove.map((roleId) => this.applicationsService.removeRoleFromUser(this.appId, this.user.id, roleId))
    ];

    forkJoin(calls.length ? calls : [of(null)]).subscribe({
      next: () => {
        this.saving.set(false);
        this.toastService.success('Papéis atualizados.');
        this.saved.emit();
      },
      error: () => {
        this.saving.set(false);
        this.toastService.error('Não foi possível atualizar os papéis agora. Tente novamente.');
      }
    });
  }
}
