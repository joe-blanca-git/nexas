import { Component, OnInit, inject, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { SettingsService } from '../../services/settings.service';
import { ToastService } from '../../../../../core/services/toast.service';
import { BreadcrumbComponent } from '../../../../../shared/components/breadcrumb/breadcrumb.component';
import { AgivysService, AgivysRole } from '../../services/agivys.service';

declare var bootstrap: any;

@Component({
  selector: 'app-settings-home',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, BreadcrumbComponent],
  templateUrl: './settings-home.component.html',
  styleUrl: './settings-home.component.scss'
})
export class SettingsHomeComponent implements OnInit {
  private settingsService = inject(SettingsService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);
  private toastService = inject(ToastService);
  private agivysService = inject(AgivysService);

  activeTab: 'categories' | 'teachers' | 'administrators' = 'categories';
  activeCategoryTab: 'courses' | 'forums' = 'courses';
  isLoading = false;

  categories: any[] = [];
  forumCategories: any[] = [];
  teachers: any[] = [];
  systemUsers: any[] = [];
  agivysRoles: AgivysRole[] = [];
  avatarPreview: string | null = null;

  categoryForm!: FormGroup;
  teacherForm!: FormGroup;
  adminForm!: FormGroup;

  categoryToEdit: any | null = null;
  categoryToDelete: any | null = null;

  teacherToEdit: any | null = null;
  teacherToDelete: any | null = null;
  
  adminToDelete: any | null = null;

  isSubmittingCategory = false;
  isDeletingCategory = false;

  isSubmittingTeacher = false;
  isDeletingTeacher = false;

  isSubmittingAdmin = false;
  isDeletingAdmin = false;

  @ViewChild('categoryModal') categoryModalRef!: ElementRef;
  @ViewChild('confirmDeleteCategoryModal') confirmDeleteCategoryModalRef!: ElementRef;
  @ViewChild('teacherModal') teacherModalRef!: ElementRef;
  @ViewChild('confirmDeleteTeacherModal') confirmDeleteTeacherModalRef!: ElementRef;
  @ViewChild('adminModal') adminModalRef!: ElementRef;
  @ViewChild('confirmDeleteAdminModal') confirmDeleteAdminModalRef!: ElementRef;

  private categoryModalInstance: any;
  private confirmDeleteCategoryModalInstance: any;
  private teacherModalInstance: any;
  private confirmDeleteTeacherModalInstance: any;
  private adminModalInstance: any;
  private confirmDeleteAdminModalInstance: any;

  ngOnInit(): void {
    this.initForms();
    this.loadSystemUsers();
    this.loadAgivysRoles();
    this.loadData();
  }

  ngAfterViewInit() {
    this.categoryModalInstance = new bootstrap.Modal(this.categoryModalRef.nativeElement);
    this.confirmDeleteCategoryModalInstance = new bootstrap.Modal(this.confirmDeleteCategoryModalRef.nativeElement);
    this.teacherModalInstance = new bootstrap.Modal(this.teacherModalRef.nativeElement);
    this.confirmDeleteTeacherModalInstance = new bootstrap.Modal(this.confirmDeleteTeacherModalRef.nativeElement);
    this.adminModalInstance = new bootstrap.Modal(this.adminModalRef.nativeElement);
    this.confirmDeleteAdminModalInstance = new bootstrap.Modal(this.confirmDeleteAdminModalRef.nativeElement);
  }

  initForms() {
    this.categoryForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      icon: ['']
    });

    this.teacherForm = this.fb.group({
      name: ['', Validators.required],
      role: [''],
      position: [''],
      avatar: [''],
      bio: [''],
      instagramLink: [''],
      linkedinLink: [''],
      idAgivys: ['']
    });

    this.adminForm = this.fb.group({
      teacherId: ['', Validators.required],
      roleName: ['', Validators.required]
    });
  }

  switchTab(tab: 'categories' | 'teachers' | 'administrators') {
    this.activeTab = tab;
  }

  switchCategoryTab(tab: 'courses' | 'forums') {
    this.activeCategoryTab = tab;
  }

  loadData() {
    this.isLoading = true;
    this.categories = [];
    this.forumCategories = [];
    this.teachers = [];
    
    forkJoin({
      categories: this.settingsService.getCategories(),
      forumCategories: this.settingsService.getForumCategories(),
      teachers: this.settingsService.getTeachers()
    }).subscribe({
      next: (res) => {
        this.categories = res.categories;
        this.forumCategories = res.forumCategories;
        this.teachers = res.teachers;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading data', err);
        this.toastService.error('Erro ao carregar dados do sistema.');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  get administrators() {
    return this.teachers.filter(t => t.role === 'Admin');
  }

  get nonAdminTeachers() {
    return this.teachers.filter(t => t.role !== 'Admin');
  }

  loadAgivysRoles() {
    this.agivysService.getRoles().subscribe({
      next: (roles) => {
        this.agivysRoles = roles;
      },
      error: (err) => {
        console.error('Error loading Agivys roles', err);
        this.toastService.error('Erro ao carregar roles do Agivys.');
      }
    });
  }

  loadSystemUsers() {
    this.settingsService.getSystemUsers().subscribe({
      next: (users) => {
        this.systemUsers = users;
      },
      error: (err) => {
        console.error('Error loading system users', err);
      }
    });
  }

  // --- Category Methods ---

  openAddCategoryModal() {
    this.categoryToEdit = null;
    this.categoryForm.reset();
    this.categoryModalInstance.show();
  }

  openEditCategoryModal(category: any) {
    this.categoryToEdit = category;
    this.categoryForm.patchValue({
      name: category.name,
      description: category.description,
      icon: category.icon || ''
    });
    this.categoryModalInstance.show();
  }

  saveCategory() {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.isSubmittingCategory = true;
    const payload = this.categoryForm.value;

    if (this.categoryToEdit) {
      payload.id = this.categoryToEdit.id;
      
      const updateRequest = this.activeCategoryTab === 'courses' 
        ? this.settingsService.updateCategory(this.categoryToEdit.id, payload)
        : this.settingsService.updateForumCategory(this.categoryToEdit.id, { ...payload, active: this.categoryToEdit.active });

      updateRequest.subscribe({
        next: () => {
          this.toastService.success('Categoria atualizada com sucesso!');
          this.isSubmittingCategory = false;
          this.categoryModalInstance.hide();
          this.loadData();
        },
        error: (err) => {
          console.error('Error updating category', err);
          this.toastService.error('Erro ao atualizar categoria.');
          this.isSubmittingCategory = false;
        }
      });
    } else {
      const createRequest = this.activeCategoryTab === 'courses'
        ? this.settingsService.createCategory(payload)
        : this.settingsService.createForumCategory(payload);

      createRequest.subscribe({
        next: () => {
          this.toastService.success('Categoria criada com sucesso!');
          this.isSubmittingCategory = false;
          this.categoryModalInstance.hide();
          this.loadData();
        },
        error: (err) => {
          console.error('Error creating category', err);
          this.toastService.error('Erro ao criar categoria.');
          this.isSubmittingCategory = false;
        }
      });
    }
  }

  confirmDeleteCategory(category: any) {
    this.categoryToDelete = category;
    this.confirmDeleteCategoryModalInstance.show();
  }

  executeDeleteCategory() {
    if (!this.categoryToDelete) return;

    this.isDeletingCategory = true;
    
    const deleteRequest = this.activeCategoryTab === 'courses'
      ? this.settingsService.deleteCategory(this.categoryToDelete.id)
      : this.settingsService.deleteForumCategory(this.categoryToDelete.id);

    deleteRequest.subscribe({
      next: () => {
        this.toastService.success('Categoria excluída com sucesso!');
        this.isDeletingCategory = false;
        this.confirmDeleteCategoryModalInstance.hide();
        this.categoryToDelete = null;
        this.loadData();
      },
      error: (err) => {
        console.error('Error deleting category', err);
        this.toastService.error('Erro ao excluir categoria.');
        this.isDeletingCategory = false;
      }
    });
  }

  // --- Teacher Methods ---

  openAddTeacherModal() {
    this.teacherToEdit = null;
    this.avatarPreview = null;
    this.teacherForm.reset();
    this.teacherModalInstance.show();
  }

  openEditTeacherModal(teacher: any) {
    this.teacherToEdit = teacher;
    this.teacherForm.patchValue({
      name: teacher.name,
      role: teacher.role,
      position: teacher.position,
      avatar: teacher.avatar,
      bio: teacher.bio,
      instagramLink: teacher.instagramLink,
      linkedinLink: teacher.linkedinLink,
      idAgivys: teacher.idAgivys
    });
    this.avatarPreview = teacher.avatar || null;
    this.teacherModalInstance.show();
  }

  saveTeacher() {
    if (this.teacherForm.invalid) {
      this.teacherForm.markAllAsTouched();
      return;
    }

    this.isSubmittingTeacher = true;
    const payload = this.teacherForm.value;

    if (this.teacherToEdit) {
      payload.id = this.teacherToEdit.id;
      this.settingsService.updateTeacher(this.teacherToEdit.id, payload).subscribe({
        next: () => {
          if (payload.idAgivys && payload.idAgivys !== this.teacherToEdit.idAgivys) {
            this.agivysService.assignRole(payload.idAgivys, 'Teacher').subscribe({
              next: () => {
                this.toastService.success('Professor e acessos atualizados com sucesso!');
                this.isSubmittingTeacher = false;
                this.teacherModalInstance.hide();
                this.loadData();
              },
              error: (err) => {
                console.error('Error updating Agivys role', err);
                this.toastService.error('Professor atualizado (Falha ao vincular role Agivys).');
                this.isSubmittingTeacher = false;
                this.teacherModalInstance.hide();
                this.loadData();
              }
            });
          } else {
            this.toastService.success('Professor atualizado com sucesso!');
            this.isSubmittingTeacher = false;
            this.teacherModalInstance.hide();
            this.loadData();
          }
        },
        error: (err) => {
          console.error('Error updating teacher', err);
          this.toastService.error('Erro ao atualizar professor.');
          this.isSubmittingTeacher = false;
        }
      });
    } else {
      this.settingsService.createTeacher(payload).subscribe({
        next: () => {
          if (payload.idAgivys) {
            this.agivysService.assignRole(payload.idAgivys, 'Teacher').subscribe({
              next: () => {
                this.toastService.success('Professor criado com acesso ao portal configurado!');
                this.isSubmittingTeacher = false;
                this.teacherModalInstance.hide();
                this.loadData();
              },
              error: (err) => {
                console.error('Error assigning Teacher role in Agivys', err);
                this.toastService.error('Professor criado, mas erro ao dar acesso na API (Agivys).');
                this.isSubmittingTeacher = false;
                this.teacherModalInstance.hide();
                this.loadData();
              }
            });
          } else {
            this.toastService.success('Professor criado com sucesso!');
            this.isSubmittingTeacher = false;
            this.teacherModalInstance.hide();
            this.loadData();
          }
        },
        error: (err) => {
          console.error('Error creating teacher', err);
          this.toastService.error('Erro ao criar professor.');
          this.isSubmittingTeacher = false;
        }
      });
    }
  }

  onUserSelect(event: any) {
    const selectedExternalId = event.target.value;
    this.teacherForm.patchValue({ idAgivys: selectedExternalId });
  }

  onAvatarSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 1024 * 1024) { // 1MB
        this.toastService.error('A imagem deve ter no máximo 1MB.');
        event.target.value = ''; // Reset file input
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

  confirmDeleteTeacher(teacher: any) {
    this.teacherToDelete = teacher;
    this.confirmDeleteTeacherModalInstance.show();
  }

  executeDeleteTeacher() {
    if (!this.teacherToDelete) return;

    this.isDeletingTeacher = true;
    this.settingsService.deleteTeacher(this.teacherToDelete.id).subscribe({
      next: () => {
        if (this.teacherToDelete.idAgivys) {
          this.agivysService.removeRole(this.teacherToDelete.idAgivys, 'Teacher').subscribe({
            next: () => {
              this.toastService.success('Professor inativado e acessos removidos com sucesso!');
              this.isDeletingTeacher = false;
              this.confirmDeleteTeacherModalInstance.hide();
              this.teacherToDelete = null;
              this.loadData();
            },
            error: (err) => {
              console.error('Error removing Teacher role', err);
              this.toastService.error('Professor inativado (Erro ao remover acesso na API).');
              this.isDeletingTeacher = false;
              this.confirmDeleteTeacherModalInstance.hide();
              this.teacherToDelete = null;
              this.loadData();
            }
          });
        } else {
          this.toastService.success('Professor inativado com sucesso!');
          this.isDeletingTeacher = false;
          this.confirmDeleteTeacherModalInstance.hide();
          this.teacherToDelete = null;
          this.loadData();
        }
      },
      error: (err) => {
        console.error('Error deleting teacher', err);
        this.toastService.error('Erro ao inativar professor.');
        this.isDeletingTeacher = false;
      }
    });
  }

  // --- Administrator Methods ---

  openAddAdminModal() {
    this.adminForm.reset();
    this.adminForm.patchValue({ teacherId: '', roleName: 'Admin' });
    this.adminModalInstance.show();
  }

  saveAdmin() {
    if (this.adminForm.invalid) {
      this.adminForm.markAllAsTouched();
      return;
    }

    const payload = this.adminForm.getRawValue();
    const teacher = this.teachers.find(t => t.id === parseInt(payload.teacherId, 10));

    if (!teacher) {
      this.toastService.error('Professor não encontrado.');
      return;
    }

    if (!teacher.idAgivys) {
      this.toastService.error('Professor selecionado não possui ID do Agivys vinculado.');
      return;
    }

    this.isSubmittingAdmin = true;

    // 1. Vincular Role no Agivys
    this.agivysService.assignRole(teacher.idAgivys, payload.roleName).subscribe({
      next: () => {
        // 2. Atualizar Role no Nexas para 'Admin'
        const updatedTeacher = { ...teacher, role: 'Admin' };
        this.settingsService.updateTeacher(teacher.id, updatedTeacher).subscribe({
          next: () => {
            this.toastService.success('Privilégio de Administrador concedido com sucesso!');
            this.isSubmittingAdmin = false;
            this.adminModalInstance.hide();
            this.loadData();
          },
          error: (err) => {
            console.error('Error updating Nexas teacher role', err);
            this.toastService.error('A role foi vinculada no Agivys, mas falhou ao atualizar no Nexas.');
            this.isSubmittingAdmin = false;
          }
        });
      },
      error: (err) => {
        console.error('Error assigning Agivys role', err);
        this.toastService.error('Erro ao vincular role no Agivys. Verifique se o usuário já possui a role.');
        this.isSubmittingAdmin = false;
      }
    });
  }

  confirmDeleteAdmin(admin: any) {
    this.adminToDelete = admin;
    this.confirmDeleteAdminModalInstance.show();
  }

  executeDeleteAdmin() {
    if (!this.adminToDelete) return;

    if (!this.adminToDelete.idAgivys) {
      this.toastService.error('Professor não possui ID do Agivys vinculado para revogar.');
      return;
    }

    this.isDeletingAdmin = true;
    
    // Revogar role no Agivys. Nota: Precisaríamos saber a roleName exata. 
    // Como a UI não armazena a roleName do Agivys na tabela Nexas, vamos remover 'Admin'
    // que é a role padrão correspondente a aba de Administradores.
    const roleToRemove = 'Admin'; 

    this.agivysService.removeRole(this.adminToDelete.idAgivys, roleToRemove).subscribe({
      next: () => {
        // Atualizar Nexas para 'Teacher'
        const updatedTeacher = { ...this.adminToDelete, role: 'Teacher' };
        this.settingsService.updateTeacher(this.adminToDelete.id, updatedTeacher).subscribe({
          next: () => {
            this.toastService.success('Privilégio revogado com sucesso!');
            this.isDeletingAdmin = false;
            this.confirmDeleteAdminModalInstance.hide();
            this.adminToDelete = null;
            this.loadData();
          },
          error: (err) => {
            console.error('Error updating Nexas role to Teacher', err);
            this.toastService.error('Role removida no Agivys, mas falha ao atualizar no Nexas.');
            this.isDeletingAdmin = false;
          }
        });
      },
      error: (err) => {
        console.error('Error removing Agivys role', err);
        // Mesmo se falhar (ex: role não encontrada), queremos forçar a atualização local?
        // Neste fluxo, bloqueamos se a API falhar.
        this.toastService.error('Erro ao revogar role no Agivys.');
        this.isDeletingAdmin = false;
      }
    });
  }
}
