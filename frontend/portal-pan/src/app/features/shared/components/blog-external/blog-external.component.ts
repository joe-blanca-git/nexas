import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ILatestNews } from '../../../modules/home/models/home.model';
import { RouterModule } from '@angular/router';

export interface IPost {
  id?: number;
  title: string;
  excerpt: string;
  image: string;
  category: string;
  date: string;
}

@Component({
  selector: 'app-blog-external',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './blog-external.component.html',
  styleUrl: './blog-external.component.scss'
})
export class BlogExternalComponent implements OnChanges {
  @Input() isLoadingPage: boolean = false;
  @Input() news: ILatestNews[] = [];

  featuredPost!: IPost;
  mediumPosts: IPost[] = [];
  smallPosts: IPost[] = [];

  ngOnChanges(changes: SimpleChanges) {
    if ((changes['news'] || changes['isLoadingPage']) && !this.isLoadingPage) {
      this.buildPosts();
    }
  }

  buildPosts() {
    // If no news, just render empty or placeholders
    if (!this.news || this.news.length === 0) {
      this.featuredPost = {
        id: 0,
        title: 'Nenhuma publicação',
        excerpt: 'Aguarde novas publicações.',
        image: 'https://s2.glbimg.com/Deg8YEkSphxP1LqSUr0QBH_O82c=/780x440/e.glbimg.com/og/ed/f/original/2022/04/20/r4f167447_rrd_1x.jpg',
        category: 'Blog',
        date: ''
      };
      this.mediumPosts = [];
      this.smallPosts = [];
      return;
    }

    const mapToIPost = (n: ILatestNews): IPost => ({
      id: n.id,
      title: n.title,
      excerpt: n.subject || 'Sem resumo',
      image: n.headerImageUrl || 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
      category: n.tags ? n.tags.split(',')[0].trim() : 'Geral',
      date: new Date(n.publishDate).toLocaleDateString()
    });

    this.featuredPost = mapToIPost(this.news[0]);
    
    this.mediumPosts = this.news.slice(1, 3).map(mapToIPost);
    
    this.smallPosts = this.news.slice(3, 7).map(mapToIPost);
  }
}
