import { Injectable, Injector } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { BaseService } from '../../../../core/services/base.service';
import { Course } from '../models/course.model';

@Injectable({
  providedIn: 'root'
})
export class CoursesService extends BaseService {

  constructor(protected override injector: Injector) {
    super(injector);
  }

  // TODO: Replace with actual HTTP GET when the endpoint is ready
  // return this.http.get<Course[]>(`${this.urlApiService}courses`, this.GetAuthHeaderJson());
  getCourses(): Observable<Course[]> {
    return of(COURSES_MOCK).pipe(delay(800)); // Simulando delay de rede
  }

  createCourse(courseData: Course): Observable<Course> {
    // Simulating POST request
    console.log('Simulating course creation payload:', courseData);
    return of(courseData).pipe(delay(1500));
  }
}

const COURSES_MOCK: Course[] = [
  {
    "id": 1,
    "name": "Mestres do Operations Center John Deere",
    "description": "Domine a plataforma de gestão agrícola mais avançada do mercado. Este curso completo foi desenvolvido para profissionais do agronegócio, desde pequenos produtores até gestores de grandes grupos agrícolas, que buscam otimizar a eficiência operacional, reduzir custos e maximizar a produtividade através do John Deere Operations Center. Ao longo de 11 módulos, você aprenderá desde a configuração inicial de sua organização até a análise profunda de dados agronômicos e gestão de manutenção preventiva, transformando informações complexas em decisões estratégicas para o seu negócio.",
    "descriptionSub": "Transforme dados em produtividade: o guia definitivo para dominar o ecossistema de gestão agrícola da John Deere.",
    "level": "Avançado",
    "priceSingle": 1249.9,
    "imgCoverLink": "https://i.pinimg.com/736x/e1/8e/56/e18e5633fad7699117edbd91bdbe745b.jpg",
    "bunnyLibraryId": "",
    "modules": [
      {
        "id": 1,
        "name": "Módulo 1: Bem-vindo ao Operations Center",
        "description": "Entenda os fundamentos e a arquitetura do ecossistema de conectividade da John Deere, preparando sua base de conhecimento para a transformação digital.",
        "descriptionSub": "Fundamentos da conectividade agrícola e visão geral da plataforma.",
        "imgCoverLink": "",
        "bunnyCollectionId": "",
        "lessons": [
          {
            "id": 1,
            "name": "O que é o Operations Center",
            "description": "Introdução aos conceitos de agricultura de precisão e a visão centralizadora da plataforma.",
            "durationSeconds": 480,
            "bunnyVideoId": ""
          },
          {
            "id": 2,
            "name": "Como funciona o ecossistema John Deere",
            "description": "A jornada dos dados desde a máquina no campo até o processamento na nuvem.",
            "durationSeconds": 720,
            "bunnyVideoId": ""
          },
          {
            "id": 3,
            "name": "Principais benefícios da plataforma",
            "description": "Análise de valor sobre eficiência, tomada de decisão e redução de desperdícios.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          },
          {
            "id": 4,
            "name": "Navegando pela interface",
            "description": "Exploração prática do menu principal e áreas de trabalho do usuário.",
            "durationSeconds": 900,
            "bunnyVideoId": ""
          }
        ]
      },
      {
        "id": 2,
        "name": "Módulo 2: Primeiros Passos",
        "description": "Processo estruturado para a criação de sua identidade digital na plataforma, garantindo segurança e acesso correto aos recursos de sua organização.",
        "descriptionSub": "Configuração da conta, validação de acesso e criação da estrutura organizacional.",
        "imgCoverLink": "",
        "bunnyCollectionId": "",
        "lessons": [
          {
            "id": 5,
            "name": "Criando uma conta",
            "description": "Passo a passo para o registro inicial no John Deere Access.",
            "durationSeconds": 540,
            "bunnyVideoId": ""
          },
          {
            "id": 6,
            "name": "Confirmando acesso",
            "description": "Validação de e-mail e configurações de segurança de conta.",
            "durationSeconds": 300,
            "bunnyVideoId": ""
          },
          {
            "id": 7,
            "name": "Criando uma Organização",
            "description": "Estabelecendo a entidade jurídica ou grupo de gestão dentro do sistema.",
            "durationSeconds": 660,
            "bunnyVideoId": ""
          },
          {
            "id": 8,
            "name": "Configurações iniciais",
            "description": "Ajustes básicos de perfil e preferências do usuário.",
            "durationSeconds": 780,
            "bunnyVideoId": ""
          }
        ]
      },
      {
        "id": 3,
        "name": "Módulo 3: Estruturando sua Organização",
        "description": "A base de dados é fundamental. Aprenda a cadastrar e organizar corretamente todos os ativos e pessoas que movem a sua operação.",
        "descriptionSub": "Cadastros essenciais: fazendas, equipamentos, insumos e equipe.",
        "imgCoverLink": "",
        "bunnyCollectionId": "",
        "lessons": [
          {
            "id": 9,
            "name": "Cadastro de Fazenda",
            "description": "Inserindo as informações básicas de localização e propriedades.",
            "durationSeconds": 840,
            "bunnyVideoId": ""
          },
          {
            "id": 10,
            "name": "Editando Fazendas",
            "description": "Manutenção e atualização de dados cadastrais das propriedades.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          },
          {
            "id": 11,
            "name": "Organização das Fazendas",
            "description": "Estruturação hierárquica e agrupamento de áreas.",
            "durationSeconds": 720,
            "bunnyVideoId": ""
          },
          {
            "id": 12,
            "name": "Boas práticas de cadastro",
            "description": "Padronização de nomenclaturas para relatórios precisos.",
            "durationSeconds": 500,
            "bunnyVideoId": ""
          },
          {
            "id": 13,
            "name": "Cadastro de Equipamentos",
            "description": "Registrando sua frota no Operations Center.",
            "durationSeconds": 900,
            "bunnyVideoId": ""
          },
          {
            "id": 14,
            "name": "Gerenciamento de Equipamentos",
            "description": "Vinculação e manutenção dos ativos cadastrados.",
            "durationSeconds": 750,
            "bunnyVideoId": ""
          },
          {
            "id": 15,
            "name": "Cadastro de Produtos",
            "description": "Registrando insumos, variedades e defensivos utilizados.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          },
          {
            "id": 16,
            "name": "Organização de Produtos",
            "description": "Classificação e gestão do catálogo de produtos agrícolas.",
            "durationSeconds": 540,
            "bunnyVideoId": ""
          },
          {
            "id": 17,
            "name": "Cadastro de Usuários",
            "description": "Adicionando membros à sua organização.",
            "durationSeconds": 660,
            "bunnyVideoId": ""
          },
          {
            "id": 18,
            "name": "Permissões e Equipes",
            "description": "Definindo níveis de acesso e hierarquia operacional.",
            "durationSeconds": 960,
            "bunnyVideoId": ""
          }
        ]
      },
      {
        "id": 4,
        "name": "Módulo 4: Configurações da Organização",
        "description": "Personalize o comportamento da plataforma para atender às demandas específicas do seu modelo de negócio agrícola.",
        "descriptionSub": "Refinando preferências globais e operacionais da empresa.",
        "imgCoverLink": "",
        "bunnyCollectionId": "",
        "lessons": [
          {
            "id": 19,
            "name": "Preferências da Organização",
            "description": "Configuração de unidades de medida, fusos horários e exibição.",
            "durationSeconds": 720,
            "bunnyVideoId": ""
          },
          {
            "id": 20,
            "name": "Configurações Operacionais",
            "description": "Parâmetros globais que afetam o monitoramento em campo.",
            "durationSeconds": 840,
            "bunnyVideoId": ""
          },
          {
            "id": 21,
            "name": "Boas práticas de configuração",
            "description": "Dicas para um ambiente de trabalho otimizado e seguro.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          }
        ]
      },
      {
        "id": 5,
        "name": "Módulo 5: Gestão Avançada de Fazendas",
        "description": "Explore as ferramentas geospaciais da John Deere para ter um controle absoluto sobre os limites, talhões e caminhos de suas operações.",
        "descriptionSub": "Domínio técnico de limites, linhas, mapeamento e ferramentas geográficas.",
        "imgCoverLink": "",
        "bunnyCollectionId": "",
        "lessons": [
          {
            "id": 22,
            "name": "Origem das Fazendas",
            "description": "Importação de dados geográficos externos e histórico.",
            "durationSeconds": 900,
            "bunnyVideoId": ""
          },
          {
            "id": 23,
            "name": "Entendendo Limites",
            "description": "Conceitos de perímetro e áreas de aplicação.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          },
          {
            "id": 24,
            "name": "Criando Limites",
            "description": "Ferramentas de desenho manual e edição de limites.",
            "durationSeconds": 960,
            "bunnyVideoId": ""
          },
          {
            "id": 25,
            "name": "Editando Limites",
            "description": "Refinamento técnico e correção de geometrias existentes.",
            "durationSeconds": 720,
            "bunnyVideoId": ""
          },
          {
            "id": 26,
            "name": "Linhas de Orientação",
            "description": "Criação de guias para o sistema de direção automática.",
            "durationSeconds": 840,
            "bunnyVideoId": ""
          },
          {
            "id": 27,
            "name": "Importação e Exportação",
            "description": "Transferência de arquivos entre softwares de gestão e o portal.",
            "durationSeconds": 780,
            "bunnyVideoId": ""
          },
          {
            "id": 28,
            "name": "Mesclagem de Fazendas",
            "description": "Consolidação de dados espaciais para gestão unificada.",
            "durationSeconds": 660,
            "bunnyVideoId": ""
          },
          {
            "id": 29,
            "name": "AutoPath",
            "description": "Configuração inteligente de linhas baseada no plantio.",
            "durationSeconds": 900,
            "bunnyVideoId": ""
          },
          {
            "id": 30,
            "name": "Marcadores",
            "description": "Identificação de pontos de interesse e anomalias no campo.",
            "durationSeconds": 540,
            "bunnyVideoId": ""
          },
          {
            "id": 31,
            "name": "Boas práticas para organização das áreas",
            "description": "Estratégias para manter a base de dados cartográfica limpa.",
            "durationSeconds": 700,
            "bunnyVideoId": ""
          }
        ]
      },
      {
        "id": 6,
        "name": "Módulo 6: Planejamento Operacional",
        "description": "Reduza incertezas no campo com um planejamento robusto e envie planos diretamente para as máquinas via conectividade sem fio.",
        "descriptionSub": "Do escritório para a máquina: fluxo eficiente de trabalho.",
        "imgCoverLink": "",
        "bunnyCollectionId": "",
        "lessons": [
          {
            "id": 32,
            "name": "O que é um Plano de Trabalho",
            "description": "Entendendo o conceito de 'Setup' e planejamento preventivo.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          },
          {
            "id": 33,
            "name": "Criando Planos",
            "description": "Estruturação de metas e atividades para o operador.",
            "durationSeconds": 900,
            "bunnyVideoId": ""
          },
          {
            "id": 34,
            "name": "Editando Planos",
            "description": "Ajustando planos baseados em horas de uso real.",
            "durationSeconds": 720,
            "bunnyVideoId": ""
          },
          {
            "id": 35,
            "name": "Enviando Planos às Máquinas",
            "description": "Utilização do Data Sync para envio sem fio.",
            "durationSeconds": 840,
            "bunnyVideoId": ""
          },
          {
            "id": 36,
            "name": "Monitorando Execução",
            "description": "Acompanhamento em tempo real do progresso planejado.",
            "durationSeconds": 960,
            "bunnyVideoId": ""
          }
        ]
      },
      {
        "id": 7,
        "name": "Módulo 7: Prescrições Agronômicas",
        "description": "Implemente taxa variável e estratégias agronômicas de precisão, aumentando a rentabilidade através da aplicação correta de insumos.",
        "descriptionSub": "Tecnologia de taxa variável aplicada à agricultura de precisão.",
        "imgCoverLink": "",
        "bunnyCollectionId": "",
        "lessons": [
          {
            "id": 37,
            "name": "O que são Prescrições",
            "description": "Conceitos básicos de prescrição agronômica.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          },
          {
            "id": 38,
            "name": "Criando Prescrições",
            "description": "Importação e criação de mapas de aplicação variável.",
            "durationSeconds": 900,
            "bunnyVideoId": ""
          },
          {
            "id": 39,
            "name": "Utilizando Prescrições nas Operações",
            "description": "Execução em campo com controle de seção e taxa.",
            "durationSeconds": 840,
            "bunnyVideoId": ""
          }
        ]
      },
      {
        "id": 8,
        "name": "Módulo 8: Inteligência e Análise de Dados",
        "description": "Transforme a massa de dados gerada por sua frota em indicadores estratégicos de performance e saúde operacional.",
        "descriptionSub": "Análise profunda: do talhão ao relatório executivo.",
        "imgCoverLink": "",
        "bunnyCollectionId": "",
        "lessons": [
          {
            "id": 40,
            "name": "Analisador de Talhão: Visão Geral",
            "description": "Introdução aos indicadores de produtividade por área.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          },
          {
            "id": 41,
            "name": "Analisador de Talhão: Comparativos",
            "description": "Comparando safra com safra e talhão com talhão.",
            "durationSeconds": 840,
            "bunnyVideoId": ""
          },
          {
            "id": 42,
            "name": "Analisador de Talhão: Métricas",
            "description": "Entendendo os principais KPIs agronômicos.",
            "durationSeconds": 720,
            "bunnyVideoId": ""
          },
          {
            "id": 43,
            "name": "Analisador de Talhão: Filtros",
            "description": "Filtragem de dados para análise de cenários.",
            "durationSeconds": 660,
            "bunnyVideoId": ""
          },
          {
            "id": 44,
            "name": "Analisador de Talhão: Relatórios",
            "description": "Extração de resultados para apresentações.",
            "durationSeconds": 900,
            "bunnyVideoId": ""
          },
          {
            "id": 45,
            "name": "Analisador de Trabalho: Eficiência",
            "description": "Análise da eficiência de campo e tempos de máquina.",
            "durationSeconds": 780,
            "bunnyVideoId": ""
          },
          {
            "id": 46,
            "name": "Analisador de Trabalho: Otimização",
            "description": "Identificando gargalos na operação de campo.",
            "durationSeconds": 840,
            "bunnyVideoId": ""
          },
          {
            "id": 47,
            "name": "Análise Premium: Ferramentas",
            "description": "Exploração dos recursos avançados de análise.",
            "durationSeconds": 900,
            "bunnyVideoId": ""
          },
          {
            "id": 48,
            "name": "Análise Premium: Dashboards",
            "description": "Configuração de painéis personalizados.",
            "durationSeconds": 800,
            "bunnyVideoId": ""
          },
          {
            "id": 49,
            "name": "Relatórios de Máquinas: Saúde",
            "description": "Monitoramento de telemetria e falhas.",
            "durationSeconds": 720,
            "bunnyVideoId": ""
          },
          {
            "id": 50,
            "name": "Relatórios de Máquinas: Combustível",
            "description": "Monitoramento de consumo e eficiência de frota.",
            "durationSeconds": 660,
            "bunnyVideoId": ""
          },
          {
            "id": 51,
            "name": "Relatórios de Máquinas: Cargas",
            "description": "Gestão de horas de motor e utilização.",
            "durationSeconds": 750,
            "bunnyVideoId": ""
          },
          {
            "id": 52,
            "name": "Relatórios de Máquinas: Exportação",
            "description": "Preparando relatórios para gestão de frota.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          },
          {
            "id": 53,
            "name": "Visão Geral do Trabalho: Resumo",
            "description": "Dashboard consolidado do dia a dia operacional.",
            "durationSeconds": 720,
            "bunnyVideoId": ""
          },
          {
            "id": 54,
            "name": "Visão Geral do Trabalho: Ações",
            "description": "Tomada de decisão baseada na visão geral.",
            "durationSeconds": 840,
            "bunnyVideoId": ""
          }
        ]
      },
      {
        "id": 9,
        "name": "Módulo 9: Gestão de Manutenção",
        "description": "Não deixe que a manutenção corretiva paralise sua operação. Aprenda a gerir preventivamente toda a sua frota.",
        "descriptionSub": "Prevenção e controle para garantir disponibilidade mecânica.",
        "imgCoverLink": "",
        "bunnyCollectionId": "",
        "lessons": [
          {
            "id": 55,
            "name": "Introdução aos Planos",
            "description": "A importância da manutenção preventiva no OpCenter.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          },
          {
            "id": 56,
            "name": "Criando Plano",
            "description": "Configurando cronogramas de revisão.",
            "durationSeconds": 900,
            "bunnyVideoId": ""
          },
          {
            "id": 57,
            "name": "Editando Plano",
            "description": "Ajustando planos baseados em horas de uso real.",
            "durationSeconds": 720,
            "bunnyVideoId": ""
          },
          {
            "id": 58,
            "name": "Programando Manutenções",
            "description": "Agendamento e priorização de serviços.",
            "durationSeconds": 840,
            "bunnyVideoId": ""
          },
          {
            "id": 59,
            "name": "Execução",
            "description": "Registrando as manutenções realizadas.",
            "durationSeconds": 750,
            "bunnyVideoId": ""
          },
          {
            "id": 60,
            "name": "Histórico",
            "description": "Consultando o histórico de manutenções da frota.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          },
          {
            "id": 61,
            "name": "Indicadores",
            "description": "KPIs de confiabilidade de equipamentos.",
            "durationSeconds": 800,
            "bunnyVideoId": ""
          },
          {
            "id": 62,
            "name": "Boas práticas",
            "description": "Estratégias para evitar paradas inesperadas.",
            "durationSeconds": 700,
            "bunnyVideoId": ""
          }
        ]
      },
      {
        "id": 10,
        "name": "Módulo 10: Centro de Controle Operacional",
        "description": "A central de comando do gestor. Aprenda a visualizar toda a sua operação em tempo real com o mapa gerencial e alertas inteligentes.",
        "descriptionSub": "Visibilidade total e monitoramento em tempo real.",
        "imgCoverLink": "",
        "bunnyCollectionId": "",
        "lessons": [
          {
            "id": 63,
            "name": "Mapa Gerencial",
            "description": "Visualizando a operação completa no mapa.",
            "durationSeconds": 960,
            "bunnyVideoId": ""
          },
          {
            "id": 64,
            "name": "Gerenciamento de Equipamentos",
            "description": "Monitoramento de localização e status ao vivo.",
            "durationSeconds": 840,
            "bunnyVideoId": ""
          },
          {
            "id": 65,
            "name": "Alertas",
            "description": "Configurando alertas críticos de operação.",
            "durationSeconds": 720,
            "bunnyVideoId": ""
          },
          {
            "id": 66,
            "name": "Indicadores Operacionais",
            "description": "Painéis de controle executivo em tempo real.",
            "durationSeconds": 900,
            "bunnyVideoId": ""
          }
        ]
      },
      {
        "id": 11,
        "name": "Módulo 11: Recursos Extras",
        "description": "Funcionalidades complementares que elevam o nível de atenção e proatividade do gestor agrícola.",
        "descriptionSub": "Notificações inteligentes para uma operação proativa.",
        "imgCoverLink": "",
        "bunnyCollectionId": "",
        "lessons": [
          {
            "id": 67,
            "name": "Sistema de Notificações",
            "description": "Configurando canais e tipos de aviso.",
            "durationSeconds": 600,
            "bunnyVideoId": ""
          },
          {
            "id": 68,
            "name": "Alertas Inteligentes",
            "description": "Automatizando a detecção de problemas operacionais.",
            "durationSeconds": 800,
            "bunnyVideoId": ""
          }
        ]
      }
    ],
    "domains": [
      {
        "id": 1,
        "title": "Configuração de Organização",
        "description": "Gerencie acessos, permissões e parceiros de forma segura."
      },
      {
        "id": 2,
        "title": "Análise de Dados de Campo",
        "description": "Interprete mapas de produtividade e velocidade com precisão."
      },
      {
        "id": 3,
        "title": "Gestão de Manutenção",
        "description": "Antecipe falhas e gerencie alertas remotos de frota."
      },
      {
        "id": 4,
        "title": "Gestão de Fazendas",
        "description": "Gerencie os limites, talhões e áreas de suas fazendas."
      }
    ],
    "teachers": [
      {
        "id": 1,
        "name": "Joeder Blanca",
        "role": "",
        "position": "Analista de Sistemas & Especialista John Deere",
        "avatar": "https://joederblanca.com.br/assets/img/profile/profile-square-2.png",
        "bio": "Com 4 anos de experiência direta no ecossistema John Deere, Joeder é reconhecido por desenvolver integrações mundiais via APIs e transformar dados brutos em lucro para produtores. Sua metodologia foca 100% na prática, removendo as barreiras técnicas entre você e o sistema.",
        "instagramLink": "https://www.instagram.com/joe_blanca/",
        "linkedinLink": "www.linkedin.com/in/joeder-blanca-032577201",
        "idAgivys": "2"
      }
    ],
    "categories": [
      {
        "id": 1,
        "name": "Agricultura de Precisão"
      }
    ]
  },
  {
    "id": 2,
    "name": "Curso Teste 1",
    "description": "Um curso completo para testar todas as funcionalidades da plataforma. Um curso completo para testar todas as funcionalidades da plataforma. Um curso completo para testar todas as funcionalidades da plataforma. Um curso completo para testar todas as funcionalidades da plataforma.",
    "descriptionSub": "Teste de descrição do curso",
    "level": "Avançado",
    "priceSingle": 5.99,
    "imgCoverLink": "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSQPDtnja7GEMD2BAEEpyE7KyJ1KP1AvNEOhu0SMamzi_7DdBrhzm_LKvR4&s=10",
    "bunnyLibraryId": "701167",
    "modules": [
      {
        "id": 12,
        "name": "Módulo 1: Fundamentos",
        "description": "string",
        "descriptionSub": "string",
        "imgCoverLink": "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQZHv9olVi55O8nlzHuI5xI8Djimn05QeHLCG0ldzmz88nuWhknvFUPCNtZ&s=10",
        "bunnyCollectionId": "2260b7cc-34f8-4a27-90a6-4e6b502b9449",
        "lessons": [
          {
            "id": 69,
            "name": "Aula 1: Introdução ao C#",
            "description": "Descrição da aula para teste de descrição das aulas testando",
            "durationSeconds": 600,
            "bunnyVideoId": "c8bc1c6c-5bcb-47b7-8c4f-a0dacf28ab7c"
          },
          {
            "id": 70,
            "name": "Aula 2: Introdução ao C#",
            "description": "string",
            "durationSeconds": 600,
            "bunnyVideoId": "video_12345"
          }
        ]
      }
    ],
    "domains": [],
    "teachers": [],
    "categories": []
  }
];
