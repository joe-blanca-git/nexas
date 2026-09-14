import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Breadcrumb } from './breadcrumb.component';

@Injectable({
  providedIn: 'root'
})
export class BreadcrumbService {
  private dynamicBreadcrumbsSource = new BehaviorSubject<Breadcrumb[] | null>(null);
  dynamicBreadcrumbs$ = this.dynamicBreadcrumbsSource.asObservable();

  setBreadcrumbs(breadcrumbs: Breadcrumb[] | null) {
    this.dynamicBreadcrumbsSource.next(breadcrumbs);
  }
}
