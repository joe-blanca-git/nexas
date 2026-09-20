import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../../../core/auth/auth.service';
import { PrivacyPreferences, PrivacyService } from '../../../../../../core/services/privacy.service';
import { ToastService } from '../../../../../../core/services/toast.service';
import { StateUtil } from '../../../../../../core/utils/UserState.util';

type PreferenceKey = keyof PrivacyPreferences;

@Component({
  selector: 'app-privacy-tab',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './privacy-tab.component.html',
  styleUrl: './privacy-tab.component.scss'
})
export class PrivacyTabComponent implements OnInit {
  private readonly privacyService = inject(PrivacyService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly stateUtil = inject(StateUtil);

  loading = signal(true);
  preferences = signal<PrivacyPreferences | null>(null);
  savingKey = signal<PreferenceKey | null>(null);

  exporting = signal(false);

  deleteModalOpen = signal(false);
  deleteConfirmInput = signal('');
  deleting = signal(false);

  get email(): string {
    return this.stateUtil.user?.email ?? '';
  }

  get canConfirmDelete(): boolean {
    const email = this.email;
    return !!email && this.deleteConfirmInput().trim().toLowerCase() === email.toLowerCase();
  }

  ngOnInit(): void {
    this.privacyService.getPreferences().subscribe({
      next: (prefs) => {
        this.preferences.set(prefs);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Não foi possível carregar suas preferências.');
      }
    });
  }

  onToggleChange(key: PreferenceKey, event: Event): void {
    const current = this.preferences();
    if (!current || this.savingKey()) return;

    const checked = (event.target as HTMLInputElement).checked;
    const updated: PrivacyPreferences = { ...current, [key]: checked };

    this.savingKey.set(key);
    this.privacyService.updatePreferences(updated).subscribe({
      next: (saved) => {
        this.preferences.set(saved);
        this.savingKey.set(null);
      },
      error: () => {
        this.savingKey.set(null);
        this.toastService.error('Não foi possível salvar essa preferência. Tente novamente.');
      }
    });
  }

  exportMyData(): void {
    if (this.exporting()) return;

    this.exporting.set(true);
    this.privacyService.exportData().subscribe({
      next: (data) => {
        this.exporting.set(false);
        this.downloadAsJson(data);
        this.toastService.success('Seus dados foram exportados.');
      },
      error: () => {
        this.exporting.set(false);
        this.toastService.error('Não foi possível exportar seus dados agora.');
      }
    });
  }

  private downloadAsJson(data: unknown): void {
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `nexas-meus-dados-${new Date().toISOString().slice(0, 10)}.json`;
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(url);
  }

  openDeleteModal(): void {
    this.deleteConfirmInput.set('');
    this.deleteModalOpen.set(true);
  }

  closeDeleteModal(): void {
    if (this.deleting()) return;
    this.deleteModalOpen.set(false);
  }

  onDeleteConfirmInput(event: Event): void {
    this.deleteConfirmInput.set((event.target as HTMLInputElement).value);
  }

  confirmDelete(): void {
    if (!this.canConfirmDelete || this.deleting()) return;

    this.deleting.set(true);
    this.privacyService.deleteAccount(this.email).subscribe({
      next: () => {
        this.toastService.success('Sua conta foi excluída.');
        this.authService.logOut();
      },
      error: () => {
        this.deleting.set(false);
        this.toastService.error('Não foi possível excluir sua conta agora. Tente novamente.');
      }
    });
  }
}
