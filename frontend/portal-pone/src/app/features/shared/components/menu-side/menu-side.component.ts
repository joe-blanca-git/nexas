import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MenuService } from '../../../../core/services/menu.service';
import { Router, RouterModule } from '@angular/router';

export interface IMenuItem {
  id: number;
  title: string;
  icon: string;
  route: string;
  subitems?: IMenuItem[];
}

@Component({
  selector: 'app-menu-side',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './menu-side.component.html',
  styleUrl: './menu-side.component.scss'
})
export class MenuSideComponent {
  @Input() sidebarCollapsed = false;
  @Input() activeTab = 'dashboard';
  @Output() tabChange = new EventEmitter<string>();

  menu: IMenuItem[] = [];

  constructor(
    private menuService: MenuService, 
    private router: Router
  ) {
    this.fetchMenu();
  }

  setActiveTab(tab: string) {
    this.router.navigate([tab]);
    this.activeTab = tab;
    this.tabChange.emit(tab);
  }

  fetchMenu() {
    this.menu = this.menuService.getMenu();
  }
}
