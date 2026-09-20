import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProfileService } from '../../../../../../core/services/profile.service';
import { ToastService } from '../../../../../../core/services/toast.service';
import { StateUtil } from '../../../../../../core/utils/UserState.util';

// Tamanho do avatar salvo (quadrado, em px). Pequeno o bastante pra virar um base64 leve
// (JPEG comprimido nesse tamanho fica na casa de poucas dezenas de KB).
const AVATAR_SIZE = 256;
const AVATAR_JPEG_QUALITY = 0.85;
const MAX_UPLOAD_SIZE_BYTES = 8 * 1024 * 1024; // 8MB — limite do arquivo ORIGINAL escolhido, antes de comprimir

@Component({
  selector: 'app-profile-tab',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './profile-tab.component.html',
  styleUrl: './profile-tab.component.scss'
})
export class ProfileTabComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly profileService = inject(ProfileService);
  private readonly toastService = inject(ToastService);
  readonly stateUtil = inject(StateUtil);

  readonly form: FormGroup = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    phoneNumber: [''],
    email: [{ value: '', disabled: true }]
  });

  loading = signal(true);
  saving = signal(false);
  avatarMenuOpen = signal(false);
  avatarPreview = signal<string | null>(null);
  cameraActive = signal(false);
  mediaStream = signal<MediaStream | null>(null);

  private readonly formValue = toSignal(this.form.valueChanges, { initialValue: this.form.value });
  private readonly originalValues = signal<{ fullName: string; phoneNumber: string; avatar: string | null } | null>(
    null
  );

  readonly hasChanges = computed(() => {
    const original = this.originalValues();
    if (!original) return false;

    const current = this.formValue();
    return (
      (current.fullName ?? '') !== original.fullName ||
      (current.phoneNumber ?? '') !== original.phoneNumber ||
      this.avatarPreview() !== original.avatar
    );
  });

  get initials(): string {
    const name = this.form.get('fullName')?.value || this.stateUtil.user?.name || '';
    return name.trim().charAt(0).toUpperCase() || 'U';
  }

  get fullName() {
    return this.form.get('fullName');
  }

  ngOnInit(): void {
    this.profileService.getProfile().subscribe({
      next: (profile) => {
        this.form.patchValue({
          fullName: profile.fullName,
          phoneNumber: profile.phoneNumber ?? '',
          email: profile.email
        });
        this.avatarPreview.set(profile.avatar ?? null);
        this.originalValues.set({
          fullName: profile.fullName,
          phoneNumber: profile.phoneNumber ?? '',
          avatar: profile.avatar ?? null
        });
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Não foi possível carregar seus dados. Tente novamente.');
      }
    });
  }

  ngOnDestroy(): void {
    this.stopCameraStream();
  }

  toggleAvatarMenu(): void {
    this.avatarMenuOpen.update((open) => !open);
  }

  triggerFileInput(input: HTMLInputElement): void {
    this.avatarMenuOpen.set(false);
    input.click();
  }

  onAvatarFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';

    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.toastService.error('Selecione um arquivo de imagem válido.');
      return;
    }

    if (file.size > MAX_UPLOAD_SIZE_BYTES) {
      this.toastService.error('A imagem é muito grande. Escolha um arquivo de até 8MB.');
      return;
    }

    const objectUrl = URL.createObjectURL(file);
    const img = new Image();

    img.onload = () => {
      try {
        const base64 = this.drawToSquareBase64(img, img.naturalWidth, img.naturalHeight, false);
        this.avatarPreview.set(base64);
        this.toastService.info('Foto selecionada. Clique em "Salvar alterações" para confirmar.');
      } catch {
        this.toastService.error('Não foi possível processar a imagem selecionada.');
      } finally {
        URL.revokeObjectURL(objectUrl);
      }
    };
    img.onerror = () => {
      URL.revokeObjectURL(objectUrl);
      this.toastService.error('Não foi possível carregar a imagem selecionada.');
    };
    img.src = objectUrl;
  }

  async openCamera(): Promise<void> {
    this.avatarMenuOpen.set(false);

    if (!navigator.mediaDevices?.getUserMedia) {
      this.toastService.error('Seu navegador não suporta acesso à câmera.');
      return;
    }

    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: 'user' },
        audio: false
      });
      this.mediaStream.set(stream);
      this.cameraActive.set(true);
    } catch {
      this.toastService.error('Não foi possível acessar a câmera. Verifique as permissões do navegador.');
    }
  }

  closeCamera(): void {
    this.stopCameraStream();
    this.cameraActive.set(false);
  }

  capturePhoto(videoEl: HTMLVideoElement): void {
    if (!videoEl.videoWidth || !videoEl.videoHeight) {
      this.toastService.error('Não foi possível capturar a foto. Tente novamente.');
      this.closeCamera();
      return;
    }

    try {
      // Espelhado (efeito "selfie"), pra bater com o preview ao vivo que o usuário viu no modal.
      const base64 = this.drawToSquareBase64(videoEl, videoEl.videoWidth, videoEl.videoHeight, true);
      this.avatarPreview.set(base64);
      this.toastService.info('Foto capturada. Clique em "Salvar alterações" para confirmar.');
    } catch {
      this.toastService.error('Não foi possível capturar a foto. Tente novamente.');
    }

    this.closeCamera();
  }

  /** Recorta a fonte num quadrado (cover-crop) e reduz pra AVATAR_SIZE, devolvendo um data URL JPEG comprimido. */
  private drawToSquareBase64(
    source: CanvasImageSource,
    sourceWidth: number,
    sourceHeight: number,
    mirror: boolean
  ): string {
    const canvas = document.createElement('canvas');
    canvas.width = AVATAR_SIZE;
    canvas.height = AVATAR_SIZE;
    const ctx = canvas.getContext('2d');
    if (!ctx) throw new Error('Canvas 2D context indisponível.');

    const scale = Math.max(AVATAR_SIZE / sourceWidth, AVATAR_SIZE / sourceHeight);
    const scaledWidth = sourceWidth * scale;
    const scaledHeight = sourceHeight * scale;
    const offsetX = (AVATAR_SIZE - scaledWidth) / 2;
    const offsetY = (AVATAR_SIZE - scaledHeight) / 2;

    if (mirror) {
      ctx.translate(AVATAR_SIZE, 0);
      ctx.scale(-1, 1);
    }

    ctx.drawImage(source, offsetX, offsetY, scaledWidth, scaledHeight);

    return canvas.toDataURL('image/jpeg', AVATAR_JPEG_QUALITY);
  }

  private stopCameraStream(): void {
    this.mediaStream()?.getTracks().forEach((track) => track.stop());
    this.mediaStream.set(null);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.saving() || !this.hasChanges()) {
      return;
    }

    this.saving.set(true);
    const { fullName, phoneNumber } = this.form.getRawValue();

    this.profileService
      .updateProfile({ fullName, phoneNumber: phoneNumber || null, avatar: this.avatarPreview() })
      .subscribe({
        next: (profile) => {
          this.saving.set(false);
          this.avatarPreview.set(profile.avatar ?? null);
          this.originalValues.set({
            fullName: profile.fullName,
            phoneNumber: profile.phoneNumber ?? '',
            avatar: profile.avatar ?? null
          });
          this.stateUtil.updateUser({ name: profile.fullName });

          try {
            const cached = sessionStorage.getItem('nexas_user');
            if (cached) {
              const parsed = JSON.parse(cached);
              sessionStorage.setItem('nexas_user', JSON.stringify({ ...parsed, name: profile.fullName }));
            }
          } catch {
            // sessionStorage indisponível (modo privado, etc.) — segue sem cache
          }

          this.toastService.success('Perfil atualizado com sucesso!');
        },
        error: () => {
          this.saving.set(false);
          this.toastService.error('Não foi possível salvar suas alterações. Tente novamente.');
        }
      });
  }
}
