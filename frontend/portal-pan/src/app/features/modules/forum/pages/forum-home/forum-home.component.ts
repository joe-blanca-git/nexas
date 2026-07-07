import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ForumService, IForumCategoryDto, IForumTopicSummaryDto, CreateForumTopicCommand } from '../../services/forum.service';

// ─── Interfaces ────────────────────────────────────────────────────────────────

export interface IForumCategoryUI {
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

export interface IForumTopicUI {
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

  private forumService = inject(ForumService);

  // ─── State ────────────────────────────────────────────────────────────────
  isLoading = true;
  activeTab: 'forums' | 'recent' | 'mine' = 'forums';
  searchTerm = '';
  selectedCategory = 'Todas';
  currentPage = 1;
  itemsPerPage = 5;

  // ─── Modal State ──────────────────────────────────────────────────────────
  showNewTopicModal = false;
  newTopicCategory: string = '';
  newTopicTitle: string = '';
  newTopicSubject: string = '';
  newTopicMessage: string = '';
  newTopicLessonId: number | null = null;
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

  // ─── Data ────────────────────────────────────────────────────────────
  categories: IForumCategoryUI[] = [];
  topics: IForumTopicUI[] = [];
  categoryOptions: string[] = [];

  ngOnInit(): void {
    this.loadData();
  }

  async loadData(): Promise<void> {
    this.isLoading = true;
    
    try {
      const cats = await this.forumService.getCategories();
      this.categories = cats.map(c => this.mapCategory(c));
      this.categoryOptions = ['Todas', ...this.categories.map(c => c.name)];
      if (this.categories.length > 0) {
        this.newTopicCategory = this.categories[0].name;
      }
      
      const tps = await this.forumService.getTopics({ pageSize: 100 });
      this.topics = tps.map(t => this.mapTopic(t));
      this.calculateStats();
    } catch (error) {
      console.error('Erro ao buscar dados do fórum', error);
      this.triggerToast('Erro ao carregar os dados do fórum.');
    } finally {
      this.isLoading = false;
    }
  }

  mapCategory(dto: IForumCategoryDto): IForumCategoryUI {
    const colors = ['#6366f1', '#06b6d4', '#10b981', '#f59e0b', '#8b5cf6', '#ec4899'];
    const icons = ['fa-laptop-code', 'fa-database', 'fa-code', 'fa-cloud', 'fa-robot', 'fa-question-circle'];
    const idx = (dto.id - 1) % colors.length;
    
    return {
      id: dto.id,
      name: dto.name,
      description: dto.description || '',
      icon: icons[idx] || 'fa-comments',
      color: colors[idx] || '#6366f1',
      topicCount: 0, 
      memberCount: 0, // Placeholder
      lastActivity: '-',
      isRecent: false
    };
  }

  mapTopic(dto: IForumTopicSummaryDto): IForumTopicUI {
    let status: TopicStatus = dto.status === 'Resolved' ? 'Resolvido' : (dto.repliesCount > 0 ? 'Em andamento' : 'Sem resposta');
    
    const parts = (dto.authorName || 'User').split(' ');
    const initials = parts.slice(0, 2).map(n => n[0]).join('').toUpperCase();
    const isUnread = false; 
    const isOwn = false; 

    return {
      id: dto.id,
      categoryId: 0, 
      categoryName: dto.categoryName,
      authorName: dto.authorName,
      authorInitials: initials,
      authorColor: '#6366f1',
      title: dto.title,
      preview: '',
      replyCount: dto.repliesCount,
      viewCount: 0, 
      date: new Date(dto.createdAt).toLocaleDateString(),
      status: status,
      isUnread: isUnread,
      isOwn: isOwn,
      hasPendingReplies: dto.repliesCount > 0 && status !== 'Resolvido'
    };
  }

  calculateStats(): void {
    this.stats = {
        totalTopics: this.topics.length,
        totalReplies: this.topics.reduce((s, t) => s + t.replyCount, 0),
        unansweredTopics: this.topics.filter(t => t.status === 'Sem resposta').length,
        favoriteTopics: 0,
        unreadReplies: 0
    };
    
    this.categories.forEach(c => {
       c.topicCount = this.topics.filter(t => t.categoryName === c.name).length;
    });
  }

  // ─── Filtering ────────────────────────────────────────────────────────────

  get filteredTopics(): IForumTopicUI[] {
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

  get paginatedTopics(): IForumTopicUI[] {
    const start = (this.currentPage - 1) * this.itemsPerPage;
    return this.filteredTopics.slice(start, start + this.itemsPerPage);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredTopics.length / this.itemsPerPage) || 1;
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  get myTopics(): IForumTopicUI[] {
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
    this.newTopicTitle = '';
    this.newTopicSubject = '';
    this.newTopicMessage = '';
    this.newTopicLessonId = null;
    this.showNewTopicModal = true;
  }

  closeNewTopic(): void {
    this.showNewTopicModal = false;
  }

  async submitNewTopic(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.newTopicTitle.trim() || !this.newTopicSubject.trim() || !this.newTopicMessage.trim()) {
      this.triggerToast('Preencha o título, assunto e a mensagem do tópico.');
      return;
    }

    const cat = this.categories.find(c => c.name === this.newTopicCategory);
    
    if (cat) {
      const cmd: CreateForumTopicCommand = {
        categoryId: cat.id,
        lessonId: this.newTopicLessonId || undefined,
        title: this.newTopicTitle,
        subject: this.newTopicSubject,
        content: this.newTopicMessage
      };

      try {
        await this.forumService.createTopic(cmd);
        this.closeNewTopic();
        this.triggerToast('Tópico criado com sucesso!');
        await this.loadData();
      } catch (err) {
        this.triggerToast('Erro ao criar tópico.');
        console.error(err);
      }
    }
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
