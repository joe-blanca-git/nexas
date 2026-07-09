import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

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
  date: string;
}

@Component({
  selector: 'app-lesson-viewer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './lesson-viewer.component.html',
  styleUrls: ['./lesson-viewer.component.scss']
})
export class LessonViewerComponent implements OnInit {
  
  // --- Propriedades de Estado ---
  course!: Course;
  currentModule!: Module;
  currentLesson!: Lesson;
  comments: ForumComment[] = [];
  
  // Controle de interface
  newCommentText: string = '';
  isVideoLoading: boolean = true;
  safeVideoUrl!: SafeResourceUrl;

  constructor(private sanitizer: DomSanitizer) {}

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
    { id: 1, lessonId: 1, authorName: 'Carlos Silva', avatar: 'https://i.pravatar.cc/150?u=1', content: 'Muito boa essa introdução! Sempre tive dúvida sobre o momento certo de usar cada EPI.', date: '2 dias atrás' },
    { id: 2, lessonId: 1, authorName: 'Ana Paula', avatar: 'https://i.pravatar.cc/150?u=2', content: 'Excelente didática, parabéns.', date: '1 dia atrás' },
    { id: 3, lessonId: 2, authorName: 'Roberto Marcos', avatar: 'https://i.pravatar.cc/150?u=3', content: 'O painel do meu trator é um pouco diferente, mas os símbolos são iguais.', date: 'Ontem' },
    { id: 4, lessonId: 6, authorName: 'João Ferreira', avatar: 'https://i.pravatar.cc/150?u=4', content: 'Qual o óleo mais recomendado para tratores de grande porte?', date: '3 horas atrás' }
  ];

  // --- Lifecycle ---
  ngOnInit(): void {
    // Inicializar estado com o mock
    this.course = JSON.parse(JSON.stringify(this.MOCK_COURSE)); // Clone profundo
    
    // Selecionar primeira aula do primeiro módulo
    if (this.course.modules.length > 0 && this.course.modules[0].lessons.length > 0) {
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
    this.course.modules.forEach(m => m.isExpanded = (m.id === module.id));

    this.loadForum();
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
    alert(`Obrigado por avaliar o ${module.title}! Esta funcionalidade será implementada no futuro.`);
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

  addComment(): void {
    if (!this.newCommentText.trim()) return;

    const newComment: ForumComment = {
      id: Date.now(),
      lessonId: this.currentLesson.id,
      authorName: 'Você (Aluno)',
      avatar: 'https://i.pravatar.cc/150?u=current',
      content: this.newCommentText.trim(),
      date: 'Agora mesmo'
    };

    // Adiciona no início da lista local
    this.comments.unshift(newComment);
    
    // Opcional: Adicionar ao Mock Global se quiser que persista ao trocar de aula
    this.MOCK_COMMENTS.push(newComment); 

    this.newCommentText = '';
  }

  // --- Métodos Privados ---
  private loadForum(): void {
    // Filtra comentários para a aula atual e ordena (simulação do mais recente primeiro, invertendo a ordem original pra teste)
    this.comments = this.MOCK_COMMENTS
      .filter(c => c.lessonId === this.currentLesson.id)
      .reverse(); 
  }
}
