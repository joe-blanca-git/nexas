import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { HomeService } from '../../services/home.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-news-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './news-detail.component.html',
  styleUrl: './news-detail.component.scss'
})
export class NewsDetailComponent implements OnInit {
  newsId: number | null = null;
  newsDetail: any = null;
  recentPosts: any[] = [];
  searchQuery: string = '';
  
  isLoading = true;
  error = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private homeService: HomeService,
    private cdr: ChangeDetectorRef
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
      
      const homeData = await this.homeService.getHomeData();
      if (homeData && homeData.latestNews) {
        this.recentPosts = homeData.latestNews.filter(n => n.id !== this.newsId).slice(0, 4);
      }
    } catch (err) {
      this.error = true;
      console.error('Failed to load news detail', err);
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }

  onSearch(event: Event) {
    event.preventDefault();
    if (this.searchQuery.trim()) {
      console.log('Buscando por:', this.searchQuery);
    }
  }

  share(network: string) {
    const url = encodeURIComponent(window.location.href);
    const title = encodeURIComponent(this.newsDetail?.title || 'Notícia Portal Pan');
    
    let shareUrl = '';
    switch(network) {
      case 'linkedin':
        shareUrl = `https://www.linkedin.com/shareArticle?mini=true&url=${url}&title=${title}`;
        break;
      case 'twitter':
      case 'x':
        shareUrl = `https://twitter.com/intent/tweet?url=${url}&text=${title}`;
        break;
      case 'whatsapp':
        shareUrl = `https://api.whatsapp.com/send?text=${title} - ${url}`;
        break;
      case 'instagram':
        // O Instagram não possui API de compartilhamento web direto via link.
        // O padrão do mercado é copiar o link para a área de transferência.
        navigator.clipboard.writeText(window.location.href).then(() => {
          alert('Link copiado! Agora é só colar no Instagram ou onde preferir.');
        });
        return;
    }
    
    if (shareUrl) {
      window.open(shareUrl, '_blank', 'width=600,height=500,scrollbars=yes');
    }
  }

  goBack() {
    this.router.navigate(['/home']);
  }
}
