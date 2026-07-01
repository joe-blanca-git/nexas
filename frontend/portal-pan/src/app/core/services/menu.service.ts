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

}
