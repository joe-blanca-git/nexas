import { Injectable, inject } from '@angular/core';
import { IMenuItem } from '../../features/shared/components/menu-side/menu-side.component';
import { Router } from '@angular/router';
import { AuthUtil } from '../auth/auth.util';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  private authUtil = inject(AuthUtil);

  constructor(private router: Router) { }

  getMenu(): IMenuItem[] {
    const isAdmin = this.authUtil.hasRole('Admin');

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
        title: 'Blog',
        icon: 'fas fa-newspaper',
        route: '/blog'
      },
      {
        id: 4,
        title: 'Configurações',
        icon: 'fas fa-cog',
        route: '/settings'
      },
      {
        id: 5,
        title: 'Suporte',
        icon: 'fas fa-headset',
        route: '/support'
      },
      {
        id: 6,
        title: 'Financeiro',
        icon: 'fas fa-dollar-sign',
        route: '/financial'
      }
    ];

    if (!isAdmin) {
      return menuMock.filter(m => m.route !== '/settings' && m.route !== '/financial');
    }

    return menuMock;
  }

}
