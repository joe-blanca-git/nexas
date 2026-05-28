import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DashboardComponent } from '../dashboard/dashboard.component';
import { BlogExternalComponent } from '../../../../shared/components/blog-external/blog-external.component';
import { StateUtil } from '../../../../../core/utils/UserState.util';

@Component({
  selector: 'app-home-dashboard',
  standalone: true,
  imports: [CommonModule, DashboardComponent, BlogExternalComponent],
  templateUrl: './home-dashboard.component.html',
  styleUrl: './home-dashboard.component.scss'
})
export class HomeDashboardComponent implements OnInit {
  private readonly stateUtil = inject(StateUtil);

  studentName = '';

  isLoadingPage = true;

  constructor(private router: Router) { }

  ngOnInit() {
    this.loadDataPage();
  }

  async loadDataPage() {
    this.isLoadingPage = true;

    try {
      this.stateUtil.getUser().subscribe(user => {
        if (user) {
          this.studentName = user.name || '--';
        };
      });

    } catch (error) {

    } finally {
      this.isLoadingPage = false;
    }
  }

  setActiveTab(tab: string) {
    this.router.navigate([tab]);
  }
}
