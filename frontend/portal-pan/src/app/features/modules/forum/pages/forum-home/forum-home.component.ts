import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// ─── Interfaces ────────────────────────────────────────────────────────────────

export interface IForumCategory {
  id: number;
  name: string;
  description: string;
  icon: string;
  color: string;
  topicCount: number;
  memberCount: number;
  lastActivity: string;
  isRecent: boolean;
}

export type TopicStatus = 'Resolvido' | 'Em andamento' | 'Sem resposta';

export interface IForumTopic {
  id: number;
  categoryId: number;
  categoryName: string;
  authorName: string;
  authorInitials: string;
  authorColor: string;
  title: string;
  preview: string;
  replyCount: number;
  viewCount: number;
  date: string;
  status: TopicStatus;
  isUnread: boolean;
  isOwn: boolean;
  hasPendingReplies: boolean;
}

export interface IForumStats {
  totalTopics: number;
  totalReplies: number;
  unansweredTopics: number;
  favoriteTopics: number;
  unreadReplies: number;
}

// ─── Component ─────────────────────────────────────────────────────────────────

@Component({
  selector: 'app-forum-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './forum-home.component.html',
  styleUrl: './forum-home.component.scss'
})
export class ForumHomeComponent implements OnInit {

  // ─── State ────────────────────────────────────────────────────────────────
  isLoading = true;
  activeTab: 'forums' | 'recent' | 'mine' = 'forums';
  searchTerm = '';
  selectedCategory = 'Todas';
  currentPage = 1;
  itemsPerPage = 5;

  // ─── Modal State ──────────────────────────────────────────────────────────
  showNewTopicModal = false;
  newTopicTitle = '';
  newTopicCategory = '';
  newTopicMessage = '';
  toastMessage = '';
  showToast = false;

  // ─── Summary Stats ────────────────────────────────────────────────────────
  stats: IForumStats = {
    totalTopics: 0,
    totalReplies: 0,
    unansweredTopics: 0,
    favoriteTopics: 0,
    unreadReplies: 0
  };

  // ─── Mock Data ────────────────────────────────────────────────────────────
  categories: IForumCategory[] = [];
  topics: IForumTopic[] = [];
  categoryOptions: string[] = [];

  ngOnInit(): void {
    setTimeout(() => {
      this.categories = [
        {
          id: 1,
          name: 'Desenvolvimento Web com Angular',
          description: 'Dúvidas sobre componentes, rotas, diretivas, signals e arquitetura standalone do Angular.',
          icon: 'fa-laptop-code',
          color: '#6366f1',
          topicCount: 128,
          memberCount: 342,
          lastActivity: 'há 5 minutos',
          isRecent: true
        },
        {
          id: 2,
          name: 'Banco de Dados NoSQL',
          description: 'Discussões sobre MongoDB, Redis, Cassandra, modelagem e otimização de queries.',
          icon: 'fa-database',
          color: '#06b6d4',
          topicCount: 87,
          memberCount: 215,
          lastActivity: 'há 1 hora',
          isRecent: false
        },
        {
          id: 3,
          name: 'Estrutura de Dados em C#',
          description: 'Árvores, grafos, algoritmos de busca, hash tables e performance em .NET.',
          icon: 'fa-code',
          color: '#10b981',
          topicCount: 203,
          memberCount: 398,
          lastActivity: 'há 20 minutos',
          isRecent: true
        },
        {
          id: 4,
          name: 'Arquitetura Cloud',
          description: 'AWS, Docker, Kubernetes, CI/CD, pipelines e infraestrutura como código.',
          icon: 'fa-cloud',
          color: '#f59e0b',
          topicCount: 64,
          memberCount: 189,
          lastActivity: 'há 3 horas',
          isRecent: false
        },
        {
          id: 5,
          name: 'Inteligência Artificial',
          description: 'Redes neurais, machine learning, engenharia de prompt e ferramentas de IA.',
          icon: 'fa-robot',
          color: '#8b5cf6',
          topicCount: 45,
          memberCount: 156,
          lastActivity: 'há 2 dias',
          isRecent: false
        },
        {
          id: 6,
          name: 'Dúvidas Gerais & Suporte',
          description: 'Questões sobre a plataforma, acesso aos cursos, certificados e financeiro.',
          icon: 'fa-question-circle',
          color: '#ec4899',
          topicCount: 312,
          memberCount: 520,
          lastActivity: 'há 8 minutos',
          isRecent: true
        }
      ];

      this.topics = [
        {
          id: 1,
          categoryId: 1,
          categoryName: 'Desenvolvimento Web com Angular',
          authorName: 'Joeder de Blanca',
          authorInitials: 'JB',
          authorColor: '#6366f1',
          title: 'Como usar Signal com Input em componentes Angular 17+?',
          preview: 'Estou tentando combinar input() com signal() para criar um componente reativo, mas o TypeScript acusa erro na desestruturação...',
          replyCount: 7,
          viewCount: 134,
          date: 'há 10 minutos',
          status: 'Em andamento',
          isUnread: true,
          isOwn: true,
          hasPendingReplies: true
        },
        {
          id: 2,
          categoryId: 1,
          categoryName: 'Desenvolvimento Web com Angular',
          authorName: 'Dr. André Silva',
          authorInitials: 'AS',
          authorColor: '#0f172a',
          title: 'Boas práticas para lazy loading com módulos standalone',
          preview: 'Compartilhando aqui algumas dicas que aprendi ao estruturar aplicações grandes com rotas preguiçosas na versão standalone...',
          replyCount: 23,
          viewCount: 389,
          date: 'há 2 horas',
          status: 'Resolvido',
          isUnread: false,
          isOwn: false,
          hasPendingReplies: false
        },
        {
          id: 3,
          categoryId: 3,
          categoryName: 'Estrutura de Dados em C#',
          authorName: 'Marina Costa',
          authorInitials: 'MC',
          authorColor: '#10b981',
          title: 'Dificuldade com recursão em árvore binária — exemplo prático',
          preview: 'Alguém pode me ajudar a entender o caso base e o caso recursivo numa travessia inorder? Meu código retorna null inesperado...',
          replyCount: 0,
          viewCount: 41,
          date: 'há 45 minutos',
          status: 'Sem resposta',
          isUnread: true,
          isOwn: false,
          hasPendingReplies: false
        },
        {
          id: 4,
          categoryId: 2,
          categoryName: 'Banco de Dados NoSQL',
          authorName: 'Felipe Nunes',
          authorInitials: 'FN',
          authorColor: '#06b6d4',
          title: 'Diferença entre aggregation pipeline e mapReduce no MongoDB',
          preview: 'Quero saber em quais casos devo preferir um ao outro em termos de performance e legibilidade de código...',
          replyCount: 12,
          viewCount: 217,
          date: 'há 4 horas',
          status: 'Resolvido',
          isUnread: false,
          isOwn: false,
          hasPendingReplies: false
        },
        {
          id: 5,
          categoryId: 6,
          categoryName: 'Dúvidas Gerais & Suporte',
          authorName: 'Joeder de Blanca',
          authorInitials: 'JB',
          authorColor: '#6366f1',
          title: 'Certificado ainda não foi gerado após conclusão do curso',
          preview: 'Concluí 100% do curso de Angular há 3 dias mas ainda não consigo baixar o certificado. Segue print da tela de conclusão...',
          replyCount: 2,
          viewCount: 28,
          date: 'há 1 dia',
          status: 'Em andamento',
          isUnread: true,
          isOwn: true,
          hasPendingReplies: true
        },
        {
          id: 6,
          categoryId: 4,
          categoryName: 'Arquitetura Cloud',
          authorName: 'Rafael Moura',
          authorInitials: 'RM',
          authorColor: '#f59e0b',
          title: 'Deploy de app Angular no AWS S3 + CloudFront com CI/CD',
          preview: 'Montei um pipeline completo usando GitHub Actions para fazer deploy automático no S3. Compartilhando o YAML e os detalhes...',
          replyCount: 18,
          viewCount: 476,
          date: 'há 2 dias',
          status: 'Resolvido',
          isUnread: false,
          isOwn: false,
          hasPendingReplies: false
        },
        {
          id: 7,
          categoryId: 5,
          categoryName: 'Inteligência Artificial',
          authorName: 'Joeder de Blanca',
          authorInitials: 'JB',
          authorColor: '#6366f1',
          title: 'Como usar a API da OpenAI com temperatura e max_tokens no Angular?',
          preview: 'Preciso integrar o ChatGPT no meu projeto Angular como parte do trabalho final. Alguém tem um exemplo funcional de service...',
          replyCount: 5,
          viewCount: 93,
          date: 'há 3 dias',
          status: 'Em andamento',
          isUnread: false,
          isOwn: true,
          hasPendingReplies: false
        }
      ];

      // Calcula stats
      this.stats = {
        totalTopics: this.topics.length,
        totalReplies: this.topics.reduce((s, t) => s + t.replyCount, 0),
        unansweredTopics: this.topics.filter(t => t.status === 'Sem resposta').length,
        favoriteTopics: 2,
        unreadReplies: this.topics.filter(t => t.isUnread).length
      };

      this.categoryOptions = ['Todas', ...this.categories.map(c => c.name)];
      this.newTopicCategory = this.categories[0]?.name ?? '';
      this.isLoading = false;
    }, 1100);
  }

  // ─── Filtering ────────────────────────────────────────────────────────────

  get filteredTopics(): IForumTopic[] {
    let list = this.topics;

    if (this.activeTab === 'mine') {
      list = list.filter(t => t.isOwn);
    }

    if (this.selectedCategory !== 'Todas') {
      list = list.filter(t => t.categoryName === this.selectedCategory);
    }

    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      list = list.filter(t =>
        t.title.toLowerCase().includes(term) ||
        t.preview.toLowerCase().includes(term) ||
        t.authorName.toLowerCase().includes(term)
      );
    }

    return list;
  }

  get paginatedTopics(): IForumTopic[] {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredTopics.slice(start, start + this.itemsPerPage);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredTopics.length / this.itemsPerPage);
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  get myTopics(): IForumTopic[] {
    return this.topics.filter(t => t.isOwn);
  }

  setPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  setTab(tab: 'forums' | 'recent' | 'mine'): void {
    this.activeTab = tab;
    this.currentPage = 1;
  }

  // ─── New Topic Modal ──────────────────────────────────────────────────────

  openNewTopic(): void {
    this.showNewTopicModal = true;
    this.newTopicTitle = '';
    this.newTopicMessage = '';
  }

  closeNewTopic(): void {
    this.showNewTopicModal = false;
  }

  submitNewTopic(event: Event): void {
    event.preventDefault();
    if (!this.newTopicTitle.trim() || !this.newTopicMessage.trim()) {
      this.triggerToast('Preencha o título e a mensagem do tópico.');
      return;
    }

    const cat = this.categories.find(c => c.name === this.newTopicCategory);
    const newTopic: IForumTopic = {
      id: Date.now(),
      categoryId: cat?.id ?? 1,
      categoryName: this.newTopicCategory,
      authorName: 'Joeder de Blanca',
      authorInitials: 'JB',
      authorColor: '#6366f1',
      title: this.newTopicTitle,
      preview: this.newTopicMessage.substring(0, 120) + '...',
      replyCount: 0,
      viewCount: 1,
      date: 'agora mesmo',
      status: 'Sem resposta',
      isUnread: false,
      isOwn: true,
      hasPendingReplies: false
    };

    this.topics.unshift(newTopic);
    this.stats.totalTopics++;
    this.stats.unansweredTopics++;
    this.closeNewTopic();
    this.triggerToast('Tópico criado com sucesso!');
  }

  // ─── Toast ────────────────────────────────────────────────────────────────

  triggerToast(message: string): void {
    this.toastMessage = message;
    this.showToast = true;
    setTimeout(() => (this.showToast = false), 3000);
  }

  // ─── Status Helpers ───────────────────────────────────────────────────────

  getStatusBadgeClass(status: TopicStatus): string {
    const map: Record<TopicStatus, string> = {
      'Resolvido': 'topic-status-solved',
      'Em andamento': 'topic-status-ongoing',
      'Sem resposta': 'topic-status-unanswered'
    };
    return map[status];
  }

  getStatusIcon(status: TopicStatus): string {
    const map: Record<TopicStatus, string> = {
      'Resolvido': 'fa-check-circle',
      'Em andamento': 'fa-spinner',
      'Sem resposta': 'fa-question-circle'
    };
    return map[status];
  }
}
