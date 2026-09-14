import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SupportService, IAdminTicket, IAdminTicketDetails, IPaginatedList } from '../../services/support.service';
import { ToastService } from '../../../../../core/services/toast.service';

@Component({
  selector: 'app-support-tickets',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './support-tickets.component.html',
  styleUrl: './support-tickets.component.scss'
})
export class SupportTicketsComponent implements OnInit {
  private supportService = inject(SupportService);
  private fb = inject(FormBuilder);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);

  tickets: IAdminTicket[] = [];
  selectedTicket: IAdminTicketDetails | null = null;
  isLoading = true;
  isReplying = false;

  replyForm!: FormGroup;
  searchQuery = '';
  statusFilter = '';

  ngOnInit(): void {
    this.replyForm = this.fb.group({
      content: ['', Validators.required]
    });
    this.loadTickets();
  }

  loadTickets(): void {
    this.isLoading = true;
    this.supportService.getTickets(this.statusFilter || undefined, undefined, this.searchQuery || undefined).subscribe({
      next: (data) => {
        this.tickets = data.items;
        this.isLoading = false;

        // Auto-select ticket if query parameter is present
        this.route.queryParams.subscribe(params => {
          const ticketId = params['ticketId'];
          if (ticketId) {
            const ticketToSelect = this.tickets.find(t => t.id.toString() === ticketId);
            if (ticketToSelect) {
              this.selectTicket(ticketToSelect);
            } else {
              // Optionally fetch directly by ID if not in current page, but for now we try to find it
              this.supportService.getTicketById(+ticketId).subscribe({
                next: (details: any) => {
                  this.selectedTicket = details;
                  this.replyForm.reset();
                }
              });
            }
          }
        });
      },
      error: () => {
        this.toastService.error('Não foi possível carregar os chamados.');
        this.isLoading = false;
      }
    });
  }

  onSearch(event: any): void {
    this.searchQuery = event.target.value;
    this.loadTickets();
  }

  onFilterStatus(event: any): void {
    this.statusFilter = event.target.value;
    this.loadTickets();
  }

  selectTicket(ticket: IAdminTicket): void {
    this.supportService.getTicketById(ticket.id).subscribe({
      next: (details: any) => {
        this.selectedTicket = details;
        this.replyForm.reset();
      },
      error: () => {
        this.toastService.error('Erro ao carregar detalhes do chamado.');
      }
    });
  }

  closeTicketView(): void {
    this.selectedTicket = null;
  }

  submitReply(): void {
    if (this.replyForm.invalid || !this.selectedTicket) return;

    this.isReplying = true;
    const content = this.replyForm.value.content;

    this.supportService.replyTicket(this.selectedTicket.id, content).subscribe({
      next: () => {
        this.toastService.success('Resposta enviada com sucesso!');
        this.selectTicket(this.selectedTicket!);
        this.isReplying = false;
        this.loadTickets();
      },
      error: () => {
        this.toastService.error('Não foi possível enviar a resposta.');
        this.isReplying = false;
      }
    });
  }

  changeStatus(newStatus: string): void {
    if (!this.selectedTicket) return;
    
    this.supportService.updateTicketStatus(this.selectedTicket.id, newStatus).subscribe({
      next: () => {
        this.toastService.success(`Status alterado.`);
        this.selectedTicket!.status = newStatus;
        const index = this.tickets.findIndex(t => t.id === this.selectedTicket!.id);
        if (index > -1) this.tickets[index].status = newStatus;
      },
      error: () => {
        this.toastService.error('Não foi possível alterar o status.');
      }
    });
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case '0': return 'bg-warning bg-opacity-10 text-warning border-warning';
      case '1': return 'bg-info bg-opacity-10 text-info border-info';
      case '2': return 'bg-primary bg-opacity-10 text-primary border-primary';
      case '3': return 'bg-success bg-opacity-10 text-success border-success';
      default: return 'bg-secondary bg-opacity-10 text-secondary border-secondary';
    }
  }
  
  getStatusName(status: string): string {
    switch (status) {
      case '0': return 'Aberto';
      case '1': return 'Pendente';
      case '2': return 'Respondido'; // Mantido para compatibilidade com tickets antigos
      case '3': return 'Fechado';
      default: return 'Desconhecido';
    }
  }

  getInitials(name: string): string {
    if (!name || !name.trim()) return 'A';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return parts[0][0].toUpperCase();
  }

  isSupportMessage(msg: any): boolean {
    if (msg.origin === 'Backoffice') return true;
    if (msg.senderName && msg.senderName !== this.selectedTicket?.studentName) return true;
    return false;
  }
}
