import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Observable, forkJoin, of } from 'rxjs';
import { AppRoleModel, ApplicationsService } from '../../../../../../core/services/applications.service';
import { ToastService } from '../../../../../../core/services/toast.service';

function generatePassword(): string {
  const upper = 'ABCDEFGHJKLMNPQRSTUVWXYZ';
  const lower = 'abcdefghijkmnpqrstuvwxyz';
  const digits = '23456789';
  const symbols = '!@#$%&*';
  const all = upper + lower + digits + symbols;

  const pick = (chars: string) => chars[Math.floor(Math.random() * chars.length)];

  const required = [pick(upper), pick(lower), pick(digits), pick(symbols)];
  const rest = Array.from({ length: 8 }, () => pick(all));

  return [...required, ...rest].sort(() => Math.random() - 0.5).join('');
}

@Component({
  selector: 'app-create-end-user-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-end-user-modal.component.html',
  styleUrl: './create-end-user-modal.component.scss'
})
export class CreateEndUserModalComponent implements OnInit {
  @Input() appId!: string;
  @Input() apiKey!: string;
  @Input() roles: AppRoleModel[] = [];

  @Output() closed = new EventEmitter<void>();
  @Output() created = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly applicationsService = inject(ApplicationsService);
  private readonly toastService = inject(ToastService);

  submitting = signal(false);
  success = signal(false);
  showPassword = signal(false);
  copied = signal(false);

  readonly form: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    fullName: [''],
    password: [generatePassword(), [Validators.required, Validators.minLength(6)]],
    roleIds: this.fb.group({})
  });

  get email() {
    return this.form.get('email');
  }

  get password() {
    return this.form.get('password');
  }

  get roleIdsGroup(): FormGroup {
    return this.form.get('roleIds') as FormGroup;
  }

  ngOnInit(): void {
    for (const role of this.roles) {
      this.roleIdsGroup.addControl(role.id, this.fb.control(false));
    }
  }

  regeneratePassword(): void {
    this.password?.setValue(generatePassword());
    this.showPassword.set(true);
  }

  togglePasswordVisibility(): void {
    this.showPassword.update((v) => !v);
  }

  async copyPassword(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.password?.value || '');
      this.copied.set(true);
      setTimeout(() => this.copied.set(false), 2000);
    } catch {
      this.toastService.error('Não foi possível copiar a senha. Copie manualmente.');
    }
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
    const selectedRoleIds = Object.entries(raw.roleIds)
      .filter(([, checked]) => checked)
      .map(([roleId]) => roleId);

    this.applicationsService
      .createEndUser(this.appId, this.apiKey, {
        email: raw.email,
        password: raw.password,
        fullName: raw.fullName || null
      })
      .subscribe({
        next: (user) => {
          const assignments: Observable<unknown> = selectedRoleIds.length
            ? forkJoin(
                selectedRoleIds.map((roleId) => this.applicationsService.assignRoleToUser(this.appId, user.id, roleId))
              )
            : of(null);

          assignments.subscribe({
            next: () => {
              this.submitting.set(false);
              this.success.set(true);
              this.created.emit();
            },
            error: () => {
              this.submitting.set(false);
              this.success.set(true);
              this.created.emit();
              this.toastService.error('Usuário criado, mas houve um erro ao atribuir algum papel. Ajuste depois na lista.');
            }
          });
        },
        error: (err) => {
          this.submitting.set(false);
          const message = err?.error?.errors?.Email?.[0] || 'Não foi possível criar o usuário. Tente novamente.';
          this.toastService.error(message);
        }
      });
  }
}
