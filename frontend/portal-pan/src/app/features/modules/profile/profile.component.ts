import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StateUtil } from '../../../core/utils/UserState.util';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  private readonly stateUtil = inject(StateUtil);

  userName: string = '';
  userEmail: string = '';
  userRole: string = '';
  
  // Mock Password Form
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  showToast = false;
  toastMessage = '';
  isSubmitting = false;
  toastClass = 'bg-success text-white';

  ngOnInit() {
    this.stateUtil.getUser().subscribe(user => {
      if (user) {
        this.userName = user.name || 'Usuário Não Informado';
        this.userEmail = user.email || 'Não informado';
        
        // Find main role
        if (user.roles && user.roles.length > 0) {
          this.userRole = user.roles[0].value || user.roles[0].name || 'Membro';
        } else {
          this.userRole = 'Membro';
        }
      }
    });
  }

  getInitials(name: string): string {
    if (!name || name === 'Usuário Não Informado') return 'U';
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return name[0].toUpperCase();
  }

  triggerToast(message: string, isError = false) {
    this.toastMessage = message;
    this.toastClass = isError ? 'bg-danger text-white' : 'bg-success text-white';
    this.showToast = true;
    setTimeout(() => {
      this.showToast = false;
    }, 4000);
  }

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
    // Simulando chamada para backend
    setTimeout(() => {
      this.isSubmitting = false;
      this.currentPassword = '';
      this.newPassword = '';
      this.confirmPassword = '';
      this.triggerToast('Sua senha foi alterada com sucesso!');
    }, 1200);
  }
}
