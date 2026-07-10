import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CoursesService } from '../../services/courses.service';
import { ForumService, CreateForumTopicCommand, IForumCategoryDto, IForumTopicSummaryDto } from '../../../forum/services/forum.service';
import { BreadcrumbService } from '../../../../../shared/components/breadcrumb/breadcrumb.service';
import { Breadcrumb } from '../../../../../shared/components/breadcrumb/breadcrumb.component';

// --- Interfaces ---
export interface Lesson {
  id: number;
  title: string;
  description: string;
  duration: string;
  videoUrl?: string;
  isCompleted: boolean;
}

export interface Module {
  id: number;
  title: string;
  description: string;
  lessons: Lesson[];
  isExpanded?: boolean;
}

export interface Course {
  id: number;
  title: string;
  description: string;
  modules: Module[];
}

export interface ForumComment {
  id: number;
  lessonId: number;
  authorName: string;
  avatar: string;
  content: string;
  subject?: string;
  date: string;
  repliesCount: number;
}

@Component({
  selector: 'app-lesson-viewer',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './lesson-viewer.component.html',
  styleUrls: ['./lesson-viewer.component.scss']
})
export class LessonViewerComponent implements OnInit, OnDestroy {
  
  // --- Propriedades de Estado ---
  course!: Course;
  currentModule!: Module;
  currentLesson!: Lesson;
  comments: ForumComment[] = [];
  
  // Controle de interface
  isVideoLoading: boolean = true;
  safeVideoUrl!: SafeResourceUrl;

  // --- Modal Novo Tópico ---
  showNewTopicModal = false;
  newTopicCategory: string = '';
  newTopicTitle: string = '';
  newTopicSubject: string = '';
  newTopicMessage: string = '';
  isSubmittingNewTopic = false;
  showToast = false;
  toastMessage = '';
  categories: { name: string, id: number }[] = [];

  // --- Modal Avaliação de Módulo ---
  showRatingModal = false;
  ratingModule: Module | null = null;
  selectedRating = 0;
  hoverRating = 0;
  isSubmittingRating = false;

  constructor(
    private sanitizer: DomSanitizer,
    private route: ActivatedRoute,
    private router: Router,
    private coursesService: CoursesService,
    private breadcrumbService: BreadcrumbService,
    private forumService: ForumService
  ) {}

  // --- Dados Mocados ---
  private readonly MOCK_COURSE: Course = {
    id: 1,
    title: 'Especialização em Máquinas Agrícolas',
    description: 'Curso completo de operação e manutenção avançada.',
    modules: [
      {
        id: 101,
        title: 'Módulo 1: Fundamentos Básicos',
        description: 'Introdução ao maquinário e segurança.',
        isExpanded: true,
        lessons: [
          { id: 1, title: 'Introdução à Segurança', description: 'Regras de segurança no campo e uso de EPIs.', duration: '12:30', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-sec-01?autoplay=false', isCompleted: false },
          { id: 2, title: 'Reconhecimento do Painel', description: 'Entendendo os indicadores do painel principal.', duration: '15:45', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-sec-02?autoplay=false', isCompleted: false },
          { id: 3, title: 'Inspeção Diária', description: 'O que checar antes de dar a partida.', duration: '08:20', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-sec-03?autoplay=false', isCompleted: false },
          { id: 4, title: 'Partida e Aquecimento', description: 'Procedimentos corretos para vida útil do motor.', duration: '10:15', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-sec-04?autoplay=false', isCompleted: false },
          { id: 5, title: 'Prática: Primeira Condução', description: 'Movimentos básicos e frenagem.', duration: '22:00', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-sec-05?autoplay=false', isCompleted: false }
        ]
      },
      {
        id: 102,
        title: 'Módulo 2: Sistemas Hidráulicos',
        description: 'Entendendo bombas, mangueiras e pistões.',
        isExpanded: false,
        lessons: [
          { id: 6, title: 'Bomba Hidráulica Principal', description: 'Como funciona a bomba de engrenagem.', duration: '18:10', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-hid-01?autoplay=false', isCompleted: false },
          { id: 7, title: 'Comandos Hidráulicos', description: 'Operando o joystick e válvulas direcionais.', duration: '25:30', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-hid-02?autoplay=false', isCompleted: false },
          { id: 8, title: 'Manutenção de Mangueiras', description: 'Identificando vazamentos e desgastes.', duration: '14:20', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-hid-03?autoplay=false', isCompleted: false },
          { id: 9, title: 'Troca de Fluido e Filtros', description: 'Procedimento correto para evitar contaminação.', duration: '30:45', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-hid-04?autoplay=false', isCompleted: false },
          { id: 10, title: 'Prática: Engate de Implementos', description: 'Acoplamento seguro de implementos.', duration: '20:15', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-hid-05?autoplay=false', isCompleted: false },
          { id: 11, title: 'Diagnóstico de Falhas Hidráulicas', description: 'Problemas comuns e soluções.', duration: '16:00', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-hid-06?autoplay=false', isCompleted: false }
        ]
      },
      {
        id: 103,
        title: 'Módulo 3: GPS e Agricultura de Precisão',
        description: 'Tecnologia embarcada e mapeamento.',
        isExpanded: false,
        lessons: [
          { id: 12, title: 'Configuração do Monitor', description: 'Ajustes iniciais e criação de fazenda/talhão.', duration: '21:10', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-gps-01?autoplay=false', isCompleted: false },
          { id: 13, title: 'Calibração de Piloto Automático', description: 'Ajuste de sensibilidade e rota.', duration: '19:40', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-gps-02?autoplay=false', isCompleted: false },
          { id: 14, title: 'Linhas AB e Contornos', description: 'Criando diferentes tipos de linhas de orientação.', duration: '24:15', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-gps-03?autoplay=false', isCompleted: false },
          { id: 15, title: 'Mapeamento de Colheita', description: 'Configurando sensores de produtividade.', duration: '28:30', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-gps-04?autoplay=false', isCompleted: false },
          { id: 16, title: 'Exportação de Dados', description: 'Extraindo mapas via pendrive ou nuvem.', duration: '11:50', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-gps-05?autoplay=false', isCompleted: false }
        ]
      },
      {
        id: 104,
        title: 'Módulo 4: Manutenção Preventiva Avançada',
        description: 'Evite quebras e maximize o tempo de máquina.',
        isExpanded: false,
        lessons: [
          { id: 17, title: 'Sistema de Injeção Eletrônica', description: 'Cuidados com filtros e sensores.', duration: '17:30', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-manut-01?autoplay=false', isCompleted: false },
          { id: 18, title: 'Arrefecimento do Motor', description: 'Limpeza de radiadores e aditivos.', duration: '15:20', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-manut-02?autoplay=false', isCompleted: false },
          { id: 19, title: 'Transmissão e Eixos', description: 'Lubrificação correta de pontos críticos.', duration: '22:10', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-manut-03?autoplay=false', isCompleted: false },
          { id: 20, title: 'Sistema Elétrico Base', description: 'Teste de baterias e alternador.', duration: '19:05', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-manut-04?autoplay=false', isCompleted: false },
          { id: 21, title: 'Prática: Revisão de 500 horas', description: 'O que fazer na revisão principal.', duration: '45:00', videoUrl: 'https://iframe.mediadelivery.net/embed/dummy/video-manut-05?autoplay=false', isCompleted: false }
        ]
      }
    ]
  };

  private readonly MOCK_COMMENTS: ForumComment[] = [
    { id: 1, lessonId: 1, authorName: 'Carlos Silva', avatar: 'https://i.pravatar.cc/150?u=1', content: 'Muito boa essa introdução! Sempre tive dúvida sobre o momento certo de usar cada EPI.', subject: 'Dúvida sobre EPIs', date: '2 dias atrás', repliesCount: 3 },
    { id: 2, lessonId: 1, authorName: 'Ana Paula', avatar: 'https://i.pravatar.cc/150?u=2', content: 'Excelente didática, parabéns.', subject: 'Feedback', date: '1 dia atrás', repliesCount: 0 },
    { id: 3, lessonId: 2, authorName: 'Roberto Marcos', avatar: 'https://i.pravatar.cc/150?u=3', content: 'O painel do meu trator é um pouco diferente, mas os símbolos são iguais.', subject: 'Painel do trator', date: 'Ontem', repliesCount: 1 },
    { id: 4, lessonId: 6, authorName: 'João Ferreira', avatar: 'https://i.pravatar.cc/150?u=4', content: 'Qual o óleo mais recomendado para tratores de grande porte?', subject: 'Recomendação de Óleo', date: '3 horas atrás', repliesCount: 5 }
  ];

  // --- Lifecycle ---
  async ngOnInit(): Promise<void> {
    const courseIdParam = this.route.snapshot.paramMap.get('courseId');
    const lessonIdParam = this.route.snapshot.queryParamMap.get('lessonId');

    // Carregar categorias do fórum para o modal
    try {
      const cats = await this.forumService.getCategories();
      this.categories = cats.map(c => ({ name: c.name, id: c.id }));
      if (this.categories.length > 0) {
        this.newTopicCategory = this.categories[0].name;
      }
    } catch (e) {
      console.error('Erro ao carregar categorias do fórum', e);
    }

    if (courseIdParam) {
      await this.loadRealCourse(+courseIdParam, lessonIdParam ? +lessonIdParam : null);
    } else {
      // Fallback para mock se não houver ID (para testes isolados)
      this.course = JSON.parse(JSON.stringify(this.MOCK_COURSE));
      this.initInitialLesson();
    }
  }

  ngOnDestroy(): void {
    // Limpar o breadcrumb customizado ao sair
    this.breadcrumbService.setBreadcrumbs(null);
  }

  private async loadRealCourse(courseId: number, lessonIdToSelect: number | null): Promise<void> {
    this.isVideoLoading = true;
    try {
      this.course = await this.coursesService.getCourseDetail(courseId);
      
      if (!this.course || !this.course.modules || this.course.modules.length === 0) {
        return;
      }

      if (lessonIdToSelect) {
        // Encontrar a aula solicitada
        for (const module of this.course.modules) {
          const lesson = module.lessons.find((l: Lesson) => l.id === lessonIdToSelect);
          if (lesson) {
            this.currentModule = module;
            this.setCurrentLesson(lesson, module);
            return;
          }
        }
      } else {
        // Procurar a primeira aula não vista
        let firstUnseenLesson: Lesson | null = null;
        let firstUnseenModule: Module | null = null;
        
        for (const module of this.course.modules) {
          if (module.lessons) {
            const unseen = module.lessons.find((l: Lesson) => !l.isCompleted);
            if (unseen) {
              firstUnseenLesson = unseen;
              firstUnseenModule = module;
              break;
            }
          }
        }

        if (firstUnseenLesson && firstUnseenModule) {
          this.currentModule = firstUnseenModule;
          this.setCurrentLesson(firstUnseenLesson, firstUnseenModule);
          return;
        }
      }
      
      // Se não houver aula específica ou não encontrou, e todas as aulas foram vistas, seleciona a primeira
      this.initInitialLesson();

    } catch (error) {
      console.error('Erro ao carregar curso no viewer:', error);
    } finally {
      this.isVideoLoading = false;
    }
  }

  private initInitialLesson(): void {
    if (this.course && this.course.modules.length > 0 && this.course.modules[0].lessons.length > 0) {
      this.currentModule = this.course.modules[0];
      this.setCurrentLesson(this.course.modules[0].lessons[0], this.course.modules[0]);
    }
  }

  // --- Métodos Públicos ---
  
  setCurrentLesson(lesson: Lesson, module: Module): void {
    if (this.currentLesson?.id === lesson.id) return;

    this.currentLesson = lesson;
    this.currentModule = module;
    
    // Configura a URL segura para o iframe diretamente do videoUrl
    if (lesson.videoUrl) {
      this.safeVideoUrl = this.sanitizer.bypassSecurityTrustResourceUrl(lesson.videoUrl);
    }

    // Simula carregamento de novo vídeo
    this.isVideoLoading = true;
    setTimeout(() => {
      this.isVideoLoading = false;
    }, 800);

    // Expandir o módulo clicado, e recolher os outros se desejar 
    // (Neste design, manteremos apenas o módulo atual aberto para focar na aula)
    if (this.course && this.course.modules) {
      this.course.modules.forEach(m => m.isExpanded = (m.id === module.id));
    }

    this.updateBreadcrumb();
    this.loadForum();

    // Marca como vista se ainda não foi
    if (!lesson.isCompleted) {
      this.markLessonAsViewed(lesson);
    }
  }

  private async markLessonAsViewed(lesson: Lesson): Promise<void> {
    try {
      await this.coursesService.toggleLessonView(lesson.id);
      lesson.isCompleted = true; // Atualiza o estado na interface imediatamente após sucesso
    } catch (e) {
      console.error('Erro ao marcar aula como vista', e);
    }
  }

  private updateBreadcrumb(): void {
    const breadcrumbs: Breadcrumb[] = [
      { label: 'Home', url: '/' },
      { label: 'Cursos', url: '/courses' },
      { label: this.course.title, url: `/courses/course-detail/${this.course.id}` },
      { label: this.currentLesson.title, url: `/courses/lesson/${this.course.id}?lessonId=${this.currentLesson.id}` }
    ];
    this.breadcrumbService.setBreadcrumbs(breadcrumbs);
  }

  goToNextLesson(): void {
    if (this.isLastLesson()) return;

    const moduleIndex = this.course.modules.findIndex(m => m.id === this.currentModule.id);
    const lessonIndex = this.currentModule.lessons.findIndex(l => l.id === this.currentLesson.id);

    if (lessonIndex < this.currentModule.lessons.length - 1) {
      // Próxima aula no mesmo módulo
      this.setCurrentLesson(this.currentModule.lessons[lessonIndex + 1], this.currentModule);
    } else if (moduleIndex < this.course.modules.length - 1) {
      // Primeira aula do próximo módulo
      const nextModule = this.course.modules[moduleIndex + 1];
      this.setCurrentLesson(nextModule.lessons[0], nextModule);
    }
  }

  goToPreviousLesson(): void {
    if (this.isFirstLesson()) return;

    const moduleIndex = this.course.modules.findIndex(m => m.id === this.currentModule.id);
    const lessonIndex = this.currentModule.lessons.findIndex(l => l.id === this.currentLesson.id);

    if (lessonIndex > 0) {
      // Aula anterior no mesmo módulo
      this.setCurrentLesson(this.currentModule.lessons[lessonIndex - 1], this.currentModule);
    } else if (moduleIndex > 0) {
      // Última aula do módulo anterior
      const prevModule = this.course.modules[moduleIndex - 1];
      this.setCurrentLesson(prevModule.lessons[prevModule.lessons.length - 1], prevModule);
    }
  }

  toggleLessonCompleted(): void {
    this.currentLesson.isCompleted = !this.currentLesson.isCompleted;
    // O Angular detectará a mudança de estado e o HTML será atualizado (incluindo ícones do sidebar)
  }

  toggleModuleAccordion(module: Module): void {
    module.isExpanded = !module.isExpanded;
  }

  checkModuleCompletion(moduleId: number): boolean {
    const mod = this.course.modules.find(m => m.id === moduleId);
    if (!mod || !mod.lessons || mod.lessons.length === 0) return false;
    return mod.lessons.every(lesson => lesson.isCompleted);
  }

  rateModule(module: Module): void {
    if (!this.checkModuleCompletion(module.id)) return;
    this.ratingModule = module;
    this.selectedRating = 0;
    this.hoverRating = 0;
    this.showRatingModal = true;
  }

  // --- Lógica de Avaliação ---
  closeRatingModal(): void {
    this.showRatingModal = false;
    this.ratingModule = null;
  }

  setHoverRating(rating: number): void {
    this.hoverRating = rating;
  }

  setRating(rating: number): void {
    this.selectedRating = rating;
  }

  async submitRating(): Promise<void> {
    if (this.selectedRating === 0) {
      this.triggerToast('Por favor, selecione uma nota para avaliar o módulo.');
      return;
    }

    if (!this.course || !this.course.id) return;

    this.isSubmittingRating = true;

    try {
      await this.coursesService.rateCourse(this.course.id, this.selectedRating);
      this.isSubmittingRating = false;
      this.closeRatingModal();
      this.triggerToast('Avaliação enviada com sucesso!');
      
      // Redirecionar para a página de detalhes do curso
      this.router.navigate(['/courses/course-detail', this.course.id]);
    } catch (error) {
      console.error('Erro ao avaliar curso', error);
      this.isSubmittingRating = false;
      this.triggerToast('Não foi possível enviar sua avaliação no momento.');
    }
  }

  isFirstLesson(): boolean {
    if (!this.course || !this.currentModule || !this.currentLesson) return true;
    const isFirstModule = this.course.modules[0].id === this.currentModule.id;
    const isFirstLessonOfModule = this.currentModule.lessons[0].id === this.currentLesson.id;
    return isFirstModule && isFirstLessonOfModule;
  }

  isLastLesson(): boolean {
    if (!this.course || !this.currentModule || !this.currentLesson) return true;
    const lastModule = this.course.modules[this.course.modules.length - 1];
    const isLastModule = lastModule.id === this.currentModule.id;
    const isLastLessonOfModule = lastModule.lessons[lastModule.lessons.length - 1].id === this.currentLesson.id;
    return isLastModule && isLastLessonOfModule;
  }

  getCompletedLessonsCount(module: Module): number {
    if (!module || !module.lessons) return 0;
    return module.lessons.filter(l => l.isCompleted).length;
  }

  // --- Fórum ---
  
  openNewTopic(): void {
    this.newTopicTitle = '';
    this.newTopicSubject = '';
    this.newTopicMessage = '';
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
    if (!cat) return;
    
    this.isSubmittingNewTopic = true;
    const cmd: CreateForumTopicCommand = {
      categoryId: cat.id,
      lessonId: this.currentLesson.id,
      title: this.newTopicTitle,
      subject: this.newTopicSubject,
      content: this.newTopicMessage
    };

    try {
      await this.forumService.createTopic(cmd);
      this.closeNewTopic();
      this.triggerToast('Tópico criado com sucesso!');
      await this.loadForum(); // Recarrega a lista
    } catch (err) {
      this.triggerToast('Erro ao criar tópico.');
      console.error(err);
    } finally {
      this.isSubmittingNewTopic = false;
    }
  }

  triggerToast(message: string): void {
    this.toastMessage = message;
    this.showToast = true;
    setTimeout(() => (this.showToast = false), 3000);
  }

  // --- Métodos Privados ---
  private async loadForum(): Promise<void> {
    try {
      // Busca tópicos reais do fórum vinculados a esta aula
      const topics = await this.forumService.getTopics({ lessonId: this.currentLesson.id, pageSize: 50 });
      
      this.comments = topics.map(t => ({
        id: t.id,
        lessonId: this.currentLesson.id,
        authorName: t.authorName || 'Usuário',
        avatar: 'https://ui-avatars.com/api/?name=' + encodeURIComponent(t.authorName || 'U') + '&background=random',
        content: t.title,
        subject: (t as any).subject || (t as any).content || '', // O backend pode retornar subject ou content dependendo da modelagem
        date: new Date(t.createdAt).toLocaleDateString(),
        repliesCount: t.repliesCount || 0
      }));
    } catch (e) {
      console.error('Erro ao buscar tópicos da aula', e);
      // Fallback para mock caso a API falhe para que a tela não quebre
      this.comments = this.MOCK_COMMENTS
        .filter(c => c.lessonId === this.currentLesson.id)
        .reverse();
    }
  }
}
