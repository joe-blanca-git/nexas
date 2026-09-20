import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../../../../shared/components/navbar/navbar.component';
import { FooterComponent } from '../../../../shared/components/footer/footer.component';
import { ProfileTabComponent } from './components/profile-tab/profile-tab.component';
import { SecurityTabComponent } from './components/security-tab/security-tab.component';
import { PrivacyTabComponent } from './components/privacy-tab/privacy-tab.component';

export type AccountTab = 'perfil' | 'seguranca' | 'privacidade' | 'pagamento';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [
    CommonModule,
    NavbarComponent,
    FooterComponent,
    ProfileTabComponent,
    SecurityTabComponent,
    PrivacyTabComponent
  ],
  templateUrl: './account.component.html',
  styleUrl: './account.component.scss'
})
export class AccountComponent {
  activeTab = signal<AccountTab>('perfil');

  selectTab(tab: AccountTab): void {
    this.activeTab.set(tab);
  }
}
