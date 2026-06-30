import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

import { CoursesService } from '../../services/courses.service';

export interface ICourse {
  id: number;
  title: string;
  description: string;
  imgCoverLink: string;
  released: boolean;
}

@Component({
  selector: 'app-course-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './course-home.component.html',
  styleUrl: './course-home.component.scss'
})
export class CourseHomeComponent implements OnInit {
  searchTerm: string = '';
  selectedCategory: string = 'Todos';

  categories: string[] = ['Todos', 'Tecnologia', 'Dados', 'Algoritmos', 'Infraestrutura', 'Design'];

  courses: ICourse[] = [];

  isLoading: boolean = true;

  constructor(
    private router: Router, 
    private coursesService: CoursesService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadCourses();
  }

  async loadCourses() {
    this.isLoading = true;
    try {
      this.courses = await this.coursesService.getMyCourses();
    } catch (error) {
      console.error('Erro ao carregar cursos', error);
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }

  selectCategory(category: string) {
    this.selectedCategory = category;
  }

  getFilteredCourses(): ICourse[] {
    return this.courses.filter(course => {
      // Temporarily removed category filtering logic since category doesn't exist yet
      // const matchesCategory = this.selectedCategory === 'Todos' || course.category === this.selectedCategory;
      const matchesSearch = course.title.toLowerCase().includes(this.searchTerm.toLowerCase());
      return matchesSearch;
    });
  }

  navigateDetail(courseId: number) {
    this.router.navigate(['courses/course-detail', courseId]);
  }
}
