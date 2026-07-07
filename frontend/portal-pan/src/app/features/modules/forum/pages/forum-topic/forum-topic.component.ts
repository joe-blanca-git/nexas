import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ForumService, IForumTopicDetailDto } from '../../services/forum.service';

@Component({
  selector: 'app-forum-topic',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './forum-topic.component.html',
  styleUrl: './forum-topic.component.scss'
})
export class ForumTopicComponent implements OnInit {
  topicId: number = 0;
  topic: IForumTopicDetailDto | null = null;
  isLoading: boolean = true;
  errorMessage: string = '';

  newReplyContent: string = '';
  isSubmittingReply: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private forumService: ForumService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.topicId = +idParam;
        this.loadTopicData();
      } else {
        this.errorMessage = 'ID do tópico não fornecido.';
        this.isLoading = false;
      }
    });
  }

  async loadTopicData() {
    this.isLoading = true;
    this.errorMessage = '';
    try {
      this.topic = await this.forumService.getTopicById(this.topicId);
    } catch (error) {
      console.error(error);
      this.errorMessage = 'Erro ao carregar os detalhes do tópico.';
    } finally {
      this.isLoading = false;
    }
  }

  goBack() {
    this.router.navigate(['/forum']);
  }

  getAuthorInitials(name: string): string {
    if (!name || name === 'Anônimo') return 'A';
    const parts = name.split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
  }

  getAuthorColor(name: string): string {
    if (!name || name === 'Anônimo') return '#94a3b8';
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    const c = (hash & 0x00FFFFFF).toString(16).toUpperCase();
    return '#' + '00000'.substring(0, 6 - c.length) + c;
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Open': return 'bg-success bg-opacity-10 text-success';
      case 'Resolved': return 'bg-primary bg-opacity-10 text-primary';
      case 'Closed': return 'bg-secondary bg-opacity-10 text-secondary';
      default: return 'bg-dark bg-opacity-10 text-dark';
    }
  }

  getStatusIcon(status: string): string {
    switch (status) {
      case 'Open': return 'fa-lock-open';
      case 'Resolved': return 'fa-check-circle';
      case 'Closed': return 'fa-lock';
      default: return 'fa-circle';
    }
  }

  async submitReply() {
    if (!this.newReplyContent.trim() || !this.topic) return;

    this.isSubmittingReply = true;
    try {
      await this.forumService.replyTopic(this.topic.id, {
        topicId: this.topic.id,
        content: this.newReplyContent
      });
      this.newReplyContent = '';
      await this.loadTopicData(); // Reload to show the new message
    } catch (error) {
      console.error('Erro ao enviar resposta:', error);
      alert('Não foi possível enviar a resposta.');
    } finally {
      this.isSubmittingReply = false;
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('pt-BR', {
      day: '2-digit', month: 'short', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    }).format(date);
  }
}
