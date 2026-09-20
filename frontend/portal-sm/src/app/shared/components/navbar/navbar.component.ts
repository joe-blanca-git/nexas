import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ThemeService, Theme } from '../../../core/services/theme.service';
import { AuthService } from '../../../core/auth/auth.service';
import { StateUtil } from '../../../core/utils/UserState.util';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
  isDropdownOpen = false;
  private router = inject(Router);
  public themeService = inject(ThemeService);
  private authService = inject(AuthService);
  public stateUtil = inject(StateUtil);

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }

  goToAccount() {
    this.isDropdownOpen = false;
    this.router.navigate(['/account']);
  }

  onLogout() {
    this.isDropdownOpen = false;
    this.authService.logOut();
  }
}
