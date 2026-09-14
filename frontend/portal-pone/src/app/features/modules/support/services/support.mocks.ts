export interface SupportTicket {
  id: number;
  title: string;
  subject: string;
  userName: string;
  userEmail: string;
  createdAt: string;
  status: 'Pendente' | 'Respondido' | 'Cancelado' | 'Finalizado';
  messages: TicketMessage[];
}

export interface TicketMessage {
  id: number;
  sender: 'User' | 'Support';
  senderName: string;
  content: string;
  createdAt: string;
}

export interface ForumTopic {
  id: number;
  title: string;
  subject?: string;
  authorName: string;
  categoryName?: string;
  createdAt: string;
  repliesCount: number;
  status: 'Open' | 'Resolved';
  messages: ForumMessage[];
}

export interface ForumMessage {
  id: number;
  authorName: string;
  content: string;
  createdAt: string;
}

export interface PurchasedCourse {
  id: number;
  name: string;
  purchaseDate: string;
  price: number;
  paymentMethod: 'Cartão de Crédito' | 'Pix' | 'Boleto';
  paymentStatus: 'Pago' | 'Pendente' | 'Cancelado' | 'Reembolsado';
  accessStatus: 'Liberado' | 'Bloqueado';
}

export interface UserAccess {
  id: number;
  name: string;
  email: string;
  createdAt: string;
  courses: PurchasedCourse[];
}

export const MOCK_TICKETS: SupportTicket[] = [
  {
    id: 1001,
    title: 'Problema de acesso ao curso de lógica',
    subject: 'Acesso',
    userName: 'João da Silva',
    userEmail: 'joao.silva@email.com',
    createdAt: new Date(Date.now() - 86400000).toISOString(),
    status: 'Pendente',
    messages: [
      {
        id: 1,
        sender: 'User',
        senderName: 'João da Silva',
        content: 'Olá! Comprei o curso ontem e ainda não estou conseguindo acessar o primeiro módulo. Podem me ajudar?',
        createdAt: new Date(Date.now() - 86400000).toISOString()
      }
    ]
  },
  {
    id: 1002,
    title: 'Dúvida sobre o certificado',
    subject: 'Certificação',
    userName: 'Maria Aparecida',
    userEmail: 'maria@email.com',
    createdAt: new Date(Date.now() - 172800000).toISOString(),
    status: 'Respondido',
    messages: [
      {
        id: 1,
        sender: 'User',
        senderName: 'Maria Aparecida',
        content: 'Gostaria de saber se o certificado é reconhecido pelo MEC.',
        createdAt: new Date(Date.now() - 172800000).toISOString()
      },
      {
        id: 2,
        sender: 'Support',
        senderName: 'Suporte',
        content: 'Olá Maria, nossos cursos são livres, portanto não exigem reconhecimento do MEC, mas possuem validade nacional para comprovação de horas extra curriculares.',
        createdAt: new Date(Date.now() - 150000000).toISOString()
      }
    ]
  },
  {
    id: 1003,
    title: 'Erro na emissão da nota fiscal',
    subject: 'Financeiro',
    userName: 'Carlos Eduardo',
    userEmail: 'carlos.edu@email.com',
    createdAt: new Date(Date.now() - 300000000).toISOString(),
    status: 'Finalizado',
    messages: [
      {
        id: 1,
        sender: 'User',
        senderName: 'Carlos Eduardo',
        content: 'Minha nota fiscal foi gerada com o CPF errado.',
        createdAt: new Date(Date.now() - 300000000).toISOString()
      },
      {
        id: 2,
        sender: 'Support',
        senderName: 'Suporte Financeiro',
        content: 'Olá Carlos, já realizamos a correção. A nova nota fiscal foi enviada para o seu email.',
        createdAt: new Date(Date.now() - 250000000).toISOString()
      }
    ]
  },
  {
    id: 1004,
    title: 'Dúvida sobre cancelamento',
    subject: 'Assinatura',
    userName: 'Fernanda Lima',
    userEmail: 'fernanda.lima@email.com',
    createdAt: new Date(Date.now() - 400000000).toISOString(),
    status: 'Cancelado',
    messages: [
      {
        id: 1,
        sender: 'User',
        senderName: 'Fernanda Lima',
        content: 'Gostaria de saber como funciona a política de reembolso em até 7 dias.',
        createdAt: new Date(Date.now() - 400000000).toISOString()
      }
    ]
  },
  {
    id: 1005,
    title: 'Não consigo recuperar minha senha',
    subject: 'Acesso',
    userName: 'Ricardo Mendes',
    userEmail: 'ricardo.mendes@email.com',
    createdAt: new Date(Date.now() - 500000000).toISOString(),
    status: 'Pendente',
    messages: [
      {
        id: 1,
        sender: 'User',
        senderName: 'Ricardo Mendes',
        content: 'O email de redefinição de senha não está chegando na minha caixa de entrada.',
        createdAt: new Date(Date.now() - 500000000).toISOString()
      }
    ]
  },
  {
    id: 1006,
    title: 'Material complementar indisponível',
    subject: 'Conteúdo',
    userName: 'Juliana Costa',
    userEmail: 'juliana.costa@email.com',
    createdAt: new Date(Date.now() - 600000000).toISOString(),
    status: 'Respondido',
    messages: [
      {
        id: 1,
        sender: 'User',
        senderName: 'Juliana Costa',
        content: 'O link do PDF da aula 5 está quebrado.',
        createdAt: new Date(Date.now() - 600000000).toISOString()
      },
      {
        id: 2,
        sender: 'Support',
        senderName: 'Equipe de Conteúdo',
        content: 'Olá Juliana, obrigado pelo aviso. O link já foi corrigido!',
        createdAt: new Date(Date.now() - 550000000).toISOString()
      }
    ]
  },
  {
    id: 1007,
    title: 'Cupom de desconto inválido',
    subject: 'Financeiro',
    userName: 'Roberto Almeida',
    userEmail: 'roberto.almeida@email.com',
    createdAt: new Date(Date.now() - 700000000).toISOString(),
    status: 'Finalizado',
    messages: [
      {
        id: 1,
        sender: 'User',
        senderName: 'Roberto Almeida',
        content: 'Tentei usar o cupom BLACKFRIDAY mas diz que está expirado.',
        createdAt: new Date(Date.now() - 700000000).toISOString()
      },
      {
        id: 2,
        sender: 'Support',
        senderName: 'Suporte de Vendas',
        content: 'Olá Roberto. Infelizmente a promoção encerrou na semana passada.',
        createdAt: new Date(Date.now() - 650000000).toISOString()
      }
    ]
  },
  {
    id: 1008,
    title: 'Alteração de nome no certificado',
    subject: 'Certificação',
    userName: 'Patrícia Gomes',
    userEmail: 'patricia.gomes@email.com',
    createdAt: new Date(Date.now() - 800000000).toISOString(),
    status: 'Respondido',
    messages: [
      {
        id: 1,
        sender: 'User',
        senderName: 'Patrícia Gomes',
        content: 'Gostaria de adicionar meu nome do meio no certificado do curso concluído hoje.',
        createdAt: new Date(Date.now() - 800000000).toISOString()
      },
      {
        id: 2,
        sender: 'Support',
        senderName: 'Suporte',
        content: 'Olá Patrícia. Atualize seu perfil nas configurações e em seguida clique em "Gerar novamente" na aba de certificados.',
        createdAt: new Date(Date.now() - 750000000).toISOString()
      }
    ]
  },
  {
    id: 1009,
    title: 'Aplicativo fechando sozinho',
    subject: 'Técnico',
    userName: 'Vitor Hugo',
    userEmail: 'vitor.hugo@email.com',
    createdAt: new Date(Date.now() - 900000000).toISOString(),
    status: 'Pendente',
    messages: [
      {
        id: 1,
        sender: 'User',
        senderName: 'Vitor Hugo',
        content: 'Toda vez que abro uma aula no app do celular ele fecha do nada.',
        createdAt: new Date(Date.now() - 900000000).toISOString()
      }
    ]
  },
  {
    id: 1010,
    title: 'Sugestão de novo curso',
    subject: 'Sugestões',
    userName: 'Camila Rocha',
    userEmail: 'camila.rocha@email.com',
    createdAt: new Date(Date.now() - 1000000000).toISOString(),
    status: 'Finalizado',
    messages: [
      {
        id: 1,
        sender: 'User',
        senderName: 'Camila Rocha',
        content: 'Adoraria um curso sobre Figma e UI/UX.',
        createdAt: new Date(Date.now() - 1000000000).toISOString()
      },
      {
        id: 2,
        sender: 'Support',
        senderName: 'Equipe de Conteúdo',
        content: 'Obrigado pela sugestão, Camila! Já estamos gravando um curso sobre Figma que será lançado mês que vem.',
        createdAt: new Date(Date.now() - 950000000).toISOString()
      }
    ]
  }
];

export const MOCK_TOPICS: ForumTopic[] = [];

export const MOCK_USERS_ACCESS: UserAccess[] = [
  {
    id: 3001,
    name: 'Amanda Fernandes',
    email: 'amanda.fernandes@email.com',
    createdAt: new Date(Date.now() - 5000000000).toISOString(),
    courses: [
      {
        id: 4001,
        name: 'React Avançado',
        purchaseDate: new Date(Date.now() - 100000000).toISOString(),
        price: 299.90,
        paymentMethod: 'Cartão de Crédito',
        paymentStatus: 'Cancelado',
        accessStatus: 'Bloqueado'
      },
      {
        id: 4002,
        name: 'Node.js para Iniciantes',
        purchaseDate: new Date(Date.now() - 4000000000).toISOString(),
        price: 199.90,
        paymentMethod: 'Pix',
        paymentStatus: 'Pago',
        accessStatus: 'Liberado'
      }
    ]
  },
  {
    id: 3002,
    name: 'Bruno Lima',
    email: 'bruno.lima@email.com',
    createdAt: new Date(Date.now() - 8000000000).toISOString(),
    courses: [
      {
        id: 4003,
        name: 'ASP.NET Core',
        purchaseDate: new Date(Date.now() - 150000000).toISOString(),
        price: 349.90,
        paymentMethod: 'Boleto',
        paymentStatus: 'Pendente',
        accessStatus: 'Bloqueado'
      }
    ]
  },
  {
    id: 3003,
    name: 'Carolina Santos',
    email: 'carolina.santos@email.com',
    createdAt: new Date(Date.now() - 12000000000).toISOString(),
    courses: [
      {
        id: 4004,
        name: 'Design Patterns',
        purchaseDate: new Date(Date.now() - 10000000000).toISOString(),
        price: 149.90,
        paymentMethod: 'Pix',
        paymentStatus: 'Reembolsado',
        accessStatus: 'Bloqueado'
      },
      {
        id: 4005,
        name: 'Arquitetura Limpa',
        purchaseDate: new Date(Date.now() - 11000000000).toISOString(),
        price: 199.90,
        paymentMethod: 'Cartão de Crédito',
        paymentStatus: 'Pago',
        accessStatus: 'Liberado'
      }
    ]
  },
  {
    id: 3004,
    name: 'Daniel Oliveira',
    email: 'daniel.oliveira@email.com',
    createdAt: new Date(Date.now() - 200000000).toISOString(),
    courses: [
      {
        id: 4006,
        name: 'Lógica de Programação',
        purchaseDate: new Date(Date.now() - 150000000).toISOString(),
        price: 99.90,
        paymentMethod: 'Cartão de Crédito',
        paymentStatus: 'Pendente',
        accessStatus: 'Bloqueado'
      }
    ]
  }
];
