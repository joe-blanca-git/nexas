import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CoursesService } from '../../services/courses.service';
import { ToastService } from '../../../../../core/services/toast.service';
import { Course, Module, Lesson } from '../../models/course.model';
import { ModuleModalComponent } from '../../components/module-modal/module-modal.component';

@Component({
  selector: 'app-course-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, ModuleModalComponent],
  templateUrl: './course-form.component.html',
  styleUrl: './course-form.component.scss'
})
export class CourseFormComponent {
  private fb = inject(FormBuilder);
  private coursesService = inject(CoursesService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  courseForm: FormGroup;
  modules: Module[] = [];
  
  isSubmitting = false;

  // Modal State
  isModuleModalOpen = false;
  selectedModule: Module | null = null;
  editingModuleIndex: number = -1;

  constructor() {
    this.courseForm = this.fb.group({
      name: ['', Validators.required],
      descriptionSub: ['', Validators.required],
      description: ['', Validators.required],
      level: ['Iniciante', Validators.required],
      priceSingle: [0, [Validators.required, Validators.min(0)]],
      imgCoverLink: [''],
    });
  }

  get f() { return this.courseForm.controls; }

  // --- Module Management ---
  
  openModuleModal(moduleToEdit?: Module, index?: number) {
    if (moduleToEdit && index !== undefined) {
      this.selectedModule = JSON.parse(JSON.stringify(moduleToEdit)); // deep copy
      this.editingModuleIndex = index;
    } else {
      this.selectedModule = {
        id: Date.now(), // temporary mock ID
        name: '',
        description: '',
        descriptionSub: '',
        imgCoverLink: '',
        bunnyCollectionId: '',
        lessons: []
      };
      this.editingModuleIndex = -1;
    }
    this.isModuleModalOpen = true;
  }

  closeModuleModal() {
    this.isModuleModalOpen = false;
    this.selectedModule = null;
    this.editingModuleIndex = -1;
  }

  saveModule(moduleData: Module) {
    if (this.editingModuleIndex > -1) {
      this.modules[this.editingModuleIndex] = moduleData;
    } else {
      this.modules.push(moduleData);
    }
    this.closeModuleModal();
  }

  removeModule(index: number) {
    if (confirm('Tem certeza que deseja excluir este módulo? Todas as aulas dentro dele serão perdidas.')) {
      this.modules.splice(index, 1);
    }
  }

  moveModuleUp(index: number) {
    if (index > 0) {
      const temp = this.modules[index];
      this.modules[index] = this.modules[index - 1];
      this.modules[index - 1] = temp;
    }
  }

  moveModuleDown(index: number) {
    if (index < this.modules.length - 1) {
      const temp = this.modules[index];
      this.modules[index] = this.modules[index + 1];
      this.modules[index + 1] = temp;
    }
  }

  // --- Course Submission ---

  onSubmit() {
    if (this.courseForm.invalid) {
      this.courseForm.markAllAsTouched();
      this.toastService.warning('Preencha todos os campos obrigatórios.');
      return;
    }

    this.isSubmitting = true;

    const courseData: Course = {
      ...this.courseForm.value,
      id: Date.now(),
      bunnyLibraryId: '',
      modules: this.modules,
      domains: [],
      teachers: [],
      categories: []
    };

    this.coursesService.createCourse(courseData).subscribe({
      next: () => {
        this.toastService.success('Curso salvo com sucesso!');
        this.isSubmitting = false;
        this.router.navigate(['/courses']);
      },
      error: (err) => {
        this.toastService.error('Erro ao salvar o curso.');
        console.error(err);
        this.isSubmitting = false;
      }
    });
  }
}
