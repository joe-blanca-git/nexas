import { Routes } from '@angular/router';
import { CoursesComponent } from './pages/courses/courses.component';

export const coursesRoutes: Routes = [
  {
    path: '',
    component: CoursesComponent,
    title: 'Cursos',
  },
  {
    path: 'new',
    loadComponent: () => import('./pages/course-form/course-form.component').then(m => m.CourseFormComponent),
    title: 'Novo Curso'
  }
];
