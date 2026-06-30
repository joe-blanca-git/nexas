import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { DashboardComponent } from '../dashboard/dashboard.component';
import { BlogExternalComponent } from '../../../../shared/components/blog-external/blog-external.component';
import { StateUtil } from '../../../../../core/utils/UserState.util';
import { HomeService } from '../../services/home.service';
import { IPortalHomeData } from '../../models/home.model';

@Component({
  selector: 'app-home-dashboard',
  standalone: true,
  imports: [CommonModule, DashboardComponent, BlogExternalComponent],
  templateUrl: './home-dashboard.component.html',
  styleUrl: './home-dashboard.component.scss'
})
export class HomeDashboardComponent implements OnInit {
  private readonly stateUtil = inject(StateUtil);

  private readonly homeService = inject(HomeService);

  studentName = '';
  isLoadingPage = true;
  homeData: IPortalHomeData | null = null;

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

      this.homeData = await this.homeService.getHomeData();

    } catch (error) {
      console.error('Erro ao carregar os dados da Home:', error);
    } finally {
      this.isLoadingPage = false;
    }
  }

  setActiveTab(tab: string) {
    this.router.navigate([tab]);
  }
}
