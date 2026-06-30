import { Component, Output, EventEmitter, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ILatestCourse } from '../../models/home.model';

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
    if ((changes['latestCourse'] || changes['isLoadingPage']) && !this.isLoadingPage) {
      this.buildInfo();
    }
  }

  buildInfo() {
    this.info = {
      myCourses: 0,
      progressMyCourses: 0,
      myForums: 0,
      myCertificates: 0,
      lastCourse: this.latestCourse ? {
        title: this.latestCourse.title,
        description: this.latestCourse.description || '',
        image: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQY5JRi1qnVrXkmfdZGtvwU__qfPnrsVZqIVg&s',
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
        image: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQY5JRi1qnVrXkmfdZGtvwU__qfPnrsVZqIVg&s',
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
