import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SupportService, UserAccess, PurchasedCourse } from '../../services/support.service';
import { ToastService } from '../../../../../core/services/toast.service';

@Component({
  selector: 'app-support-access',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './support-access.component.html',
  styleUrl: './support-access.component.scss'
})
export class SupportAccessComponent implements OnInit {
  private supportService = inject(SupportService);
  private toastService = inject(ToastService);

  users: UserAccess[] = [];
  filteredUsers: UserAccess[] = [];
  selectedUser: UserAccess | null = null;
  
  isLoading = true;
  searchQuery = '';
  
  showConfirmModal = false;
  courseToProcess: PurchasedCourse | null = null;
  actionType: 'grant' | 'revoke' = 'grant';
  isProcessing = false;

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.supportService.getUsersAccess().subscribe({
      next: (data) => {
        this.users = data;
        this.applySearch();
        this.isLoading = false;
      },
      error: () => {
        this.toastService.error('Não foi possível carregar a lista de acesso.');
        this.isLoading = false;
      }
    });
  }

  onSearch(event: any): void {
    this.searchQuery = event.target.value;
    this.applySearch();
  }

  applySearch(): void {
    this.filteredUsers = this.users
      .filter(u => 
        u.name.toLowerCase().includes(this.searchQuery.toLowerCase()) || 
        u.email.toLowerCase().includes(this.searchQuery.toLowerCase())
      )
      .sort((a, b) => a.name.localeCompare(b.name));
  }

  userCourses: PurchasedCourse[] = [];
  isLoadingCourses = false;

  selectUser(user: UserAccess): void {
    this.selectedUser = user;
    this.loadUserCourses(user.id);
  }

  loadUserCourses(userId: number): void {
    this.isLoadingCourses = true;
    this.supportService.getUserCourses(userId).subscribe({
      next: (courses) => {
        this.userCourses = courses;
        this.isLoadingCourses = false;
      },
      error: () => {
        this.toastService.error('Erro ao carregar cursos do usuário.');
        this.isLoadingCourses = false;
      }
    });
  }

  // (Removido o método getPaymentBadgeClass se não for necessário na listagem nova, ou podemos manter para uso futuro)

  getAccessBadgeClass(status: string): string {
    if (status === 'Gratuito') return 'bg-info bg-opacity-10 text-info border-info';
    return status === 'Liberado' 
      ? 'bg-success bg-opacity-10 text-success border-success' 
      : 'bg-danger bg-opacity-10 text-danger border-danger';
  }

  openConfirmModal(course: PurchasedCourse, action: 'grant' | 'revoke'): void {
    this.courseToProcess = course;
    this.actionType = action;
    this.showConfirmModal = true;
  }

  closeConfirmModal(): void {
    this.showConfirmModal = false;
    this.courseToProcess = null;
  }

  confirmAction(): void {
    if (!this.selectedUser || !this.courseToProcess) return;

    this.isProcessing = true;
    
    if (this.actionType === 'grant') {
      this.supportService.grantCourseAccess(this.selectedUser.id, this.courseToProcess.id).subscribe({
        next: () => {
          this.toastService.success('Curso liberado com sucesso!');
          this.courseToProcess!.accessStatus = 'Liberado';
          this.selectedUser!.enrolledCoursesCount++;
          this.isProcessing = false;
          this.closeConfirmModal();
        },
        error: () => {
          this.toastService.error('Erro ao tentar liberar o curso.');
          this.isProcessing = false;
          this.closeConfirmModal();
        }
      });
    } else {
      this.supportService.revokeCourseAccess(this.selectedUser.id, this.courseToProcess.id).subscribe({
        next: () => {
          this.toastService.success('Acesso removido com sucesso!');
          this.courseToProcess!.accessStatus = 'Bloqueado';
          if (this.selectedUser!.enrolledCoursesCount > 0) this.selectedUser!.enrolledCoursesCount--;
          this.isProcessing = false;
          this.closeConfirmModal();
        },
        error: () => {
          this.toastService.error('Erro ao tentar remover o acesso.');
          this.isProcessing = false;
          this.closeConfirmModal();
        }
      });
    }
  }
}
