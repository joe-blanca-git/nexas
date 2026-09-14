import { Component, OnInit, inject, ViewChild, ElementRef, ChangeDetectorRef, HostListener } from '@angular/core';
import * as tus from 'tus-js-client';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CoursesService } from '../../services/courses.service';
import { Course } from '../../models/course.model';
import { ToastService } from '../../../../../core/services/toast.service';
import { AuthUtil } from '../../../../../core/auth/auth.util';

// Declare bootstrap variable to use native Bootstrap modals
declare var bootstrap: any;

@Component({
  selector: 'app-courses',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './courses.component.html',
  styleUrl: './courses.component.scss'
})
export class CoursesComponent implements OnInit {
  private coursesService = inject(CoursesService);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);
  private toastService = inject(ToastService);
  private authUtil = inject(AuthUtil);
  
  isAdmin = false;
  
  courses: Course[] = [];
  isLoading = true;

  // Metrics
  totalCourses = 0;
  totalModules = 0;
  totalLessons = 0;
  averageWorkload = 0;

  // Modals & State
  courseForm!: FormGroup;
  moduleForm!: FormGroup;
  lessonForm!: FormGroup;
  domainForm!: FormGroup;
  teacherAssignForm!: FormGroup;

  selectedCourseId: number | null = null;
  selectedCourseForLesson: Course | null = null;
  selectedCourseForModules: Course | null = null;
  courseToEdit: any | null = null;
  moduleToEdit: any | null = null;
  moduleToDelete: any | null = null;
  lessonToEdit: any | null = null;
  lessonToDelete: any | null = null;
  selectedCourseForLessons: Course | null = null;
  domainToEdit: any | null = null;
  domainToDelete: any | null = null;
  selectedCourseForDomains: Course | null = null;
  selectedCourseForTeachers: Course | null = null;
  availableTeachers: any[] = [];
  
  isSubmittingCourse = false;
  isSubmittingModule = false;
  isSavingLesson = false;
  isTogglingStatus = false;
  isDeletingModule = false;
  isDeletingLesson = false;
  isSubmittingDomain = false;
  isDeletingDomain = false;
  isAssigningTeacher = false;
  isUnassigningTeacher = false;
  teacherToUnassign: any = null;

  categories: any[] = [];

  // Cover Image State
  selectedCoverImage: File | null = null;
  coverImagePreview: string | null = null;
  coverImageError: string | null = null;

  // Lesson Video State
  selectedLessonVideo: File | null = null;
  lessonVideoError: string | null = null;

  // Lesson Thumbnail State
  selectedLessonThumbnail: File | null = null;
  lessonThumbnailPreview: string | null = null;
  lessonThumbnailError: string | null = null;

  // TUS Upload State
  uploadProgress: number = 0;
  uploadStatusMessage: string = '';
  tusUploadInstance: tus.Upload | null = null;

  @ViewChild('courseModal') courseModalRef!: ElementRef;
  @ViewChild('moduleModal') moduleModalRef!: ElementRef;
  @ViewChild('lessonModal') lessonModalRef!: ElementRef;
  @ViewChild('confirmToggleModal') confirmToggleModalRef!: ElementRef;
  @ViewChild('moduleListModal') moduleListModalRef!: ElementRef;
  @ViewChild('confirmDeleteModuleModal') confirmDeleteModuleModalRef!: ElementRef;
  @ViewChild('lessonListModal') lessonListModalRef!: ElementRef;
  @ViewChild('confirmDeleteLessonModal') confirmDeleteLessonModalRef!: ElementRef;
  @ViewChild('domainModal') domainModalRef!: ElementRef;
  @ViewChild('domainListModal') domainListModalRef!: ElementRef;
  @ViewChild('confirmDeleteDomainModal') confirmDeleteDomainModalRef!: ElementRef;
  @ViewChild('teacherListModal') teacherListModalRef!: ElementRef;
  @ViewChild('teacherAssignModal') teacherAssignModalRef!: ElementRef;
  @ViewChild('confirmUnassignTeacherModal') confirmUnassignTeacherModalRef!: ElementRef;

  private courseModalInstance: any;
  private moduleModalInstance: any;
  private lessonModalInstance: any;
  private confirmToggleModalInstance: any;
  private moduleListModalInstance: any;
  private confirmDeleteModuleModalInstance: any;
  private lessonListModalInstance: any;
  private confirmDeleteLessonModalInstance: any;
  private domainModalInstance: any;
  private domainListModalInstance: any;
  private confirmDeleteDomainModalInstance: any;
  private teacherListModalInstance: any;
  private teacherAssignModalInstance: any;
  private confirmUnassignTeacherModalInstance: any;

  courseToToggle: any = null;

  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    if (this.isSavingLesson && this.tusUploadInstance) {
      $event.returnValue = true;
    }
  }

  ngOnInit(): void {
    this.initForms();
    this.checkAdminStatus();
    this.loadCategories();
    this.loadTeachers();
    this.loadCourses();
  }

  checkAdminStatus() {
    this.isAdmin = this.authUtil.hasRole('Admin');
  }

  ngAfterViewInit() {
    this.courseModalInstance = new bootstrap.Modal(this.courseModalRef.nativeElement);
    this.moduleModalInstance = new bootstrap.Modal(this.moduleModalRef.nativeElement);
    this.lessonModalInstance = new bootstrap.Modal(this.lessonModalRef.nativeElement);
    this.confirmToggleModalInstance = new bootstrap.Modal(this.confirmToggleModalRef.nativeElement);
    this.moduleListModalInstance = new bootstrap.Modal(this.moduleListModalRef.nativeElement);
    this.confirmDeleteModuleModalInstance = new bootstrap.Modal(this.confirmDeleteModuleModalRef.nativeElement);
    this.lessonListModalInstance = new bootstrap.Modal(this.lessonListModalRef.nativeElement);
    this.confirmDeleteLessonModalInstance = new bootstrap.Modal(this.confirmDeleteLessonModalRef.nativeElement);
    this.domainModalInstance = new bootstrap.Modal(this.domainModalRef.nativeElement);
    this.domainListModalInstance = new bootstrap.Modal(this.domainListModalRef.nativeElement);
    this.confirmDeleteDomainModalInstance = new bootstrap.Modal(this.confirmDeleteDomainModalRef.nativeElement);
    this.teacherListModalInstance = new bootstrap.Modal(this.teacherListModalRef.nativeElement);
    this.teacherAssignModalInstance = new bootstrap.Modal(this.teacherAssignModalRef.nativeElement);
    this.confirmUnassignTeacherModalInstance = new bootstrap.Modal(this.confirmUnassignTeacherModalRef.nativeElement);
  }

  initForms() {
    this.courseForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      descriptionSub: [''],
      level: ['Iniciante', Validators.required],
      priceSingle: [0, [Validators.required, Validators.min(0)]],
      imgCoverLink: [''],
      categoryId: ['', Validators.required],
      isComingSoon: [false],
      releaseDate: ['']
    });

    this.moduleForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      descriptionSub: [''],
      imgCoverLink: ['']
    });

    this.lessonForm = this.fb.group({
      moduleId: ['', Validators.required],
      name: ['', Validators.required],
      description: [''],
      durationSeconds: [0, [Validators.required, Validators.min(1)]],
      thumbnail: ['']
    });

    this.domainForm = this.fb.group({
      title: ['', Validators.required],
      description: ['']
    });

    this.teacherAssignForm = this.fb.group({
      teacherId: ['', Validators.required]
    });
  }

  loadCategories() {
    this.coursesService.getCategories().subscribe({
      next: (data: any) => {
        this.categories = data;
      },
      error: (err) => {
        console.error('Error loading categories', err);
      }
    });
  }

  loadTeachers() {
    this.coursesService.getTeachers().subscribe({
      next: (data: any) => {
        this.availableTeachers = data.filter((t: any) => t.active !== false); // Assumes active is true or undefined for active teachers
      },
      error: (err) => {
        console.error('Error loading teachers', err);
      }
    });
  }

  loadCourses() {
    this.isLoading = true;
    this.coursesService.getCourses().subscribe({
      next: (data) => {
        this.courses = data;
        this.totalCourses = data.filter(c => c.active).length;
        
        let modulesCount = 0;
        let lessonsCount = 0;
        let totalSeconds = 0;

        data.forEach(course => {
          modulesCount += course.modules ? course.modules.length : 0;
          if (course.modules) {
            course.modules.forEach(module => {
              lessonsCount += module.lessons ? module.lessons.length : 0;
              if (module.lessons) {
                module.lessons.forEach(lesson => {
                  totalSeconds += lesson.durationSeconds || 0;
                });
              }
            });
          }
        });

        this.totalModules = modulesCount;
        this.totalLessons = lessonsCount;
        
        // Convert seconds to hours
        this.averageWorkload = Math.round(totalSeconds / 3600);

        if (this.selectedCourseForModules) {
          this.selectedCourseForModules = this.courses.find(c => c.id === this.selectedCourseForModules?.id) || null;
        }

        if (this.selectedCourseForLessons) {
          this.selectedCourseForLessons = this.courses.find(c => c.id === this.selectedCourseForLessons?.id) || null;
        }

        if (this.selectedCourseForDomains) {
          this.selectedCourseForDomains = this.courses.find(c => c.id === this.selectedCourseForDomains?.id) || null;
        }

        if (this.selectedCourseForTeachers) {
          this.selectedCourseForTeachers = this.courses.find(c => c.id === this.selectedCourseForTeachers?.id) || null;
        }

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading courses', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  toggleCourseStatus(course: any) {
    this.courseToToggle = course;
    this.confirmToggleModalInstance.show();
  }

  confirmToggleStatus() {
    if (!this.courseToToggle) return;
    
    this.isTogglingStatus = true;
    this.coursesService.toggleCourseStatus(this.courseToToggle.id).subscribe({
      next: (res) => {
        this.courseToToggle.active = res.active;
        this.totalCourses = this.courses.filter(c => c.active).length;
        this.confirmToggleModalInstance.hide();
        this.toastService.success(`Curso ${res.active ? 'ativado' : 'desativado'} com sucesso!`);
        this.courseToToggle = null;
        this.isTogglingStatus = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error toggling course status', err);
        this.toastService.error('Erro ao alterar status do curso.');
        this.confirmToggleModalInstance.hide();
        this.courseToToggle = null;
        this.isTogglingStatus = false;
      }
    });
  }

  openNewCourseModal() {
    this.courseToEdit = null;
    this.courseForm.reset({ level: 'Iniciante', priceSingle: 0, categoryId: '', isComingSoon: false, releaseDate: '' });
    this.selectedCoverImage = null;
    this.coverImagePreview = null;
    this.coverImageError = null;
    this.courseModalInstance.show();
  }

  openEditCourseModal(course: Course) {
    this.courseToEdit = course;
    this.courseForm.patchValue({
      name: course.name,
      description: course.description,
      descriptionSub: course.descriptionSub,
      level: course.level || 'Iniciante',
      priceSingle: course.priceSingle || 0,
      imgCoverLink: course.imgCoverLink,
      categoryId: course.categories && course.categories.length > 0 ? course.categories[0].id : '',
      isComingSoon: course.isComingSoon || false,
      releaseDate: course.releaseDate ? new Date(course.releaseDate).toISOString().split('T')[0] : ''
    });
    this.selectedCoverImage = null;
    this.coverImagePreview = course.imgCoverLink || null;
    this.coverImageError = null;
    this.courseModalInstance.show();
  }

  onCoverImageSelected(event: any) {
    const file = event.target.files[0] as File;
    this.coverImageError = null;

    if (!file) {
      this.selectedCoverImage = null;
      this.coverImagePreview = null;
      return;
    }

    if (file.size > 2 * 1024 * 1024) {
      this.coverImageError = 'A imagem deve ter no máximo 2MB.';
      event.target.value = '';
      return;
    }

    const img = new Image();
    img.src = URL.createObjectURL(file);
    img.onload = () => {
      const ratio = img.width / img.height;
      // Aceita se largura <= 1080 e altura <= 720 OU proporo 16:9
      if ((img.width <= 1080 && img.height <= 720) || (ratio >= 1.7 && ratio <= 1.8)) {
        this.selectedCoverImage = file;
        this.coverImagePreview = img.src;
      } else {
        this.coverImageError = 'A imagem deve ter no máximo 1080x720 ou proporção 16:9.';
        this.selectedCoverImage = null;
        this.coverImagePreview = null;
        event.target.value = '';
      }
    };
  }

  saveCourse() {
    if (this.courseForm.invalid) {
      this.courseForm.markAllAsTouched();
      return;
    }
    
    this.isSubmittingCourse = true;

    if (this.selectedCoverImage) {
      this.coursesService.uploadImage(this.selectedCoverImage).subscribe({
        next: (response) => {
          this.courseForm.patchValue({ imgCoverLink: response.url });
          this.submitCourseData();
        },
        error: (err) => {
          console.error('Error uploading image', err);
          this.isSubmittingCourse = false;
          this.coverImageError = 'Erro ao fazer upload da imagem no servidor.';
        }
      });
    } else {
      this.submitCourseData();
    }
  }

  private submitCourseData() {
    const payload = { ...this.courseForm.value };
    
    if (payload.categoryId) {
      payload.categoryIds = [parseInt(payload.categoryId, 10)];
    } else {
      payload.categoryIds = [];
    }
    delete payload.categoryId;

    if (this.courseToEdit) {
      payload.id = this.courseToEdit.id;
      if (this.courseToEdit.bunnyLibraryId) {
        payload.bunnyLibraryId = this.courseToEdit.bunnyLibraryId;
      }
      this.coursesService.updateCourse(payload).subscribe({
        next: () => {
          this.toastService.success('Curso atualizado com sucesso!');
          this.isSubmittingCourse = false;
          this.courseModalInstance.hide();
          this.loadCourses(); // Reload list to show updates
        },
        error: (err) => {
          console.error('Error updating course', err);
          this.toastService.error('Erro ao atualizar curso.');
          this.isSubmittingCourse = false;
        }
      });
    } else {
      this.coursesService.createCourse(payload).subscribe({
        next: (id) => {
          this.toastService.success('Curso criado com sucesso!');
          this.isSubmittingCourse = false;
          this.courseModalInstance.hide();
          this.loadCourses(); // Reload list to show the new course
        },
        error: (err) => {
          console.error('Error creating course', err);
          this.toastService.error('Erro ao criar curso.');
          this.isSubmittingCourse = false;
        }
      });
    }
  }

  openAddModuleModal(courseId: number) {
    this.selectedCourseId = courseId;
    this.moduleToEdit = null;
    this.moduleForm.reset();
    
    this.moduleModalInstance.show();
  }

  openEditModuleModal(module: any, courseId: number) {
    this.selectedCourseId = courseId;
    this.moduleToEdit = module;
    this.moduleForm.patchValue({
      name: module.name,
      description: module.description,
      descriptionSub: module.descriptionSub,
      imgCoverLink: module.imgCoverLink
    });

    this.moduleModalInstance.show();
  }

  openModuleListModal(course: Course) {
    this.selectedCourseForModules = course;
    this.moduleListModalInstance.show();
  }

  closeModuleListModal() {
    this.moduleListModalInstance.hide();
    this.selectedCourseForModules = null;
  }

  saveModule() {
    if (this.moduleForm.invalid || !this.selectedCourseId) {
      this.moduleForm.markAllAsTouched();
      return;
    }

    this.isSubmittingModule = true;

    if (this.moduleToEdit) {
      const modulePayload = {
        ...this.moduleForm.value,
        id: this.moduleToEdit.id,
        bunnyCollectionId: this.moduleToEdit.bunnyCollectionId || undefined
      };

      this.coursesService.updateModule(modulePayload).subscribe({
        next: () => {
          this.toastService.success('Módulo atualizado com sucesso!');
          this.finishModuleCreation();
        },
        error: (err) => {
          console.error('Error updating module', err);
          this.toastService.error('Erro ao atualizar módulo.');
          this.isSubmittingModule = false;
        }
      });
    } else {
      const modulePayload = {
        ...this.moduleForm.value,
        courseId: this.selectedCourseId
      };

      this.coursesService.createModule(modulePayload).subscribe({
        next: (moduleId) => {
          this.toastService.success('Módulo criado com sucesso!');
          this.finishModuleCreation();
        },
        error: (err) => {
          console.error('Error creating module', err);
          this.toastService.error('Erro ao criar módulo.');
          this.isSubmittingModule = false;
        }
      });
    }
  }

  private finishModuleCreation() {
    this.isSubmittingModule = false;
    this.moduleModalInstance.hide();
    this.loadCourses(); // Refresh list to update module counts
  }

  confirmDeleteModule(module: any) {
    this.moduleToDelete = module;
    this.confirmDeleteModuleModalInstance.show();
  }

  executeDeleteModule() {
    if (!this.moduleToDelete) return;

    this.isDeletingModule = true;
    this.coursesService.deleteModule(this.moduleToDelete.id).subscribe({
      next: () => {
        this.toastService.success('Módulo excluído com sucesso!');
        this.confirmDeleteModuleModalInstance.hide();
        this.isDeletingModule = false;
        this.moduleToDelete = null;
        this.loadCourses();
      },
      error: (err) => {
        console.error('Error deleting module', err);
        this.toastService.error('Erro ao excluir módulo.');
        this.isDeletingModule = false;
      }
    });
  }

  getCourseLessonsCount(course: Course): number {
    if (!course.modules) return 0;
    return course.modules.reduce((acc, mod) => acc + (mod.lessons?.length || 0), 0);
  }

  openLessonListModal(course: Course) {
    this.selectedCourseForLessons = course;
    this.lessonListModalInstance.show();
  }

  closeLessonListModal() {
    this.lessonListModalInstance.hide();
    this.selectedCourseForLessons = null;
  }

  openAddLessonModal(course?: Course) {
    if (course) {
      this.selectedCourseForLesson = course;
    } else if (this.selectedCourseForLessons) {
      this.selectedCourseForLesson = this.selectedCourseForLessons;
    }
    this.lessonToEdit = null;
    this.lessonForm.reset({ moduleId: '', durationSeconds: 0, thumbnail: '' });
    this.selectedLessonVideo = null;
    this.lessonVideoError = null;
    this.selectedLessonThumbnail = null;
    this.lessonThumbnailPreview = null;
    this.lessonThumbnailError = null;
    this.uploadProgress = 0;
    this.uploadStatusMessage = '';
    this.tusUploadInstance = null;
    
    this.lessonModalInstance.show();
  }

  openEditLessonModal(lesson: any, moduleId: number, courseId: number) {
    if (this.courses) {
       this.selectedCourseForLesson = this.courses.find(c => c.id === courseId) || null;
    }
    this.lessonToEdit = lesson;
    this.lessonForm.patchValue({
      moduleId: moduleId,
      name: lesson.name,
      description: lesson.description,
      durationSeconds: lesson.durationSeconds,
      thumbnail: lesson.thumbnail || ''
    });
    this.selectedLessonVideo = null;
    this.lessonVideoError = null;
    this.selectedLessonThumbnail = null;
    this.lessonThumbnailPreview = lesson.thumbnail || null;
    this.lessonThumbnailError = null;
    this.uploadProgress = 0;
    this.uploadStatusMessage = '';
    this.tusUploadInstance = null;
    
    this.lessonModalInstance.show();
  }

  confirmDeleteLesson(lesson: any) {
    this.lessonToDelete = lesson;
    this.confirmDeleteLessonModalInstance.show();
  }

  executeDeleteLesson() {
    if (!this.lessonToDelete) return;

    this.isDeletingLesson = true;
    this.coursesService.deleteLesson(this.lessonToDelete.id).subscribe({
      next: () => {
        this.toastService.success('Aula excluída com sucesso!');
        this.isDeletingLesson = false;
        this.lessonToDelete = null;
        this.loadCourses();
      },
      error: (err) => {
        console.error('Error deleting lesson', err);
        this.toastService.error('Erro ao excluir aula.');
        this.isDeletingLesson = false;
      }
    });
  }

  onLessonVideoSelected(event: any) {
    const file = event.target.files[0] as File;
    this.lessonVideoError = null;

    if (!file) {
      this.selectedLessonVideo = null;
      return;
    }

    if (file.size > 2 * 1024 * 1024 * 1024) { // 2GB max for video
      this.lessonVideoError = 'O vídeo deve ter no máximo 2GB.';
      event.target.value = '';
      return;
    }

    this.selectedLessonVideo = file;

    // Calculate video duration automatically
    const video = document.createElement('video');
    video.preload = 'metadata';
    video.onloadedmetadata = () => {
      window.URL.revokeObjectURL(video.src);
      const duration = Math.round(video.duration);
      if (!isNaN(duration)) {
        this.lessonForm.patchValue({ durationSeconds: duration });
        this.cdr.detectChanges();
      }
    };
    video.src = URL.createObjectURL(file);
  }

  onLessonThumbnailChange(event: any) {
    const file = event.target.files[0];
    this.lessonThumbnailError = null;

    if (!file) {
      this.selectedLessonThumbnail = null;
      this.lessonThumbnailPreview = null;
      return;
    }

    if (file.size > 1.5 * 1024 * 1024) {
      this.lessonThumbnailError = 'A imagem deve ter no máximo 1.5MB.';
      this.selectedLessonThumbnail = null;
      this.lessonThumbnailPreview = null;
      event.target.value = '';
      return;
    }

    const img = new Image();
    img.onload = () => {
      const ratio = img.width / img.height;
      if ((img.width <= 1080 && img.height <= 720) || (ratio >= 1.7 && ratio <= 1.8)) {
        this.selectedLessonThumbnail = file;
        this.lessonThumbnailPreview = img.src;
        this.lessonThumbnailError = null;
      } else {
        this.lessonThumbnailError = 'A imagem deve ter no máximo 1080x720 ou proporção 16:9.';
        this.selectedLessonThumbnail = null;
        this.lessonThumbnailPreview = null;
        event.target.value = '';
      }
    };
    img.src = URL.createObjectURL(file);
  }

  removeLessonThumbnail() {
    this.selectedLessonThumbnail = null;
    this.lessonThumbnailPreview = null;
    this.lessonThumbnailError = null;
    this.lessonForm.patchValue({ thumbnail: '' });
  }

  saveLesson() {
    if (this.lessonForm.invalid) {
      this.lessonForm.markAllAsTouched();
      return;
    }

    this.isSavingLesson = true;

    if (this.selectedLessonThumbnail) {
      this.coursesService.uploadImage(this.selectedLessonThumbnail).subscribe({
        next: (response) => {
          this.lessonForm.patchValue({ thumbnail: response.url });
          this.submitLessonForm();
        },
        error: (err) => {
          console.error('Error uploading lesson thumbnail', err);
          this.toastService.error('Erro ao enviar a imagem de miniatura.');
          this.isSavingLesson = false;
        }
      });
    } else {
      this.submitLessonForm();
    }
  }

  private submitLessonForm() {
    const payload = this.lessonForm.value;
    payload.moduleId = parseInt(payload.moduleId, 10);

    if (this.lessonToEdit) {
      this.uploadStatusMessage = 'Atualizando aula...';
      payload.id = this.lessonToEdit.id;
      if (this.lessonToEdit.bunnyVideoId) {
        payload.bunnyVideoId = this.lessonToEdit.bunnyVideoId;
      }
      this.coursesService.updateLesson(payload).subscribe({
        next: () => {
          this.toastService.success('Aula atualizada com sucesso!');
          this.finishLessonUpload();
        },
        error: (err) => {
          console.error('Error updating lesson', err);
          this.uploadStatusMessage = 'Falha ao atualizar a aula.';
          this.isSavingLesson = false;
        }
      });
    } else {
      this.uploadStatusMessage = 'Criando aula...';
      this.coursesService.createLesson(payload).subscribe({
        next: (response: any) => {
          const lessonId = typeof response === 'object' ? response.id : response;
          if (!this.selectedLessonVideo) {
            // If no video, we are done
            this.finishLessonUpload();
            return;
          }
          this.startVideoUpload(lessonId, this.selectedLessonVideo);
        },
        error: (err) => {
          console.error('Error creating lesson', err);
          this.uploadStatusMessage = 'Falha ao criar rascunho da aula.';
          this.isSavingLesson = false;
        }
      });
    }
  }

  private startVideoUpload(lessonId: number, file: File) {
    this.uploadStatusMessage = 'Preparando upload...';

    this.coursesService.generateVideoUpload(lessonId).subscribe({
      next: (info) => {
        this.uploadStatusMessage = 'Enviando vídeo: 0%...';
        this.uploadProgress = 0;

        this.tusUploadInstance = new tus.Upload(file, {
          endpoint: info.uploadUrl,
          retryDelays: [0, 1000, 3000, 5000, 10000],
          headers: info.headers,
          metadata: {
            filename: file.name,
            filetype: file.type
          },
          onError: (error) => {
            console.error('Upload failed:', error);
            this.uploadStatusMessage = 'Erro no upload. Tente novamente mais tarde.';
            this.isSavingLesson = false;
            this.tusUploadInstance = null;
          },
          onProgress: (bytesUploaded, bytesTotal) => {
            const percentage = ((bytesUploaded / bytesTotal) * 100).toFixed(0);
            this.uploadProgress = parseInt(percentage, 10);
            this.uploadStatusMessage = `Enviando vídeo: ${percentage}%...`;
            this.cdr.detectChanges();
          },
          onSuccess: () => {
            this.uploadStatusMessage = 'Processando vídeo...';
            this.tusUploadInstance = null;
            
            // Call complete API
            this.coursesService.completeVideoUpload(lessonId).subscribe({
              next: () => {
                this.uploadStatusMessage = 'Concluído.';
                setTimeout(() => this.finishLessonUpload(), 1000);
              },
              error: (err) => {
                console.error('Error completing upload', err);
                this.uploadStatusMessage = 'Erro ao processar. O vídeo foi enviado, mas pode demorar a aparecer.';
                setTimeout(() => this.finishLessonUpload(), 3000);
              }
            });
          }
        });

        // Check if there are any previous uploads to continue (though we generate a fresh signature usually)
        this.tusUploadInstance.start();
      },
      error: (err) => {
        console.error('Error generating upload', err);
        this.uploadStatusMessage = 'Falha na preparação. O rascunho da aula foi salvo.';
        this.isSavingLesson = false;
      }
    });
  }

  cancelUpload() {
    if (this.tusUploadInstance) {
      this.tusUploadInstance.abort(true).then(() => {
        this.uploadStatusMessage = 'Upload cancelado.';
        this.isSavingLesson = false;
        this.tusUploadInstance = null;
        this.uploadProgress = 0;
      }).catch(err => {
        console.error('Failed to abort upload', err);
      });
    }
  }

  private finishLessonUpload() {
    this.isSavingLesson = false;
    this.uploadStatusMessage = '';
    this.uploadProgress = 0;
    this.lessonModalInstance.hide();
    this.loadCourses(); // refresh
  }

  openAddDomainModal(courseId: number) {
    this.selectedCourseId = courseId;
    this.domainToEdit = null;
    this.domainForm.reset();
    
    this.domainModalInstance.show();
  }

  openEditDomainModal(domain: any, courseId: number) {
    this.selectedCourseId = courseId;
    this.domainToEdit = domain;
    this.domainForm.patchValue({
      title: domain.title,
      description: domain.description
    });

    this.domainModalInstance.show();
  }

  openDomainListModal(course: Course) {
    this.selectedCourseForDomains = course;
    this.domainListModalInstance.show();
  }

  closeDomainListModal() {
    this.domainListModalInstance.hide();
    this.selectedCourseForDomains = null;
  }

  saveDomain() {
    if (this.domainForm.invalid || !this.selectedCourseId) {
      this.domainForm.markAllAsTouched();
      return;
    }

    this.isSubmittingDomain = true;

    if (this.domainToEdit) {
      const domainPayload = {
        ...this.domainForm.value,
        id: this.domainToEdit.id,
        courseId: this.selectedCourseId
      };

      this.coursesService.updateDomain(this.selectedCourseId, domainPayload).subscribe({
        next: () => {
          this.toastService.success('Domínio atualizado com sucesso!');
          this.finishDomainCreation();
        },
        error: (err) => {
          console.error('Error updating domain', err);
          this.toastService.error('Erro ao atualizar domínio.');
          this.isSubmittingDomain = false;
        }
      });
    } else {
      const domainPayload = {
        id: 0,
        ...this.domainForm.value,
        courseId: this.selectedCourseId
      };

      this.coursesService.createDomain(this.selectedCourseId, domainPayload).subscribe({
        next: () => {
          this.toastService.success('Domínio criado com sucesso!');
          this.finishDomainCreation();
        },
        error: (err) => {
          console.error('Error creating domain', err);
          this.toastService.error('Erro ao criar domínio.');
          this.isSubmittingDomain = false;
        }
      });
    }
  }

  private finishDomainCreation() {
    this.isSubmittingDomain = false;
    this.domainModalInstance.hide();
    this.loadCourses();
  }

  confirmDeleteDomain(domain: any, courseId: number) {
    this.selectedCourseId = courseId;
    this.domainToDelete = domain;
    this.confirmDeleteDomainModalInstance.show();
  }

  executeDeleteDomain() {
    if (!this.domainToDelete || !this.selectedCourseId) return;

    this.isDeletingDomain = true;
    this.coursesService.deleteDomain(this.selectedCourseId, this.domainToDelete.id).subscribe({
      next: () => {
        this.toastService.success('Domínio excluído com sucesso!');
        this.confirmDeleteDomainModalInstance.hide();
        this.isDeletingDomain = false;
        this.domainToDelete = null;
        this.loadCourses();
      },
      error: (err) => {
        console.error('Error deleting domain', err);
        this.toastService.error('Erro ao excluir domínio.');
        this.isDeletingDomain = false;
      }
    });
  }

  // --- Teachers Management ---

  openTeacherListModal(course: Course) {
    this.selectedCourseForTeachers = course;
    this.teacherListModalInstance.show();
  }

  closeTeacherListModal() {
    this.teacherListModalInstance.hide();
    this.selectedCourseForTeachers = null;
  }

  openAssignTeacherModal(courseId: number) {
    this.selectedCourseId = courseId;
    this.teacherAssignForm.reset({ teacherId: '' });
    this.teacherAssignModalInstance.show();
  }

  saveTeacherAssignment() {
    if (this.teacherAssignForm.invalid || !this.selectedCourseId) {
      this.teacherAssignForm.markAllAsTouched();
      return;
    }

    this.isAssigningTeacher = true;
    const teacherId = parseInt(this.teacherAssignForm.value.teacherId, 10);

    this.coursesService.assignTeacher(teacherId, this.selectedCourseId).subscribe({
      next: () => {
        this.toastService.success('Professor vinculado com sucesso!');
        this.isAssigningTeacher = false;
        this.teacherAssignModalInstance.hide();
        this.loadCourses();
      },
      error: (err) => {
        console.error('Error assigning teacher', err);
        this.toastService.error('Erro ao vincular professor.');
        this.isAssigningTeacher = false;
      }
    });
  }

  unassignTeacher(teacher: any) {
    if (!this.selectedCourseForTeachers) return;
    this.teacherToUnassign = teacher;
    this.confirmUnassignTeacherModalInstance.show();
  }

  executeUnassignTeacher() {
    if (!this.teacherToUnassign || !this.selectedCourseForTeachers) return;

    this.isUnassigningTeacher = true;
    const courseId = this.selectedCourseForTeachers.id;
    const teacherId = this.teacherToUnassign.id;

    this.coursesService.unassignTeacher(teacherId, courseId).subscribe({
      next: () => {
        this.toastService.success('Professor desvinculado com sucesso!');
        this.selectedCourseForTeachers!.teachers = this.selectedCourseForTeachers!.teachers.filter((t: any) => t.id !== teacherId);
        this.confirmUnassignTeacherModalInstance.hide();
        this.isUnassigningTeacher = false;
        this.teacherToUnassign = null;
        this.loadCourses();
      },
      error: (err) => {
        console.error('Error unassigning teacher', err);
        this.toastService.error('Erro ao desvincular professor.');
        this.isUnassigningTeacher = false;
      }
    });
  }

  trackById(index: number, item: any): number {
    return item.id;
  }
}
