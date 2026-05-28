import { Routes } from '@angular/router';
import { ForumHomeComponent } from './pages/forum-home/forum-home.component';

export const forumRoutes: Routes = [
    {
        path: '',
        component: ForumHomeComponent,
        title: 'Fórum da Comunidade'
    }
];
