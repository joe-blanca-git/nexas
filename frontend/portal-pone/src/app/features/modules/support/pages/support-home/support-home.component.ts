import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SupportTicketsComponent } from '../support-tickets/support-tickets.component';
import { SupportForumComponent } from '../support-forum/support-forum.component';
import { SupportAccessComponent } from '../support-access/support-access.component';

@Component({
  selector: 'app-support-home',
  standalone: true,
  imports: [CommonModule, SupportTicketsComponent, SupportForumComponent, SupportAccessComponent],
  templateUrl: './support-home.component.html',
  styleUrl: './support-home.component.scss'
})
export class SupportHomeComponent {
  activeTab: 'tickets' | 'forum' | 'access' = 'tickets';

  switchTab(tab: 'tickets' | 'forum' | 'access'): void {
    this.activeTab = tab;
  }
}
