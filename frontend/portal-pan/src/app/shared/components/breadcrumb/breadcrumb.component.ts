import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd, ActivatedRoute, Event as RouterEvent } from '@angular/router';
import { filter, Subscription } from 'rxjs';

export interface Breadcrumb {
  label: string;
  url: string;
}

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './breadcrumb.component.html',
  styleUrl: './breadcrumb.component.scss'
})
export class BreadcrumbComponent implements OnInit, OnDestroy {
  public breadcrumbs: Breadcrumb[] = [];
  private routerSubscription?: Subscription;

  constructor(private router: Router, private activatedRoute: ActivatedRoute) {}

  ngOnInit() {
    // Inicializa o breadcrumb com a rota atual
    this.breadcrumbs = this.buildBreadcrumb(this.activatedRoute.root);

    // Ouve os eventos de roteamento
    this.routerSubscription = this.router.events
      .pipe(filter((event: RouterEvent) => event instanceof NavigationEnd))
      .subscribe(() => {
        this.breadcrumbs = this.buildBreadcrumb(this.activatedRoute.root);
      });
  }

  ngOnDestroy() {
    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
  }

  private buildBreadcrumb(route: ActivatedRoute, url: string = '', breadcrumbs: Breadcrumb[] = []): Breadcrumb[] {
    // Pega os segmentos de caminho desta rota
    let path = route.routeConfig && route.routeConfig.path ? route.routeConfig.path : '';
    
    // Resolve caminhos dinâmicos (ex: cart/:id => cart/1)
    const lastPathIndex = breadcrumbs.length;
    let nextUrl = url;
    
    if (path) {
      // Cria a nova URL baseado no path
      let resolvedPath = path;
      // Substitui parâmetros dinâmicos se existirem
      if (route.snapshot) {
        Object.keys(route.snapshot.params).forEach(key => {
          resolvedPath = resolvedPath.replace(`:${key}`, route.snapshot.params[key]);
        });
      }
      
      nextUrl = `${url}/${resolvedPath}`;
      
      // Remove double slashes se existirem
      nextUrl = nextUrl.replace('//', '/');
    }

    // Tenta pegar o título nativo ou o data.title
    const label = route.routeConfig ? (route.routeConfig.title as string || (route.routeConfig.data ? route.routeConfig.data['title'] : '')) : '';

    // Apenas adiciona se tiver label e não for repetido (mesma label da URL anterior)
    if (label && (!breadcrumbs.length || breadcrumbs[breadcrumbs.length - 1].label !== label)) {
      breadcrumbs.push({
        label: label,
        url: nextUrl
      });
    }

    // Se tem filhas, navega recursivamente
    if (route.firstChild) {
      return this.buildBreadcrumb(route.firstChild, nextUrl, breadcrumbs);
    }
    
    return breadcrumbs;
  }
}
