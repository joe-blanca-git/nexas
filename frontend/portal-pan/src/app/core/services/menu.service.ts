import { Injectable } from '@angular/core';
import { IMenuItem } from '../../features/shared/components/menu-side/menu-side.component';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class MenuService {

  constructor(private router: Router) { }

  getMenu(): IMenuItem[] {
    const menuMock = [
      {
        id: 1,
        title: 'Pagina Inicial',
        icon: 'far fa-house',
        route: '/home'
      },
      {
        id: 2,
        title: 'Cursos',
        icon: 'fas fa-book-reader',
        route: '/courses'
      },
      {
        id: 3,
        title: 'Certificados',
        icon: 'fas fa-award',
        route: '/certificates'
      },
      {
        id: 4,
        title: 'Financeiro',
        icon: 'fas fa-dollar',
        route: '/financial'
      },
      {
        id: 5,
        title: 'Fórum',
        icon: 'far fa-comment-dots',
        route: '/forum'
      },
      {
        id: 6,
        title: 'Suporte & FAQ',
        icon: 'far fa-headphones',
        route: '/support'
      }
    ];
    return menuMock;
  }

  getBreadCrumb() {
    const currentUrl = this.router.url.split('?')[0];
    const menuItems = this.getMenu();
    const breadcrumbs: { id?: number; title: string; route: string }[] = [];

    if (currentUrl === '/home' || currentUrl === '/') {
      const homeItem = menuItems.find(item => item.route === '/home');
      if (homeItem) {
        breadcrumbs.push({
          id: homeItem.id,
          title: homeItem.title,
          route: homeItem.route
        });
      }
      return breadcrumbs;
    }

    const matchedItem = menuItems.find(item =>
      item.route !== '/home' && currentUrl.startsWith(item.route)
    );

    if (matchedItem) {
      breadcrumbs.push({
        id: matchedItem.id,
        title: matchedItem.title,
        route: matchedItem.route
      });

      if (currentUrl !== matchedItem.route) {
        if (currentUrl.includes('course-detail')) {
          breadcrumbs.push({
            title: 'Detalhes do Curso',
            route: currentUrl
          });
        } else {
          // Fallback inteligente para outras sub-rotas
          const segments = currentUrl.split('/').filter(s => s);
          const lastSegment = segments[segments.length - 1];
          const displaySegment = isNaN(Number(lastSegment)) ? lastSegment : segments[segments.length - 2];

          if (displaySegment && displaySegment !== matchedItem.route.replace('/', '')) {
            const formattedTitle = displaySegment
              .split('-')
              .map(word => word.charAt(0).toUpperCase() + word.slice(1))
              .join(' ');

            breadcrumbs.push({
              title: formattedTitle,
              route: currentUrl
            });
          }
        }
      }
    }

    return breadcrumbs;
  }
}
