import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// ─── Interfaces ────────────────────────────────────────────────────────────────

export interface IFaqItem {
  id: number;
  category: string;
  question: string;
  answer: string;
  isOpen: boolean;
}

export interface IFaqCategory {
  id: string;
  name: string;
  icon: string;
  color: string;
}

export interface ISupportChannel {
  id: string;
  name: string;
  description: string;
  icon: string;
  color: string;
  responseTime: string;
  isOnline: boolean;
  actionLabel: string;
  actionHref: string;
}

// ─── Component ─────────────────────────────────────────────────────────────────

@Component({
  selector: 'app-support-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './support-home.component.html',
  styleUrl: './support-home.component.scss'
})
export class SupportHomeComponent implements OnInit {

  // ─── State ────────────────────────────────────────────────────────────────
  isLoading = true;
  searchTerm = '';
  selectedCategory = 'Todas';

  // ─── Contact Form State ───────────────────────────────────────────────────
  contactName = '';
  contactEmail = '';
  contactSubject = '';
  contactMessage = '';
  contactCategory = 'Dúvida Geral';
  toastMessage = '';
  showToast = false;

  // ─── Data ─────────────────────────────────────────────────────────────────
  faqCategories: IFaqCategory[] = [];
  faqItems: IFaqItem[] = [];
  channels: ISupportChannel[] = [];

  ngOnInit(): void {
    setTimeout(() => {
      this.faqCategories = [
        { id: 'Todas', name: 'Todas', icon: 'fa-th', color: '#6366f1' },
        { id: 'Pagamentos', name: 'Pagamentos', icon: 'fa-credit-card', color: '#10b981' },
        { id: 'Cursos', name: 'Cursos', icon: 'fa-book-open', color: '#6366f1' },
        { id: 'Certificados', name: 'Certificados', icon: 'fa-certificate', color: '#f59e0b' },
        { id: 'Conta', name: 'Conta', icon: 'fa-user-circle', color: '#8b5cf6' },
        { id: 'Assinatura', name: 'Assinatura', icon: 'fa-crown', color: '#ec4899' },
        { id: 'Plataforma', name: 'Plataforma', icon: 'fa-desktop', color: '#06b6d4' }
      ];

      this.faqItems = [
        {
          id: 1,
          category: 'Pagamentos',
          question: 'Quais formas de pagamento são aceitas na plataforma?',
          answer: 'Aceitamos cartão de crédito (Visa, Mastercard, Amex e Elo), cartão de débito, PIX e boleto bancário. Para assinaturas recorrentes, recomendamos o uso de cartão de crédito, pois a renovação é feita automaticamente.',
          isOpen: false
        },
        {
          id: 2,
          category: 'Pagamentos',
          question: 'Como solicitar reembolso de uma compra?',
          answer: 'Você pode solicitar reembolso em até 7 dias corridos após a data de compra, desde que não tenha acessado mais de 20% do conteúdo do curso. Para iniciar, acesse Financeiro > Histórico de Pagamentos e clique em "Ver Detalhes" na transação desejada.',
          isOpen: false
        },
        {
          id: 3,
          category: 'Assinatura',
          question: 'Como cancelar minha assinatura Nexas Premium?',
          answer: 'Para cancelar sua assinatura, acesse Financeiro > Assinatura Ativa > Gerenciar Assinatura > Cancelar. O cancelamento é imediato e você mantém o acesso até o final do período já pago. Não realizamos cobranças adicionais após o cancelamento.',
          isOpen: false
        },
        {
          id: 4,
          category: 'Assinatura',
          question: 'O que está incluído no plano Nexas Premium?',
          answer: 'O Plano Premium inclui acesso ilimitado a todos os cursos da plataforma, suporte prioritário, certificados digitais verificáveis, downloads de material complementar, acesso antecipado a novos conteúdos e participação nos fóruns exclusivos de alunos premium.',
          isOpen: false
        },
        {
          id: 5,
          category: 'Cursos',
          question: 'Posso acessar os cursos sem conexão com a internet?',
          answer: 'No momento, o acesso aos cursos requer conexão com a internet. Estamos desenvolvendo uma funcionalidade de download offline que estará disponível em breve para assinantes Premium.',
          isOpen: false
        },
        {
          id: 6,
          category: 'Cursos',
          question: 'Os cursos possuem data de expiração para assistir?',
          answer: 'Cursos comprados individualmente têm acesso vitalício — você pode assisti-los quantas vezes quiser, sem prazo de expiração. Assinantes Nexas Premium têm acesso a todos os cursos enquanto a assinatura estiver ativa.',
          isOpen: false
        },
        {
          id: 7,
          category: 'Certificados',
          question: 'Como baixar meu certificado de conclusão?',
          answer: 'Após concluir 100% das aulas de um curso, acesse a página "Certificados" no menu lateral. Seus certificados estarão listados e disponíveis para download em PDF. Cada certificado possui um código único de verificação para fins de autenticidade.',
          isOpen: false
        },
        {
          id: 8,
          category: 'Certificados',
          question: 'Os certificados da Nexas são reconhecidos pelo mercado?',
          answer: 'Nossos certificados são emitidos com verificação digital e um código de autenticidade único que pode ser validado por qualquer empresa empregadora em nexas.com.br/verificar. Cada certificado exibe o nome completo do curso, carga horária e data de conclusão.',
          isOpen: false
        },
        {
          id: 9,
          category: 'Conta',
          question: 'Como alterar minha senha de acesso?',
          answer: 'Acesse seu perfil clicando no avatar no canto superior direito > Dados Cadastrais > Segurança > Alterar Senha. Você precisará confirmar a senha atual antes de definir a nova. Também é possível redefinir a senha pela tela de login clicando em "Esqueci a senha".',
          isOpen: false
        },
        {
          id: 10,
          category: 'Conta',
          question: 'Como atualizar meu e-mail de cadastro?',
          answer: 'Para alterar seu e-mail de cadastro, acesse Perfil > Dados Cadastrais > E-mail. Um link de confirmação será enviado para o novo endereço antes de a mudança ser efetivada.',
          isOpen: false
        },
        {
          id: 11,
          category: 'Plataforma',
          question: 'Quais navegadores são compatíveis com a plataforma?',
          answer: 'A Nexas é compatível com as versões mais recentes do Google Chrome, Mozilla Firefox, Microsoft Edge e Safari. Para melhor experiência, recomendamos o uso do Google Chrome em sua versão mais atualizada.',
          isOpen: false
        },
        {
          id: 12,
          category: 'Plataforma',
          question: 'O que fazer quando as aulas não carregam corretamente?',
          answer: 'Verifique sua conexão com a internet e tente limpar o cache do navegador (Ctrl+Shift+Del). Se o problema persistir, tente acessar em uma aba anônima ou em outro navegador. Se ainda não funcionar, entre em contato com nosso suporte descrevendo o problema e o código do curso.',
          isOpen: false
        }
      ];

      this.channels = [
        {
          id: 'email',
          name: 'Suporte por E-mail',
          description: 'Envie uma mensagem detalhada sobre seu problema. Nossa equipe analisa e responde com todas as informações necessárias para a resolução.',
          icon: 'fa-envelope',
          color: '#6366f1',
          responseTime: 'Resposta em até 24 horas úteis',
          isOnline: true,
          actionLabel: 'Enviar Mensagem',
          actionHref: 'mailto:suporte@nexas.com.br'
        },
        {
          id: 'forum',
          name: 'Fórum da Comunidade',
          description: 'Publique sua dúvida no fórum e receba ajuda de professores e colegas. A maioria das dúvidas já foi respondida lá!',
          icon: 'fa-comments',
          color: '#10b981',
          responseTime: 'Resposta normalmente em horas',
          isOnline: true,
          actionLabel: 'Acessar Fórum',
          actionHref: '/forum'
        },
        {
          id: 'whatsapp',
          name: 'Suporte via WhatsApp',
          description: 'Converse com nosso assistente virtual 24h por dia para resolver dúvidas rápidas sobre a plataforma, pagamentos e acesso.',
          icon: 'fa-whatsapp',
          color: '#25d366',
          responseTime: 'Atendimento imediato',
          isOnline: true,
          actionLabel: 'Iniciar Conversa',
          actionHref: 'https://wa.me/5511900000000'
        }
      ];

      this.isLoading = false;
    }, 900);
  }

  // ─── FAQ Filtering ────────────────────────────────────────────────────────

  get filteredFaqs(): IFaqItem[] {
    return this.faqItems.filter(item => {
      const matchCategory = this.selectedCategory === 'Todas' || item.category === this.selectedCategory;
      const matchSearch = !this.searchTerm.trim() ||
        item.question.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        item.answer.toLowerCase().includes(this.searchTerm.toLowerCase());
      return matchCategory && matchSearch;
    });
  }

  toggleFaq(id: number): void {
    this.faqItems = this.faqItems.map(item => ({
      ...item,
      isOpen: item.id === id ? !item.isOpen : false
    }));
  }

  setCategory(cat: string): void {
    this.selectedCategory = cat;
  }

  // ─── Contact Form ─────────────────────────────────────────────────────────

  submitContactForm(event: Event): void {
    event.preventDefault();
    if (!this.contactName.trim() || !this.contactEmail.trim() || !this.contactMessage.trim()) {
      this.triggerToast('Por favor, preencha todos os campos obrigatórios.');
      return;
    }
    this.triggerToast('Mensagem enviada com sucesso! Responderemos em breve.');
    this.contactName = '';
    this.contactEmail = '';
    this.contactSubject = '';
    this.contactMessage = '';
  }

  // ─── Toast ────────────────────────────────────────────────────────────────

  triggerToast(message: string): void {
    this.toastMessage = message;
    this.showToast = true;
    setTimeout(() => (this.showToast = false), 3500);
  }
}
