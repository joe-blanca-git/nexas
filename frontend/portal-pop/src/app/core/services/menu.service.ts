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
      }
    ];
    return menuMock;
  }

}
