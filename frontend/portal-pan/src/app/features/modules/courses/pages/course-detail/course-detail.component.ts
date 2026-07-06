import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CoursesService } from '../../services/courses.service';

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './course-detail.component.html',
  styleUrl: './course-detail.component.scss'
})
export class CourseDetailComponent implements OnInit {
  courseId: number = 0;
  course: any = null;
  isLoading: boolean = true;
  errorMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private coursesService: CoursesService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.courseId = +idParam;
        this.loadCourseData();
      } else {
        this.errorMessage = 'ID do curso não fornecido.';
        this.isLoading = false;
      }
    });
  }

  async loadCourseData() {
    this.isLoading = true;
    this.errorMessage = '';
    try {
      this.course = await this.coursesService.getCourseDetail(this.courseId);
    } catch (error) {
      console.error(error);
      this.errorMessage = 'Erro ao carregar os detalhes do curso.';
    } finally {
      this.isLoading = false;
    }
  }

  goBack() {
    this.router.navigate(['/courses']);
  }

  goToLesson() {
    if (!this.course) return;

    if (this.course.progressPercentage !== 100) {
      if (this.course.lastViewedLesson && this.course.lastViewedLesson.lessonId) {
        // Continue from last viewed
        this.router.navigate(['/courses/lesson', this.courseId], { queryParams: { lessonId: this.course.lastViewedLesson.lessonId } });
      } else {
        this.router.navigate(['/courses/lesson', this.courseId]);
      }
    } else {
      alert('Certificado será gerado em breve!');
    }
  }

  getTeacherInitials(teacherName: string): string {
    if (!teacherName) return 'PF';
    const parts = teacherName.split(' ');
    if (parts.length > 1) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return teacherName.substring(0, 2).toUpperCase();
  }
}
