import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { BreadcrumbItem } from './breadcrumb.component';

export type { BreadcrumbItem } from './breadcrumb.component';

@Injectable({
  providedIn: 'root'
})
export class BreadcrumbService {
  private breadcrumbSubject = new BehaviorSubject<BreadcrumbItem[]>([]);
  public breadcrumbs$ = this.breadcrumbSubject.asObservable();

  setBreadcrumbs(items: BreadcrumbItem[]): void {
    this.breadcrumbSubject.next(items);
  }

  getBreadcrumbs(): BreadcrumbItem[] {
    return this.breadcrumbSubject.value;
  }

  // Métodos utilitários para breadcrumbs comuns
  setDashboardBreadcrumb(): void {
    this.setBreadcrumbs([
      { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' }
    ]);
  }

  setUsersBreadcrumb(): void {
    this.setBreadcrumbs([
      { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
      { label: 'Gerenciamento de Usuários', icon: 'people' }
    ]);
  }

  setTravelRequestsBreadcrumb(): void {
    this.setBreadcrumbs([
      { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
      { label: 'Requisições de Viagem', icon: 'card_travel' }
    ]);
  }

  setNotificationsBreadcrumb(): void {
    this.setBreadcrumbs([
      { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
      { label: 'Notificações', icon: 'notifications' }
    ]);
  }

  setProfileBreadcrumb(): void {
    this.setBreadcrumbs([
      { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
      { label: 'Meu Perfil', icon: 'person' }
    ]);
  }
} 