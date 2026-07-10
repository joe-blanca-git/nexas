import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute, NavigationEnd, Event as RouterEvent } from '@angular/router';
import { filter } from 'rxjs';
import { MenuSideComponent } from '../../../shared/components/menu-side/menu-side.component';
import { BreadcrumbComponent } from '../../../../shared/components/breadcrumb/breadcrumb.component';
import { MenuService } from '../../../../core/services/menu.service';
import { StateUtil } from '../../../../core/utils/UserState.util';
import { AuthService } from '../../../../core/auth/auth.service';

interface Course {
  id: number;
  name: string;
  code: string;
  teacher: string;
  teacherRole: string;
  progress: number;
  totalLessons: number;
  completedLessons: number;
  gradient: string;
  nextClass: string;
  nextClassTime: string;
  color: string;
  category: string;
  grade1: number | null;
  grade2: number | null;
  gradeSub: number | null;
  gradeFinal: number | null;
  attendance: number;
}

interface Activity {
  id: number;
  title: string;
  course: string;
  date: string;
  status: 'pending' | 'completed';
  type: string;
  icon: string;
  badgeClass: string;
  urgent: boolean;
  points: string;
}

interface NotificationItem {
  id: number;
  title: string;
  description: string;
  time: string;
  unread: boolean;
  icon: string;
}

interface FinancialItem {
  id: number;
  month: string;
  dueDate: string;
  value: number;
  status: 'Pago' | 'A vencer' | 'Atrasado';
  paymentDate: string | null;
  barcode: string;
  pixKey: string;
}

interface ProtocolItem {
  id: string;
  type: string;
  requestDate: string;
  status: string;
  statusClass: string;
  estimatedDate: string;
}

interface FaqItem {
  question: string;
  answer: string;
  open: boolean;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MenuSideComponent, BreadcrumbComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  private readonly stateUtil = inject(StateUtil);

  sidebarCollapsed = false;
  activeTab = 'dashboard';
  searchTerm = '';

  // Student Info
  studentName = '--';
  studentRA = '--';
  studentCourse = '--';
  studentPeriod = '--';
  studentGPA = 0;
  studentAttendance = 0;

  //general
  isLoadingPage = false;

  // Dropdowns
  showNotificationsDropdown = false;
  showProfileDropdown = false;

  // Copy Toast Feedbacks
  toastMessage = '';
  showToast = false;

  // Support Form Mock
  supportSubject = '';
  supportCategory = 'Dúvida Acadêmica';
  supportMessage = '';

  // New Protocol Form Mock
  showProtocolModal = false;
  newProtocolType = 'Declaração de Matrícula';
  newProtocolUrgency = 'Normal';
  newProtocolObs = '';

  // Data Sources
  courses: Course[] = [
    {
      id: 1,
      name: 'Desenvolvimento Web com Angular',
      code: 'PAN-301',
      teacher: 'Dr. André Silva',
      teacherRole: 'Ph.D. em Engenharia de Software',
      progress: 68,
      totalLessons: 40,
      completedLessons: 27,
      gradient: 'linear-gradient(135deg, #6366f1 0%, #4f46e5 100%)',
      nextClass: 'Terça-feira',
      nextClassTime: '19:00 - 22:30',
      color: '#6366f1',
      category: 'Tecnologia',
      grade1: 8.5,
      grade2: 9.0,
      gradeSub: null,
      gradeFinal: 8.75,
      attendance: 95
    },
    {
      id: 2,
      name: 'Banco de Dados NoSQL',
      code: 'PAN-302',
      teacher: 'Dra. Amanda Lima',
      teacherRole: 'Especialista em Big Data',
      progress: 45,
      totalLessons: 36,
      completedLessons: 16,
      gradient: 'linear-gradient(135deg, #06b6d4 0%, #0891b2 100%)',
      nextClass: 'Quarta-feira',
      nextClassTime: '19:00 - 22:30',
      color: '#06b6d4',
      category: 'Dados',
      grade1: 7.8,
      grade2: null,
      gradeSub: null,
      gradeFinal: null,
      attendance: 88
    },
    {
      id: 3,
      name: 'Estrutura de Dados em C#',
      code: 'PAN-303',
      teacher: 'Prof. Carlos Souza',
      teacherRole: 'Arquiteto .NET Senior',
      progress: 85,
      totalLessons: 44,
      completedLessons: 37,
      gradient: 'linear-gradient(135deg, #10b981 0%, #059669 100%)',
      nextClass: 'Quinta-feira',
      nextClassTime: '19:00 - 22:30',
      color: '#10b981',
      category: 'Algoritmos',
      grade1: 9.5,
      grade2: 8.8,
      gradeSub: null,
      gradeFinal: 9.15,
      attendance: 96
    },
    {
      id: 4,
      name: 'Arquitetura de Softwares Cloud',
      code: 'PAN-304',
      teacher: 'Prof. Roberta Mendes',
      teacherRole: 'DevOps Engineer AWS',
      progress: 20,
      totalLessons: 32,
      completedLessons: 6,
      gradient: 'linear-gradient(135deg, #f59e0b 0%, #d97706 100%)',
      nextClass: 'Sexta-feira',
      nextClassTime: '19:00 - 22:30',
      color: '#f59e0b',
      category: 'Infraestrutura',
      grade1: null,
      grade2: null,
      gradeSub: null,
      gradeFinal: null,
      attendance: 90
    }
  ];

  activities: Activity[] = [
    {
      id: 101,
      title: 'Entrega de Trabalho - Diretivas e Pipes',
      course: 'Desenvolvimento Web com Angular',
      date: 'Amanhã, às 23:59',
      status: 'pending',
      type: 'Trabalho',
      icon: 'fa-file-code',
      badgeClass: 'bg-warning text-dark',
      urgent: true,
      points: '1.5 pts'
    },
    {
      id: 102,
      title: 'Avaliação Prática A1 - Modelagem NoSQL',
      course: 'Banco de Dados NoSQL',
      date: '25/05/2026, às 19:30',
      status: 'pending',
      type: 'Prova',
      icon: 'fa-file-signature',
      badgeClass: 'bg-danger text-white',
      urgent: true,
      points: '6.0 pts'
    },
    {
      id: 103,
      title: 'Projeto Integrador - Sistema Bancário C#',
      course: 'Estrutura de Dados em C#',
      date: '02/06/2026, às 23:59',
      status: 'pending',
      type: 'Projeto',
      icon: 'fa-project-diagram',
      badgeClass: 'bg-info text-dark',
      urgent: false,
      points: '4.0 pts'
    },
    {
      id: 104,
      title: 'Questionário Semanal - Introdução ao Docker',
      course: 'Arquitetura de Softwares Cloud',
      date: 'Concluído',
      status: 'completed',
      type: 'Atividade',
      icon: 'fa-check-double',
      badgeClass: 'bg-success text-white',
      urgent: false,
      points: '1.0 pt'
    }
  ];

  notifications: NotificationItem[] = [
    {
      id: 201,
      title: 'Manutenção Programada',
      description: 'O ambiente virtual passará por manutenção corretiva em 20/05/2026, das 22h às 23h.',
      time: 'há 2 horas',
      unread: true,
      icon: 'fa-cog text-warning'
    },
    {
      id: 202,
      title: 'Nota Publicada',
      description: "Sua nota da A1 de 'Estrutura de Dados em C#' foi lançada: 9.5!",
      time: 'há 5 horas',
      unread: true,
      icon: 'fa-graduation-cap text-success'
    },
    {
      id: 203,
      title: 'Nova Mensagem do Professor',
      description: 'Dr. André Silva respondeu a sua dúvida no fórum de discussão de Angular.',
      time: 'ontem',
      unread: false,
      icon: 'fa-envelope text-primary'
    }
  ];

  financials: FinancialItem[] = [
    {
      id: 301,
      month: 'Maio/2026',
      dueDate: '10/05/2026',
      value: 850.00,
      status: 'Pago',
      paymentDate: '08/05/2026',
      barcode: '34191.79001 01043.513184 91020.150008 7 98760000085000',
      pixKey: '03848293000109'
    },
    {
      id: 302,
      month: 'Junho/2026',
      dueDate: '10/06/2026',
      value: 850.00,
      status: 'A vencer',
      paymentDate: null,
      barcode: '34191.79001 01043.513184 91020.150008 7 98760000085000',
      pixKey: '03848293000109'
    },
    {
      id: 303,
      month: 'Julho/2026',
      dueDate: '10/07/2026',
      value: 850.00,
      status: 'A vencer',
      paymentDate: null,
      barcode: '34191.79001 01043.513184 91020.150008 7 98760000085000',
      pixKey: '03848293000109'
    },
    {
      id: 304,
      month: 'Abril/2026',
      dueDate: '10/04/2026',
      value: 850.00,
      status: 'Pago',
      paymentDate: '10/04/2026',
      barcode: '34191.79001 01043.513184 91020.150008 7 98760000085000',
      pixKey: '03848293000109'
    }
  ];

  protocols: ProtocolItem[] = [
    {
      id: 'REQ-2026-0089',
      type: 'Declaração de Matrícula',
      requestDate: '15/05/2026',
      status: 'Concluído',
      statusClass: 'bg-success text-white',
      estimatedDate: '17/05/2026'
    },
    {
      id: 'REQ-2026-0104',
      type: 'Histórico Acadêmico Parcial',
      requestDate: '18/05/2026',
      status: 'Em Análise',
      statusClass: 'bg-warning text-dark',
      estimatedDate: '22/05/2026'
    },
    {
      id: 'REQ-2026-0112',
      type: 'Dispensa de Disciplina (EAD)',
      requestDate: '19/05/2026',
      status: 'Aguardando Documentação',
      statusClass: 'bg-danger text-white',
      estimatedDate: '28/05/2026'
    }
  ];

  faqs: FaqItem[] = [
    {
      question: 'Como faço para solicitar trancamento ou dispensa de disciplinas?',
      answer: 'Você pode solicitar trancamento de matrícula ou dispensa de disciplinas diretamente na aba "Secretaria". Clique em "Novo Requerimento", selecione o tipo adequado e preencha as informações adicionais necessárias.',
      open: false
    },
    {
      question: 'Onde posso obter minha via do Contrato de Prestação de Serviços?',
      answer: 'O contrato assinado digitalmente fica disponível a qualquer momento para download na aba "Financeiro", logo abaixo das faturas mensais, ou na central de contratos.',
      open: false
    },
    {
      question: 'Quais são as datas limites para entrega de trabalhos acadêmicos?',
      answer: 'As datas de entrega são definidas individualmente por cada professor para cada disciplina. Você pode acompanhar todas de forma unificada na aba "Calendário" ou na barra lateral de atividades pendentes no seu painel principal.',
      open: false
    },
    {
      question: 'Esqueci minha senha de acesso, o que devo fazer?',
      answer: 'Na página de login do portal do aluno, clique em "Esqueceu a senha?". Insira seu e-mail cadastrado e você receberá um link seguro para cadastrar uma nova senha.',
      open: false
    }
  ];

  pageTitle: string = 'Página Inicial';

  constructor(private router: Router, private activatedRoute: ActivatedRoute, private authService: AuthService) {
    this.router.events.pipe(
      filter((event: RouterEvent) => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.pageTitle = this.getChildTitle(this.activatedRoute.root);
    });
  }

  private getChildTitle(route: ActivatedRoute): string {
    if (route.firstChild) {
      return this.getChildTitle(route.firstChild);
    }
    return route.routeConfig?.title as string || (route.routeConfig?.data ? route.routeConfig.data['title'] : 'Navegação');
  }

  ngOnInit() {
    if (typeof window !== 'undefined') {
      // Carrega o estado salvo do cache (localStorage)
      const savedSidebarState = localStorage.getItem('sidebarCollapsed');
      if (savedSidebarState !== null) {
        this.sidebarCollapsed = JSON.parse(savedSidebarState);
      }

      // Em telas menores, garantimos que comece fechada por padrão
      if (window.innerWidth < 992) {
        this.sidebarCollapsed = true;
      }

      const currentPath = window.location.pathname;
      const cleanTab = currentPath.substring(currentPath.lastIndexOf('/') + 1);
      if (cleanTab && cleanTab !== 'home') {
        this.activeTab = cleanTab;
      }
    }

    this.loadDataPage();
    
  }

  logOut(){
    this.authService.logOut();
  }

  async loadDataPage(){
    this.isLoadingPage = true;

    try{
       this.stateUtil.getUser().subscribe(user => {
        if (user) {
          this.studentName = user.name || '--';
        };
      });
      
    }catch(error){

    }finally{
      this.isLoadingPage = false;
    }
  }

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
    if (typeof window !== 'undefined') {
      localStorage.setItem('sidebarCollapsed', JSON.stringify(this.sidebarCollapsed));
    }
  }

  setActiveTab(tab: string) {
    const cleanTab = tab.startsWith('/') ? tab.substring(1) : tab;
    this.activeTab = cleanTab === 'home' ? 'dashboard' : cleanTab;
    this.closeDropdowns();

    // Auto collapse sidebar on small screen after selecting an option
    if (typeof window !== 'undefined') {
      if (window.innerWidth < 768) {
        this.sidebarCollapsed = true;
      }
    }
  }

  getInitials(name: string): string {
    if (!name) return 'A';
    const parts = name.split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return name[0].toUpperCase();
  }

  get unreadNotificationsCount(): number {
    return this.notifications.filter(n => n.unread).length;
  }

  markAllAsRead() {
    this.notifications.forEach(n => n.unread = false);
    this.triggerToast('Todas as notificações marcadas como lidas.');
  }

  toggleNotifications(event: MouseEvent) {
    event.stopPropagation();
    this.showNotificationsDropdown = !this.showNotificationsDropdown;
    this.showProfileDropdown = false;
  }

  toggleProfile(event: MouseEvent) {
    event.stopPropagation();
    this.showProfileDropdown = !this.showProfileDropdown;
    this.showNotificationsDropdown = false;
  }

  closeDropdowns() {
    this.showNotificationsDropdown = false;
    this.showProfileDropdown = false;
  }

  triggerToast(message: string) {
    this.toastMessage = message;
    this.showToast = true;
    setTimeout(() => {
      this.showToast = false;
    }, 3000);
  }

  copyText(text: string, type: string) {
    if (typeof navigator !== 'undefined' && navigator.clipboard) {
      navigator.clipboard.writeText(text).then(() => {
        this.triggerToast(`${type} copiado com sucesso!`);
      });
    } else {
      // Fallback
      this.triggerToast(`${type} copiado!`);
    }
  }

  toggleFaq(index: number) {
    this.faqs[index].open = !this.faqs[index].open;
  }

  submitSupport(event: Event) {
    event.preventDefault();
    if (!this.supportSubject || !this.supportMessage) {
      this.triggerToast('Por favor, preencha o assunto e a mensagem.');
      return;
    }
    this.triggerToast('Chamado de suporte enviado com sucesso! Retornaremos em breve.');
    this.supportSubject = '';
    this.supportMessage = '';
  }

  openProtocolModal() {
    this.showProtocolModal = true;
  }

  closeProtocolModal() {
    this.showProtocolModal = false;
    this.newProtocolObs = '';
  }

  submitProtocol(event: Event) {
    event.preventDefault();
    const newId = `REQ-2026-0${113 + this.protocols.length}`;
    const today = new Date();
    const dd = String(today.getDate()).padStart(2, '0');
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const yyyy = today.getFullYear();
    const formattedToday = `${dd}/${mm}/${yyyy}`;

    const estDate = new Date();
    estDate.setDate(today.getDate() + 5);
    const ddE = String(estDate.getDate()).padStart(2, '0');
    const mmE = String(estDate.getMonth() + 1).padStart(2, '0');
    const yyyyE = estDate.getFullYear();
    const formattedEst = `${ddE}/${mmE}/${yyyyE}`;

    this.protocols.unshift({
      id: newId,
      type: this.newProtocolType,
      requestDate: formattedToday,
      status: 'Em Análise',
      statusClass: 'bg-warning text-dark',
      estimatedDate: formattedEst
    });

    this.closeProtocolModal();
    this.triggerToast('Requerimento protocolado com sucesso!');
  }

  get filteredCourses() {
    if (!this.searchTerm) {
      return this.courses;
    }
    const term = this.searchTerm.toLowerCase();
    return this.courses.filter(c =>
      c.name.toLowerCase().includes(term) ||
      c.teacher.toLowerCase().includes(term) ||
      c.code.toLowerCase().includes(term)
    );
  }
}

