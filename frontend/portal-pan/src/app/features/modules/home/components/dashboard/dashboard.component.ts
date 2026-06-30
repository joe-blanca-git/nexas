import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

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
export class DashboardComponent {
  @Output() tabChange = new EventEmitter<string>();

  isLoadingPage = false;

  info!: IInfo

  ngOnInit() {
    this.loadData();
  }

  async loadData() {
    this.isLoadingPage = true;
    try {
      this.info = {
        myCourses: 0,
        progressMyCourses: 0,
        myForums: 0,
        myCertificates: 0,
        lastCourse:{
          title: 'Mestres do Operations Center',
          description: 'Aprenda a monitorar infraestruturas complexas de nuvem e gerenciar incidentes em tempo real utilizando as melhores ferramentas do mercado de DevOps.',
          image: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQY5JRi1qnVrXkmfdZGtvwU__qfPnrsVZqIVg&s',
          rating: 4.9,
          reviews: 120,
          btnText: 'Saiba Mais',
          btnAction: 'courses',
          btnIcon: 'fas fa-arrow-right',
          badgeText: 'NOVO LANÇAMENTO',
          badgeColor: 'bg-success',
          percentage: 100
        }
      }
      
    } catch (error) {
      
    } finally {
        this.isLoadingPage = false;
    }
  }

  setActiveTab(tab: string) {
    this.tabChange.emit(tab);
  }
}
