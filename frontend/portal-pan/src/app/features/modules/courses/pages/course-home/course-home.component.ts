import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';

export interface ICourse {
  id: number;
  name: string;
  code: string;
  teacher: string;
  progress: number;
  totalLessons: number;
  completedLessons: number;
  color: string;
  category: string;
  image: string;
  description: string;
  rating: number;
}

@Component({
  selector: 'app-course-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './course-home.component.html',
  styleUrl: './course-home.component.scss'
})
export class CourseHomeComponent implements OnInit {
  searchTerm: string = '';
  selectedCategory: string = 'Todos';

  categories: string[] = ['Todos', 'Tecnologia', 'Dados', 'Algoritmos', 'Infraestrutura', 'Design'];

  courses: ICourse[] = [
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
      rating: 4.8
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
      rating: 4.7
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
      rating: 4.9
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
      rating: 4.6
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
      rating: 4.9
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
      rating: 5.0
    }
  ];

  constructor(private router: Router) {}

  ngOnInit(): void {}

  selectCategory(category: string) {
    this.selectedCategory = category;
  }

  getFilteredCourses(): ICourse[] {
    return this.courses.filter(course => {
      const matchesCategory = this.selectedCategory === 'Todos' || course.category === this.selectedCategory;
      const matchesSearch = course.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
                            course.teacher.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
                            course.code.toLowerCase().includes(this.searchTerm.toLowerCase());
      return matchesCategory && matchesSearch;
    });
  }

  navigateDetail(courseId: number) {
    this.router.navigate(['courses/course-detail', courseId]);
  }
}
