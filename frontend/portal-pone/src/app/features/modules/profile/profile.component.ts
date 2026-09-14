import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { StateUtil } from '../../../core/utils/UserState.util';
import { SettingsService } from '../settings/services/settings.service';
import { AuthService } from '../../../core/auth/auth.service';
import { AddressVerifyService } from '../../../core/services/address-verify.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  private readonly stateUtil = inject(StateUtil);
  private readonly fb = inject(FormBuilder);
  private readonly settingsService = inject(SettingsService);
  private readonly authService = inject(AuthService);
  private readonly addressVerifyService = inject(AddressVerifyService);

  // Tabs
  activeTab: 'cadastrais' | 'professor' | 'seguranca' = 'cadastrais';

  // Constants
  countries = [
    { code: '+55', name: '+55 (BR)' },
    { code: '+1', name: '+1 (US/CA)' },
    { code: '+351', name: '+351 (PT)' }
    // Omitted other countries for brevity, added main ones
  ];

  // User Base Data
  userName: string = '';
  userEmail: string = '';
  userRole: string = '';
  userIdAgivys: string = '';
  
  // Person Form & Addresses
  personForm!: FormGroup;
  addressForm!: FormGroup;
  addresses: any[] = [];
  showAddressModal = false;
  isEditingAddress = false;
  currentAddressId: string | null = null;

  // Teacher Form Data
  teacherId: number | null = null;
  teacherForm!: FormGroup;
  avatarPreview: string | null = null;
  isLoading = false;
  isFetchingTeacher = false;

  // Security Form
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  isSubmitting = false;
  
  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  isInitializing = true;

  // Feedback
  showToast = false;
  toastMessage = '';
  toastClass = 'bg-success text-white';

  ngOnInit() {
    this.initForms();
    
    this.stateUtil.getUser().subscribe(user => {
      if (user) {
        this.userName = user.name || 'Usuário Não Informado';
        this.userEmail = user.email || '';
        this.userIdAgivys = user.id || '';
        
        if (user.roles && user.roles.length > 0) {
          this.userRole = user.roles[0].value || user.roles[0].name || 'Membro';
        } else {
          this.userRole = 'Membro';
        }

        // Fill Person Form
        this.personForm.patchValue({
          name: this.userName,
          email: this.userEmail
        });

        if (this.userIdAgivys) {
          this.loadTeacherData();
        }
      }
    });

    // Handle initialization state
    let completedRequests = 0;
    const checkCompletion = () => {
      completedRequests++;
      if (completedRequests >= 2) {
        // Add a slight delay for smoother transition
        setTimeout(() => { this.isInitializing = false; }, 300);
      }
    };

    // Replace regular load with intercepted ones
    const originalLoadPersonData = this.loadPersonData.bind(this);
    const originalLoadAddresses = this.loadAddresses.bind(this);

    this.loadPersonData = () => {
      this.authService.getPerson().subscribe({
        next: (person) => {
          if (person) {
            let phoneVal = person.phone || '';
            let countryCodeVal = '+55';
            if (phoneVal.startsWith('+')) {
              const spaceIdx = phoneVal.indexOf(' ');
              if (spaceIdx > 0) {
                countryCodeVal = phoneVal.substring(0, spaceIdx);
                phoneVal = phoneVal.substring(spaceIdx + 1);
              }
            }
            let formattedDate = '';
            if (person.birthDate) {
              formattedDate = new Date(person.birthDate).toISOString().split('T')[0];
            }
            this.personForm.patchValue({
              name: person.name || this.userName,
              email: person.email || this.userEmail,
              document: person.document || '',
              birthDate: formattedDate,
              phone: phoneVal,
              countryCode: countryCodeVal
            });
            if (person.name) this.userName = person.name;
          }
        },
        error: (err) => console.error('Falha ao buscar dados cadastrais:', err),
        complete: () => checkCompletion()
      });
    };

    this.loadAddresses = () => {
      this.authService.getMyAddresses().subscribe({
        next: (res) => this.addresses = res || [],
        error: () => console.error('Falha ao buscar endereços'),
        complete: () => checkCompletion()
      });
    };

    this.loadPersonData();
    this.loadAddresses();
  }

  initForms() {
    this.personForm = this.fb.group({
      name: ['', Validators.required],
      birthDate: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      countryCode: ['+55', Validators.required],
      phone: ['', Validators.required],
      document: ['', Validators.required]
    });

    this.addressForm = this.fb.group({
      description: ['Casa', Validators.required],
      zipCode: ['', Validators.required],
      street: ['', Validators.required],
      number: ['', Validators.required],
      complement: [''],
      neighborhood: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required]
    });

    this.teacherForm = this.fb.group({
      name: ['', Validators.required],
      role: [{ value: '', disabled: true }],
      position: [''],
      avatar: [''],
      instagramLink: [''],
      linkedinLink: [''],
      bio: ['']
    });
  }

  switchTab(tab: 'cadastrais' | 'professor' | 'seguranca') {
    this.activeTab = tab;
  }

  getInitials(name: string): string {
    if (!name || !name.trim()) return 'A';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return parts[0][0].toUpperCase();
  }

  triggerToast(message: string, isError = false) {
    this.toastMessage = message;
    this.toastClass = isError ? 'bg-danger text-white' : 'bg-success text-white';
    this.showToast = true;
    setTimeout(() => {
      this.showToast = false;
    }, 4000);
  }

  // --- DADOS CADASTRAIS --- //

  onSubmitPerson() {
    if (this.personForm.invalid) {
      this.triggerToast('Preencha os campos obrigatórios.', true);
      return;
    }

    this.isLoading = true;
    const val = this.personForm.getRawValue();
    
    let isoDate = val.birthDate;
    if (isoDate && !isoDate.includes('T')) {
      isoDate = new Date(isoDate).toISOString();
    }

    const payload = {
      name: val.name,
      birthDate: isoDate,
      email: val.email,
      phone: val.countryCode + ' ' + val.phone,
      document: val.document
    };

    this.authService.updatePerson(payload).subscribe({
      next: () => {
        this.isLoading = false;
        this.triggerToast('Dados cadastrais atualizados com sucesso!');
        this.userName = val.name; // Atualiza a UI header
      },
      error: () => {
        this.isLoading = false;
        this.triggerToast('Erro ao atualizar dados cadastrais.', true);
      }
    });
  }

  loadPersonData() {
    // This is overridden in ngOnInit
  }

  // --- ENDEREÇOS --- //

  loadAddresses() {
    // This is overridden in ngOnInit
  }

  onPhoneInput(event: any) {
    let value = event.target.value.replace(/\D/g, '');
    if (value.length > 11) value = value.slice(0, 11);
    
    let formatted = '';
    if (value.length > 0) {
      if (value.length <= 2) {
        formatted = '(' + value;
      } else if (value.length <= 6) {
        formatted = '(' + value.slice(0, 2) + ') ' + value.slice(2);
      } else if (value.length <= 10) {
        formatted = '(' + value.slice(0, 2) + ') ' + value.slice(2, 6) + '-' + value.slice(6);
      } else {
        formatted = '(' + value.slice(0, 2) + ') ' + value.slice(2, 7) + '-' + value.slice(7);
      }
    }
    
    event.target.value = formatted;
    this.personForm.get('phone')?.setValue(formatted, { emitEvent: false });
  }

  openAddressModal(address?: any) {
    if (address) {
      this.isEditingAddress = true;
      this.currentAddressId = address.id;
      this.addressForm.patchValue(address);
    } else {
      this.isEditingAddress = false;
      this.currentAddressId = null;
      this.addressForm.reset({ description: 'Casa' });
    }
    this.showAddressModal = true;
  }

  closeAddressModal() {
    this.showAddressModal = false;
  }

  onCepInput(event: any) {
    let value = event.target.value.replace(/\D/g, '');
    if (value.length > 8) value = value.slice(0, 8);
    
    if (value.length > 5) {
      value = value.slice(0, 5) + '-' + value.slice(5);
    }
    
    event.target.value = value;
    this.addressForm.get('zipCode')?.setValue(value, { emitEvent: false });
  }

  onCepChange() {
    const cep = this.addressForm.get('zipCode')?.value?.replace(/\D/g, '');
    if (cep && cep.length === 8) {
      this.addressVerifyService.getZipCode(cep).subscribe({
        next: (res: any) => {
          if (!res.erro) {
            this.addressForm.patchValue({
              street: res.logradouro,
              neighborhood: res.bairro,
              city: res.localidade,
              state: res.uf
            });
          }
        },
        error: () => this.triggerToast('Erro ao buscar CEP', true)
      });
    }
  }

  onSubmitAddress() {
    if (this.addressForm.invalid) {
      this.triggerToast('Preencha todos os campos obrigatórios do endereço.', true);
      return;
    }

    this.isLoading = true;
    const val = this.addressForm.getRawValue();
    const payload = {
      ...val,
      zipCode: val.zipCode?.replace(/\D/g, '')
    };

    if (this.isEditingAddress && this.currentAddressId) {
      this.authService.updateAddress(this.currentAddressId, payload).subscribe({
        next: () => {
          this.isLoading = false;
          this.triggerToast('Endereço atualizado com sucesso!');
          this.closeAddressModal();
          this.loadAddresses();
        },
        error: () => {
          this.isLoading = false;
          this.triggerToast('Erro ao atualizar endereço.', true);
        }
      });
    } else {
      this.authService.addAddress(payload).subscribe({
        next: () => {
          this.isLoading = false;
          this.triggerToast('Endereço adicionado com sucesso!');
          this.closeAddressModal();
          this.loadAddresses();
        },
        error: () => {
          this.isLoading = false;
          this.triggerToast('Erro ao adicionar endereço.', true);
        }
      });
    }
  }

  deleteAddress(id: string) {
    if (confirm('Tem certeza que deseja excluir este endereço?')) {
      this.authService.deleteAddress(id).subscribe({
        next: () => {
          this.triggerToast('Endereço excluído com sucesso!');
          this.loadAddresses();
        },
        error: () => {
          this.triggerToast('Erro ao excluir endereço.', true);
        }
      });
    }
  }

  // --- DADOS DO PROFESSOR --- //

  loadTeacherData() {
    this.isFetchingTeacher = true;
    this.settingsService.getTeachers().subscribe({
      next: (teachers) => {
        const myTeacher = teachers.find(t => String(t.idAgivys) === String(this.userIdAgivys));
        if (myTeacher) {
          this.teacherId = myTeacher.id;
          this.teacherForm.patchValue({
            name: myTeacher.name,
            role: myTeacher.role || '',
            position: myTeacher.position || '',
            avatar: myTeacher.avatar || '',
            instagramLink: myTeacher.instagramLink || '',
            linkedinLink: myTeacher.linkedinLink || '',
            bio: myTeacher.bio || ''
          });
          this.avatarPreview = myTeacher.avatar || null;
        } else {
          this.teacherForm.patchValue({
            name: this.userName,
            role: this.userRole
          });
        }
        this.isFetchingTeacher = false;
      },
      error: (err) => {
        console.error('Erro ao carregar dados do professor', err);
        this.isFetchingTeacher = false;
      }
    });
  }

  onAvatarSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 1024 * 1024) { // 1MB
        this.triggerToast('A imagem deve ter no máximo 1MB.', true);
        event.target.value = '';
        return;
      }
      const reader = new FileReader();
      reader.onload = () => {
        this.avatarPreview = reader.result as string;
        this.teacherForm.patchValue({ avatar: this.avatarPreview });
        this.teacherForm.get('avatar')?.markAsDirty();
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmitTeacher() {
    if (this.teacherForm.invalid) {
      this.triggerToast('Preencha os campos obrigatórios.', true);
      return;
    }

    this.isLoading = true;
    const payload = this.teacherForm.getRawValue();
    payload.idAgivys = String(this.userIdAgivys);

    if (this.teacherId) {
      payload.id = this.teacherId;
      this.settingsService.updateTeacher(this.teacherId, payload).subscribe({
        next: () => {
          this.isLoading = false;
          this.userName = payload.name;
          this.triggerToast('Perfil do professor atualizado com sucesso!');
        },
        error: (err) => {
          console.error(err);
          this.isLoading = false;
          this.triggerToast('Erro ao atualizar perfil do professor.', true);
        }
      });
    } else {
      this.settingsService.createTeacher(payload).subscribe({
        next: (id) => {
          this.isLoading = false;
          this.teacherId = id;
          this.userName = payload.name;
          this.triggerToast('Perfil do professor criado com sucesso!');
        },
        error: (err) => {
          console.error(err);
          this.isLoading = false;
          this.triggerToast('Erro ao criar perfil do professor.', true);
        }
      });
    }
  }

  // --- SEGURANÇA --- //

  onSubmitPassword(event: Event) {
    event.preventDefault();
    if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
      this.triggerToast('Por favor, preencha todos os campos.', true);
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.triggerToast('As novas senhas não conferem!', true);
      return;
    }

    if (this.newPassword.length < 6) {
      this.triggerToast('A nova senha deve ter no mínimo 6 caracteres.', true);
      return;
    }

    this.isSubmitting = true;
    this.authService.changePassword(this.currentPassword, this.newPassword).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.currentPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';
        this.triggerToast('Sua senha foi alterada com sucesso!');
      },
      error: () => {
        this.isSubmitting = false;
        this.triggerToast('Erro ao alterar a senha. Verifique sua senha atual.', true);
      }
    });
  }
}
