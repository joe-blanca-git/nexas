import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface IPost {
  title: string;
  excerpt: string;
  image: string;
  category: string;
  date: string;
}

@Component({
  selector: 'app-blog-external',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './blog-external.component.html',
  styleUrl: './blog-external.component.scss'
})
export class BlogExternalComponent {
  isLoadingPage = false;

  featuredPost!: IPost;

  mediumPosts!: IPost[]

  smallPosts: IPost[] = [
    {
      title: 'Eletrificação de Frotas',
      excerpt: 'O impacto econômico e ecológico dos novos motores elétricos.',
      image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
      category: 'Sustentabilidade',
      date: 'Há 4 dias'
    },
    {
      title: 'Machine Learning',
      excerpt: 'Modelos estatísticos preditivos aplicados ao solo brasileiro.',
      image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
      category: 'Ciência de Dados',
      date: 'Há 5 dias'
    },
    {
      title: 'Drones e Mapeamento',
      excerpt: 'Imagens multiespectrais capturadas com precisão centimétrica.',
      image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
      category: 'Drones',
      date: 'Há 1 semana'
    },
    {
      title: 'Segurança Cibernética',
      excerpt: 'Protegendo as fazendas conectadas de ameaças digitais.',
      image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
      category: 'Cybersecurity',
      date: 'Há 1 semana'
    }
  ];

  ngOnInit() {
    this.loadData();
  }

  async loadData() {
    this.isLoadingPage = true;
    try {
      this.featuredPost = {
        title: 'O Futuro da Inteligência Artificial na Agricultura de Precisão',
        excerpt: 'Descubra como algoritmos de Machine Learning e Visão Computacional estão revolucionando o monitoramento de safras, reduzindo desperdícios e prevendo colheitas com precisão inédita no setor agrícola.',
        image: 'https://s2.glbimg.com/Deg8YEkSphxP1LqSUr0QBH_O82c=/780x440/e.glbimg.com/og/ed/f/original/2022/04/20/r4f167447_rrd_1x.jpg',
        category: 'Tecnologia & Campo',
        date: 'Há 3 minutos'
      }

      this.mediumPosts = [
        {
          title: 'Tratores Autônomos: Revolução Silenciosa',
          excerpt: 'Saiba como as máquinas autônomas operam sem intervenção humana, otimizando rotas e reduzindo emissões.',
          image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
          category: 'Automação',
          date: 'Ontem'
        },
        {
          title: 'Sensores IoT e o Futuro das Plantações',
          excerpt: 'Uma análise de como pequenos sensores medem umidade, acidez e nutrientes em tempo real diretamente para a nuvem.',
          image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
          category: 'IoT',
          date: 'Há 2 dias'
        }
      ];

      this.smallPosts = [
        {
          title: 'Eletrificação de Frotas',
          excerpt: 'O impacto econômico e ecológico dos novos motores elétricos.',
          image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
          category: 'Sustentabilidade',
          date: 'Há 4 dias'
        },
        {
          title: 'Machine Learning',
          excerpt: 'Modelos estatísticos preditivos aplicados ao solo brasileiro.',
          image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
          category: 'Ciência de Dados',
          date: 'Há 5 dias'
        },
        {
          title: 'Drones e Mapeamento',
          excerpt: 'Imagens multiespectrais capturadas com precisão centimétrica.',
          image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
          category: 'Drones',
          date: 'Há 1 semana'
        },
        {
          title: 'Segurança Cibernética',
          excerpt: 'Protegendo as fazendas conectadas de ameaças digitais.',
          image: 'https://d2yghbees9788u.cloudfront.net/futurecom/2023/03/Tratores-Autnomos-Saiba-como-funciona-essa-tecnologia.jpg',
          category: 'Cybersecurity',
          date: 'Há 1 semana'
        }
      ];

    } catch (error) {

    } finally {
        this.isLoadingPage = false;
    }
  }
}
