import { Routes } from '@angular/router';
import { BlogListComponent } from './pages/blog-list/blog-list.component';

export const blogRoutes: Routes = [
    {
        path: '',
        component: BlogListComponent
    }
];
