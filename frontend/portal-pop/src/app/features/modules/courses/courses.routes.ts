import { Routes } from '@angular/router';
import { CoursesComponent } from './pages/courses/courses.component';

export const coursesRoutes: Routes = [
  {
    path: '',
    component: CoursesComponent,
    title: 'Cursos',
  }
];
