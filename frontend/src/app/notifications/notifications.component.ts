import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subscription } from 'rxjs';
import { NotificationService, Notification } from '../core/notification.service';
import { TravelRequestApprovalDialogComponent } from '../travel-requests/travel-request-approval-dialog/travel-request-approval-dialog.component';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { TravelRequestService } from '../travel-requests/travel-request.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../shared/breadcrumb/breadcrumb.component';
import { BreadcrumbService } from '../shared/breadcrumb/breadcrumb.service';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    MatChipsModule,
    MatCardModule,
    MatTabsModule,
    MatSnackBarModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    BreadcrumbComponent
  ],
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss']
})
export class NotificationsComponent implements OnInit, OnDestroy {
  allNotifications: Notification[] = [];
  unreadNotifications: Notification[] = [];
  readNotifications: Notification[] = [];
  loading = false;
  selectedTabIndex = 0;
  breadcrumbItems: BreadcrumbItem[] = [];
  private subscriptions: Subscription[] = [];

  constructor(
    private notificationService: NotificationService,
    private travelRequestService: TravelRequestService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private breadcrumbService: BreadcrumbService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.initializeBreadcrumb();
    this.loadAllNotifications();
    
    // Subscrever às mudanças em tempo real
    this.subscriptions.push(
      this.notificationService.notifications$.subscribe(notifications => {
        this.updateNotificationLists(notifications);
      })
    );
  }

  private initializeBreadcrumb(): void {
    this.breadcrumbService.setNotificationsBreadcrumb();
    this.breadcrumbItems = this.breadcrumbService.getBreadcrumbs();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  loadAllNotifications(): void {
    this.loading = true;
    this.notificationService.getNotifications(true).subscribe({
      next: (notifications) => {
        this.updateNotificationLists(notifications);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
        this.snackBar.open('Erro ao carregar notificações', 'Fechar', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  updateNotificationLists(notifications: Notification[]): void {
    this.allNotifications = notifications;
    this.unreadNotifications = notifications.filter(n => !n.isRead);
    this.readNotifications = notifications.filter(n => n.isRead);
  }

  onNotificationClick(notification: Notification): void {
    if (!notification.isRead) {
      this.notificationService.markNotificationAsRead(notification.id);
    }

    // Navegar para a entidade relacionada
    if (notification.relatedEntityType === 'TravelRequest' && notification.relatedEntityId) {
      this.openTravelRequestApprovalDialog(notification.relatedEntityId);
    }
  }

  openTravelRequestApprovalDialog(travelRequestId: string): void {
    this.travelRequestService.getById(travelRequestId).subscribe({
      next: (travelRequest) => {
        const dialogRef = this.dialog.open(TravelRequestApprovalDialogComponent, {
          data: { travelRequest },
          width: '600px',
          disableClose: false
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result) {
            this.snackBar.open('Requisição de viagem processada com sucesso!', 'Fechar', { duration: 3000 });
            this.loadAllNotifications(); // Recarregar notificações
          }
        });
      },
      error: (error) => {
        console.error('Error loading travel request:', error);
        this.snackBar.open('Erro ao carregar requisição de viagem', 'Fechar', { duration: 3000 });
      }
    });
  }

  onMarkAllAsRead(): void {
    this.notificationService.markAllNotificationsAsRead();
    this.snackBar.open('Todas as notificações foram marcadas como lidas', 'Fechar', { duration: 2000 });
  }

  onMarkAsRead(notification: Notification): void {
    this.notificationService.markNotificationAsRead(notification.id);
  }

  getNotificationIcon(type: string): string {
    switch (type) {
      case 'info':
        return 'info';
      case 'success':
        return 'check_circle';
      case 'warning':
        return 'warning';
      case 'error':
        return 'error';
      default:
        return 'notifications';
    }
  }

  getNotificationColor(type: string): string {
    switch (type) {
      case 'info':
        return 'primary';
      case 'success':
        return 'accent';
      case 'warning':
        return 'warn';
      case 'error':
        return 'warn';
      default:
        return 'primary';
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));
    
    if (diffInMinutes < 1) {
      return 'Agora';
    } else if (diffInMinutes < 60) {
      return `${diffInMinutes} min atrás`;
    } else if (diffInMinutes < 1440) {
      const hours = Math.floor(diffInMinutes / 60);
      return `${hours}h atrás`;
    } else {
      const days = Math.floor(diffInMinutes / 1440);
      return `${days}d atrás`;
    }
  }

  trackByNotificationId(index: number, notification: Notification): string {
    return notification.id;
  }

  getHighlightedMessage(notification: Notification): SafeHtml {
    if (!notification.message) return '';
    // Regex para encontrar Origem: <cidade>, Destino: <cidade>
    const regex = /Origem: ([^,]+), Destino: ([^\n]+)/;
    const match = notification.message.match(regex);
    if (match) {
      const origem = match[1].trim();
      const destino = match[2].trim();
      // Substitui por span com classe highlight e emoji de avião
      const highlighted = notification.message.replace(
        regex,
        `Origem: <span class='highlight'>${origem}</span> ✈️ Destino: <span class='highlight'>${destino}</span>`
      );
      return this.sanitizer.bypassSecurityTrustHtml(highlighted);
    }
    return notification.message;
  }

  getMainMessage(notification: Notification): string {
    // Remove a parte de origem/destino da mensagem
    return notification.message?.replace(/Origem: ([^,]+), Destino: ([^\n]+)/, '').trim() || '';
  }

  getRouteInfo(notification: Notification): { origem: string, destino: string } | null {
    if (!notification.message) return null;
    const regex = /Origem: ([^,]+), Destino: ([^\n]+)/;
    const match = notification.message.match(regex);
    if (match) {
      return { origem: match[1].trim(), destino: match[2].trim() };
    }
    return null;
  }
} 