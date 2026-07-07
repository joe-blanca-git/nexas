import { Routes } from '@angular/router';
import { ForumHomeComponent } from './pages/forum-home/forum-home.component';
import { ForumTopicComponent } from './pages/forum-topic/forum-topic.component';

export const forumRoutes: Routes = [
    {
        path: '',
        component: ForumHomeComponent,
        title: 'Fórum da Comunidade'
    },
    {
        path: 'topic/:id',
        component: ForumTopicComponent,
        title: 'Tópico do Fórum'
    }
];
