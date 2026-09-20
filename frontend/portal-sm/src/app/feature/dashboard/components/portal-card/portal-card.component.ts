import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-portal-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './portal-card.component.html',
  styleUrl: './portal-card.component.scss'
})
export class PortalCardComponent {
  @Input() title: string = '';
  @Input() category: string = '';
  @Input() description: string = '';
  @Input() routeLink: string = '#';
  @Input() variant: 'applications' | 'organization' = 'applications';
}
