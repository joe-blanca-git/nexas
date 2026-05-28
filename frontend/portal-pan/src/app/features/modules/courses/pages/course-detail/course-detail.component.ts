import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

export interface ILesson {
  id: number;
  title: string;
  duration: string;
  completed: boolean;
  type: 'video' | 'pdf' | 'quiz';
}

@Component({
  selector: 'app-course-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './course-detail.component.html',
  styleUrl: './course-detail.component.scss'
})
export class CourseDetailComponent implements OnInit {
  courseId: number = 0;
  course: any = null;

  coursesList = [
    {
      id: 1,
      name: 'Desenvolvimento Web com Angular',
      code: 'PAN-301',
      teacher: 'Dr. André Silva',
      progress: 68,
      totalLessons: 40,
      completedLessons: 27,
      color: '#6366f1',
      category: 'Tecnologia',
      image: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQY5JRi1qnVrXkmfdZGtvwU__qfPnrsVZqIVg&s',
      description: 'Domine a criação de SPAs de alta performance com a nova arquitetura standalone do Angular, sinalização reativa e estados globais.',
      rating: 4.8,
      syllabus: [
        { id: 1, title: 'Introdução ao Angular Moderno e Conceito Standalone', duration: '12m 40s', completed: true, type: 'video' },
        { id: 2, title: 'Arquitetura de Diretórios e Ciclo de Vida de Componentes', duration: '18m 15s', completed: true, type: 'video' },
        { id: 3, title: 'Data Binding, Diretivas Estruturais e Encapsulamento', duration: '22m 30s', completed: true, type: 'video' },
        { id: 4, title: 'Injeção de Dependências e Serviços Reativos', duration: '15m 10s', completed: true, type: 'video' },
        { id: 5, title: 'Mapeamento de Rotas Aninhadas e Lazy Loading', duration: '28m 45s', completed: false, type: 'video' },
        { id: 6, title: 'Gerenciamento de Estado com Signals e RxJS', duration: '35m 20s', completed: false, type: 'video' }
      ]
    },
    {
      id: 2,
      name: 'Banco de Dados NoSQL',
      code: 'PAN-302',
      teacher: 'Dra. Amanda Lima',
      progress: 45,
      totalLessons: 36,
      completedLessons: 16,
      color: '#06b6d4',
      category: 'Dados',
      image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
      description: 'Projete infraestruturas de dados extremamente escaláveis usando MongoDB, Redis e Cassandra, otimizando queries em tempo real.',
      rating: 4.7,
      syllabus: [
        { id: 1, title: 'Introdução aos Modelos de Dados NoSQL', duration: '10m 15s', completed: true, type: 'video' },
        { id: 2, title: 'Instalação e Queries CRUD em MongoDB', duration: '25m 40s', completed: true, type: 'video' },
        { id: 3, title: 'Modelagem de Documentos e Agregações Avançadas', duration: '32m 10s', completed: true, type: 'video' },
        { id: 4, title: 'Armazenamento em Cache de Alta Performance com Redis', duration: '20m 50s', completed: false, type: 'video' },
        { id: 5, title: 'Clusterização e Replicação de Dados com Cassandra', duration: '40m 30s', completed: false, type: 'video' }
      ]
    },
    {
      id: 3,
      name: 'Estrutura de Dados em C#',
      code: 'PAN-303',
      teacher: 'Prof. Carlos Souza',
      progress: 85,
      totalLessons: 44,
      completedLessons: 37,
      color: '#10b981',
      category: 'Algoritmos',
      image: 'https://s2.glbimg.com/Deg8YEkSphxP1LqSUr0QBH_O82c=/780x440/e.glbimg.com/og/ed/f/original/2022/04/20/r4f167447_rrd_1x.jpg',
      description: 'Aprenda lógica profunda de algoritmos, recursividade, listas encadeadas, árvores binárias e hash tables em .NET.',
      rating: 4.9,
      syllabus: [
        { id: 1, title: 'Conceito de Complexidade de Algoritmos (Big O Notation)', duration: '15m 30s', completed: true, type: 'video' },
        { id: 2, title: 'Listas Encadeadas e Listas Duplamente Encadeadas', duration: '28m 12s', completed: true, type: 'video' },
        { id: 3, title: 'Estruturas de Pilhas (Stack) e Filas (Queue)', duration: '18m 45s', completed: true, type: 'video' },
        { id: 4, title: 'Árvores Binárias de Busca e Algoritmos de Travessia', duration: '35m 10s', completed: true, type: 'video' },
        { id: 5, title: 'Hash Tables e Resolução de Colisões', duration: '26m 40s', completed: false, type: 'video' }
      ]
    },
    {
      id: 4,
      name: 'Arquitetura de Softwares Cloud',
      code: 'PAN-304',
      teacher: 'Prof. Roberta Mendes',
      progress: 20,
      totalLessons: 32,
      completedLessons: 6,
      color: '#f59e0b',
      category: 'Infraestrutura',
      image: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQY5JRi1qnVrXkmfdZGtvwU__qfPnrsVZqIVg&s',
      description: 'Planeje resiliência e alta disponibilidade usando AWS, conteinerização com Docker/Kubernetes e pipelines de CI/CD.',
      rating: 4.6,
      syllabus: [
        { id: 1, title: 'Conceito de Computação em Nuvem e IaaS vs PaaS', duration: '14m 20s', completed: true, type: 'video' },
        { id: 2, title: 'Configurando Servidores Virtuais (EC2) e VPC na AWS', duration: '30m 10s', completed: true, type: 'video' },
        { id: 3, title: 'Criando Containers Docker do Zero', duration: '24m 45s', completed: false, type: 'video' },
        { id: 4, title: 'Orquestração de Containers com Kubernetes (EKS)', duration: '45m 12s', completed: false, type: 'video' },
        { id: 5, title: 'Montando Pipelines de Deploy com GitHub Actions', duration: '28m 30s', completed: false, type: 'video' }
      ]
    },
    {
      id: 5,
      name: 'Introdução à Inteligência Artificial',
      code: 'PAN-305',
      teacher: 'Dr. Fábio Santos',
      progress: 0,
      totalLessons: 28,
      completedLessons: 0,
      color: '#8b5cf6',
      category: 'Tecnologia',
      image: 'https://s2.glbimg.com/Deg8YEkSphxP1LqSUr0QBH_O82c=/780x440/e.glbimg.com/og/ed/f/original/2022/04/20/r4f167447_rrd_1x.jpg',
      description: 'Conceitos práticos de redes neurais, modelagem preditiva, engenharia de prompt e machine learning aplicados ao mercado.',
      rating: 4.9,
      syllabus: [
        { id: 1, title: 'História da IA e Fundamentos de Machine Learning', duration: '18m 10s', completed: false, type: 'video' },
        { id: 2, title: 'Regressão Linear e Classificação Conceitual', duration: '25m 30s', completed: false, type: 'video' },
        { id: 3, title: 'Introdução a Redes Neurais e Deep Learning', duration: '32m 45s', completed: false, type: 'video' },
        { id: 4, title: 'Modelos de Linguagem e Prompt Engineering Eficiente', duration: '22m 15s', completed: false, type: 'video' }
      ]
    },
    {
      id: 6,
      name: 'Interface do Usuário (UI/UX)',
      code: 'PAN-306',
      teacher: 'Dra. Julia Costa',
      progress: 100,
      totalLessons: 24,
      completedLessons: 24,
      color: '#ec4899',
      category: 'Design',
      image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
      description: 'Domine wireframing de alta fidelidade no Figma, testes de usabilidade, paletas cromáticas e design responsivo moderno.',
      rating: 5.0,
      syllabus: [
        { id: 1, title: 'Fundamentos e Diferenças de UI e UX Design', duration: '12m 15s', completed: true, type: 'video' },
        { id: 2, title: 'Criando Wireframes de Baixa Fidelidade no Figma', duration: '22m 40s', completed: true, type: 'video' },
        { id: 3, title: 'Construindo Protótipos Interativos e Micro-interações', duration: '30m 50s', completed: true, type: 'video' },
        { id: 4, title: 'Planejando e Executando Testes de Usabilidade Reais', duration: '26m 12s', completed: true, type: 'video' }
      ]
    }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.courseId = +idParam;
        this.course = this.coursesList.find(c => c.id === this.courseId) || this.coursesList[0];
      } else {
        this.course = this.coursesList[0];
      }
    });
  }

  goBack() {
    this.router.navigate(['/courses']);
  }
}
