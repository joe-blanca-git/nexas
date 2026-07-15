import { Component, OnInit, inject, ViewChild, ElementRef } from '@angular/core';
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
  pendingLessons: any[] = [];
  
  isSubmittingCourse = false;
  isSubmittingModule = false;

  @ViewChild('courseModal') courseModalRef!: ElementRef;
  @ViewChild('moduleModal') moduleModalRef!: ElementRef;

  private courseModalInstance: any;
  private moduleModalInstance: any;

  ngOnInit(): void {
    this.initForms();
    this.loadCourses();
  }

  ngAfterViewInit() {
    this.courseModalInstance = new bootstrap.Modal(this.courseModalRef.nativeElement);
    this.moduleModalInstance = new bootstrap.Modal(this.moduleModalRef.nativeElement);
  }

  initForms() {
    this.courseForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      descriptionSub: [''],
      level: ['Iniciante', Validators.required],
      priceSingle: [0, [Validators.required, Validators.min(0)]],
      imgCoverLink: [''],
      bunnyLibraryId: ['']
    });

    this.moduleForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      descriptionSub: [''],
      imgCoverLink: [''],
      bunnyCollectionId: ['']
    });

    this.lessonForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      durationSeconds: [0, [Validators.required, Validators.min(1)]],
      bunnyVideoId: ['', Validators.required]
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
      },
      error: (err) => {
        console.error('Error loading courses', err);
        this.isLoading = false;
      }
    });
  }

  openNewCourseModal() {
    this.courseForm.reset({ level: 'Iniciante', priceSingle: 0 });
    this.courseModalInstance.show();
  }

  saveCourse() {
    if (this.courseForm.invalid) {
      this.courseForm.markAllAsTouched();
      return;
    }
    
    this.isSubmittingCourse = true;
    this.coursesService.createCourse(this.courseForm.value).subscribe({
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
    this.lessonForm.reset({ durationSeconds: 0 });
    this.pendingLessons = [];
    this.moduleModalInstance.show();
  }

  addPendingLesson() {
    if (this.lessonForm.invalid) {
      this.lessonForm.markAllAsTouched();
      return;
    }
    this.pendingLessons.push(this.lessonForm.value);
    this.lessonForm.reset({ durationSeconds: 0 });
  }

  removePendingLesson(index: number) {
    this.pendingLessons.splice(index, 1);
  }

  saveModuleWithLessons() {
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
        if (this.pendingLessons.length === 0) {
          this.finishModuleCreation();
          return;
        }

        // Sequential POST for each lesson
        let completed = 0;
        this.pendingLessons.forEach(lesson => {
          const lessonPayload = {
            ...lesson,
            moduleId: moduleId
          };
          this.coursesService.createLesson(lessonPayload).subscribe({
            next: () => {
              completed++;
              if (completed === this.pendingLessons.length) {
                this.finishModuleCreation();
              }
            },
            error: (err) => {
              console.error('Error creating lesson', err);
              // In a real app, handle partial failure
              completed++;
              if (completed === this.pendingLessons.length) {
                this.finishModuleCreation();
              }
            }
          });
        });
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
}
