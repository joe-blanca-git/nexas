import { Routes } from "@angular/router";
import { CourseDetailComponent } from "./pages/course-detail/course-detail.component";
import { CourseHomeComponent } from "./pages/course-home/course-home.component";

export const coursesRoutes: Routes = [
    {
        path: '',
        component: CourseHomeComponent,
        title: 'Cursos',
    },
    {
        path: 'course-detail/:id',
        component: CourseDetailComponent,
        title: 'Detalhes do Curso',
    },
    {
        path: 'lesson/:courseId',
        loadComponent: () => import('./pages/lesson-viewer/lesson-viewer.component').then(m => m.LessonViewerComponent),
        title: 'Aula',
    }
];