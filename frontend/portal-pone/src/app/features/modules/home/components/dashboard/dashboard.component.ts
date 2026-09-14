import { Component, Output, EventEmitter, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { Chart, ChartConfiguration, ChartType, registerables } from 'chart.js';
Chart.register(...registerables);

export interface DashboardKpi {
  title: string;
  value: string;
  icon: string;
  badge?: {
    text: string;
    type: 'success' | 'danger' | 'warning' | 'info';
  };
  auxText?: string;
}

export interface SummaryItem {
  label: string;
  value: string;
  icon: string;
}

export interface ForumTopic {
  course: string;
  lesson: string;
  title: string;
  author: string;
  date: string;
  status: 'Novo' | 'Respondido' | 'Fechado';
}

import { HomeService } from '../../services/home.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private homeService = inject(HomeService);

  @Output() tabChange = new EventEmitter<string>();
  @Input() isLoadingPage: boolean = false;

  kpis: DashboardKpi[] = [];
  summaryItems: SummaryItem[] = [];
  forumTopics: ForumTopic[] = [];

  public lineChartData: ChartConfiguration['data'] = {
    labels: ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
    datasets: []
  };

  public lineChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    elements: {
      point: {
        radius: 0,
        hitRadius: 10,
        hoverRadius: 5,
      }
    },
    scales: {
      x: {
        grid: { display: false },
        border: { display: false },
        ticks: { color: '#9ca3af' }
      },
      y: {
        grid: { color: 'rgba(255, 255, 255, 0.05)' },
        border: { display: false },
        ticks: { color: '#9ca3af' }
      }
    },
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: '#1f2937',
        titleColor: '#f9fafb',
        bodyColor: '#f9fafb',
        borderColor: 'rgba(255, 255, 255, 0.1)',
        borderWidth: 1,
        padding: 10,
        displayColors: false,
        callbacks: {
          label: function(context) {
            let label = context.dataset.label || '';
            if (label) { label += ': '; }
            if (context.parsed.y !== null) {
              label += new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(context.parsed.y);
            }
            return label;
          }
        }
      }
    }
  };

  public lineChartType: ChartType = 'line';

  ngOnInit() {
    this.isLoadingPage = true;
    this.homeService.getTeacherDashboard().subscribe({
      next: (data) => {
        this.updateDashboard(data);
        this.isLoadingPage = false;
      },
      error: (err) => {
        console.error('Erro ao buscar dashboard do professor', err);
        this.isLoadingPage = false;
      }
    });
  }

  updateDashboard(data: any) {
    const formatCurrency = (val: number) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);

    this.kpis = [
      { 
        title: 'Cursos Vendidos', 
        value: data.totalCoursesSold.toString(), 
        icon: 'fas fa-book', 
        badge: { 
          text: data.salesGrowthPercentage >= 0 ? `+${data.salesGrowthPercentage}% este mês` : `${data.salesGrowthPercentage}% este mês`, 
          type: data.salesGrowthPercentage >= 0 ? 'success' : 'danger' 
        } 
      },
      { 
        title: 'Faturamento Total', 
        value: formatCurrency(data.totalRevenue), 
        icon: 'fas fa-dollar-sign' 
      },
      { 
        title: 'Crescimento', 
        value: data.revenueGrowthPercentage >= 0 ? `+${data.revenueGrowthPercentage}%` : `${data.revenueGrowthPercentage}%`, 
        icon: 'fas fa-chart-line', 
        auxText: 'Crescimento em relação ao mês anterior.' 
      },
      { 
        title: 'Alunos Ativos', 
        value: data.totalActiveStudents.toString(), 
        icon: 'fas fa-users' 
      }
    ];

    this.summaryItems = [
      { label: 'Receita hoje', value: formatCurrency(data.todayRevenue), icon: 'fas fa-money-bill-wave' },
      { label: 'Novos alunos', value: data.newStudentsThisMonth.toString(), icon: 'fas fa-user-plus' },
      { label: 'Cursos publicados', value: data.totalActivePublishedCourses.toString(), icon: 'fas fa-video' },
      { label: 'Aulas publicadas', value: data.totalPublishedClassesOfActiveCourses.toString(), icon: 'fas fa-play-circle' },
      { label: 'Fóruns abertos', value: data.totalOpenForumsWithoutReply.toString(), icon: 'fas fa-envelope' },
      { label: 'Avaliação média', value: `⭐ ${data.averageCourseRating}`, icon: 'fas fa-star' }
    ];

    this.forumTopics = data.last5Forums.map((f: any) => ({
      course: f.courseName,
      lesson: f.lessonName,
      title: f.title,
      author: f.authorName,
      date: new Date(f.date).toLocaleDateString('pt-BR'),
      status: f.status === 'Open' ? 'Novo' : (f.status === 'Resolved' ? 'Respondido' : 'Fechado')
    }));

    this.lineChartData = {
      labels: ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
      datasets: [
        {
          data: data.monthlyRevenueCurrentYear,
          label: 'Faturamento (R$)',
          backgroundColor: 'rgba(99, 102, 241, 0.1)',
          borderColor: '#6366f1',
          pointBackgroundColor: '#6366f1',
          pointBorderColor: '#fff',
          pointHoverBackgroundColor: '#fff',
          pointHoverBorderColor: 'rgba(99, 102, 241, 0.8)',
          fill: 'origin',
          tension: 0.4,
          borderWidth: 2
        }
      ]
    };
  }
}
