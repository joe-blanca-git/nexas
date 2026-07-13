import { Component, Output, EventEmitter, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ILatestCourse, IMyCoursesSummary } from '../../models/home.model';

export interface IInfo {
  myCourses: number;
  progressMyCourses: number;
  myForums: number;
  myCertificates: number;
  lastCourse: ILastCourse;
}

export interface ILastCourse{
  title: string;
  description: string;
  image: string;
  rating: number;
  reviews: number;
  btnText: string;
  btnAction: string;
  btnIcon: string;
  badgeText: string;
  badgeColor: string;
  percentage: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnChanges {
  @Output() tabChange = new EventEmitter<string>();
  
  @Input() isLoadingPage: boolean = false;
  @Input() latestCourse: ILatestCourse | null = null;
  @Input() coursesSummary: IMyCoursesSummary | null = null;

  info: IInfo = {
    myCourses: 0,
    progressMyCourses: 0,
    myForums: 0,
    myCertificates: 0,
    lastCourse: {
      title: '',
      description: '',
      image: '',
      rating: 0,
      reviews: 0,
      btnText: '',
      btnAction: '',
      btnIcon: '',
      badgeText: '',
      badgeColor: '',
      percentage: 0
    }
  };

  ngOnChanges(changes: SimpleChanges) {
    if ((changes['latestCourse'] || changes['coursesSummary'] || changes['isLoadingPage']) && !this.isLoadingPage) {
      this.buildInfo();
    }
  }

  buildInfo() {
    this.info = {
      myCourses: this.coursesSummary?.totalCourses || 0,
      progressMyCourses: this.coursesSummary?.overallProgress || 0,
      myForums: 0,
      myCertificates: 0,
      lastCourse: this.latestCourse ? {
        title: this.latestCourse.title,
        description: this.latestCourse.description || '',
        image: this.latestCourse.headerImageUrl || 'https://i.pinimg.com/1200x/29/dd/db/29dddbb74db0c68adc5358271281e03a.jpg',
        rating: this.latestCourse.rating,
        reviews: this.latestCourse.voteCount,
        btnText: 'Saiba Mais',
        btnAction: 'courses',
        btnIcon: 'fas fa-arrow-right',
        badgeText: 'NOVO LANÇAMENTO',
        badgeColor: 'bg-success',
        percentage: 0
      } : {
        title: 'Nenhum curso disponível',
        description: 'Aguarde novos lançamentos!',
        image: 'https://i.pinimg.com/736x/a1/f1/43/a1f143a671136781b11120884da9e77d.jpg',
        rating: 0,
        reviews: 0,
        btnText: 'Saiba Mais',
        btnAction: 'courses',
        btnIcon: 'fas fa-arrow-right',
        badgeText: '-',
        badgeColor: 'bg-secondary',
        percentage: 0
      }
    };
  }

  setActiveTab(tab: string) {
    this.tabChange.emit(tab);
  }
}
