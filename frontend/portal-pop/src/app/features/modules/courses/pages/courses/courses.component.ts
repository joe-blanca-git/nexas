import { Component, OnInit, inject, ViewChild, ElementRef, ChangeDetectorRef, HostListener } from '@angular/core';
import * as tus from 'tus-js-client';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CoursesService } from '../../services/courses.service';
import { Course } from '../../models/course.model';

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

  selectedCourseId: number | null = null;
  selectedCourseForLesson: Course | null = null;
  
  isSubmittingCourse = false;
  isSubmittingModule = false;
  isSavingLesson = false;

  categories: any[] = [];

  // Cover Image State
  selectedCoverImage: File | null = null;
  coverImagePreview: string | null = null;
  coverImageError: string | null = null;

  // Lesson Video State
  selectedLessonVideo: File | null = null;
  lessonVideoError: string | null = null;

  // TUS Upload State
  uploadProgress: number = 0;
  uploadStatusMessage: string = '';
  tusUploadInstance: tus.Upload | null = null;

  @ViewChild('courseModal') courseModalRef!: ElementRef;
  @ViewChild('moduleModal') moduleModalRef!: ElementRef;
  @ViewChild('lessonModal') lessonModalRef!: ElementRef;

  private courseModalInstance: any;
  private moduleModalInstance: any;
  private lessonModalInstance: any;

  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    if (this.isSavingLesson && this.tusUploadInstance) {
      $event.returnValue = true;
    }
  }

  ngOnInit(): void {
    this.initForms();
    this.loadCategories();
    this.loadCourses();
  }

  ngAfterViewInit() {
    this.courseModalInstance = new bootstrap.Modal(this.courseModalRef.nativeElement);
    this.moduleModalInstance = new bootstrap.Modal(this.moduleModalRef.nativeElement);
    this.lessonModalInstance = new bootstrap.Modal(this.lessonModalRef.nativeElement);
  }

  initForms() {
    this.courseForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      descriptionSub: [''],
      level: ['Iniciante', Validators.required],
      priceSingle: [0, [Validators.required, Validators.min(0)]],
      imgCoverLink: [''],
      categoryId: ['', Validators.required]
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
      durationSeconds: [0, [Validators.required, Validators.min(1)]]
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

  loadCourses() {
    this.isLoading = true;
    this.coursesService.getCourses().subscribe({
      next: (data) => {
        this.courses = data;
        this.totalCourses = data.length;
        
        let modulesCount = 0;
        let lessonsCount = 0;
        let totalWorkload = 0;

        data.forEach(course => {
          modulesCount += course.modules ? course.modules.length : 0;
          totalWorkload += course.workloadHours || 0;
          if (course.modules) {
            course.modules.forEach(module => {
              lessonsCount += module.lessons ? module.lessons.length : 0;
            });
          }
        });

        this.totalModules = modulesCount;
        this.totalLessons = lessonsCount;
        this.averageWorkload = this.totalCourses > 0 ? Math.round(totalWorkload / this.totalCourses) : 0;

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

  openNewCourseModal() {
    this.courseForm.reset({ level: 'Iniciante', priceSingle: 0, categoryId: '' });
    this.selectedCoverImage = null;
    this.coverImagePreview = null;
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
      if (ratio < 1.7 || ratio > 1.8) { // Target is 1.77 (16:9)
        this.coverImageError = 'A imagem deve ter a proporção 16:9 (ex: 1280x720).';
        this.selectedCoverImage = null;
        this.coverImagePreview = null;
        event.target.value = '';
      } else {
        this.selectedCoverImage = file;
        this.coverImagePreview = img.src;
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

    this.coursesService.createCourse(payload).subscribe({
      next: (id) => {
        this.isSubmittingCourse = false;
        this.courseModalInstance.hide();
        this.loadCourses(); // Reload list to show the new course
      },
      error: (err) => {
        console.error('Error creating course', err);
        this.isSubmittingCourse = false;
      }
    });
  }

  openAddModuleModal(courseId: number) {
    this.selectedCourseId = courseId;
    this.moduleForm.reset();
    this.moduleModalInstance.show();
  }

  saveModule() {
    if (this.moduleForm.invalid || !this.selectedCourseId) {
      this.moduleForm.markAllAsTouched();
      return;
    }

    this.isSubmittingModule = true;
    const modulePayload = {
      ...this.moduleForm.value,
      courseId: this.selectedCourseId
    };

    this.coursesService.createModule(modulePayload).subscribe({
      next: (moduleId) => {
        this.finishModuleCreation();
      },
      error: (err) => {
        console.error('Error creating module', err);
        this.isSubmittingModule = false;
      }
    });
  }

  private finishModuleCreation() {
    this.isSubmittingModule = false;
    this.moduleModalInstance.hide();
    this.loadCourses(); // Refresh list to update module counts
  }

  getCourseLessonsCount(course: Course): number {
    if (!course.modules) return 0;
    return course.modules.reduce((acc, mod) => acc + (mod.lessons?.length || 0), 0);
  }

  openAddLessonModal(course: Course) {
    this.selectedCourseForLesson = course;
    this.lessonForm.reset({ moduleId: '', durationSeconds: 0 });
    this.selectedLessonVideo = null;
    this.lessonVideoError = null;
    this.uploadProgress = 0;
    this.uploadStatusMessage = '';
    this.tusUploadInstance = null;
    this.lessonModalInstance.show();
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

  saveLesson() {
    if (this.lessonForm.invalid) {
      this.lessonForm.markAllAsTouched();
      return;
    }

    this.isSavingLesson = true;
    this.uploadStatusMessage = 'Criando aula...';
    
    const payload = this.lessonForm.value;
    payload.moduleId = parseInt(payload.moduleId, 10);

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

  trackById(index: number, item: any): number {
    return item.id;
  }
}
