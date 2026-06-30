import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HomeService } from '../../services/home.service';

@Component({
  selector: 'app-news-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './news-detail.component.html',
  styleUrl: './news-detail.component.scss'
})
export class NewsDetailComponent implements OnInit {
  newsId: number | null = null;
  newsDetail: any = null;
  isLoading = true;
  error = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private homeService: HomeService
  ) {}

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.newsId = +idParam;
        this.loadNewsDetail();
      } else {
        this.router.navigate(['/home']);
      }
    });
  }

  async loadNewsDetail() {
    if (!this.newsId) return;
    
    this.isLoading = true;
    this.error = false;
    
    try {
      this.newsDetail = await this.homeService.getNewsDetail(this.newsId);
    } catch (err) {
      this.error = true;
      console.error('Failed to load news detail', err);
    } finally {
      this.isLoading = false;
    }
  }

  goBack() {
    this.router.navigate(['/home']);
  }
}
