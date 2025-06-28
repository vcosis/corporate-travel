import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subscription } from 'rxjs';
import { NotificationService, Notification } from '../../core/notification.service';
import { TravelRequestApprovalDialogComponent } from '../../travel-requests/travel-request-approval-dialog/travel-request-approval-dialog.component';
import { TravelRequestService } from '../../travel-requests/travel-request.service';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatBadgeModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
    MatChipsModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './notification-bell.component.html',
  styleUrls: ['./notification-bell.component.scss']
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  notifications: Notification[] = [];
  unreadCount: number = 0;
  private subscriptions: Subscription[] = [];

  constructor(
    private notificationService: NotificationService,
    private travelRequestService: TravelRequestService,
    private dialog: MatDialog,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    console.log('=== NotificationBellComponent.ngOnInit ===');
    
    // Carregar notificações iniciais
    this.notificationService.loadNotifications();

    // Subscrever às mudanças
    this.subscriptions.push(
      this.notificationService.notifications$.subscribe(notifications => {
        console.log('Notifications updated in component:', notifications);
        this.notifications = notifications;
      }),
      this.notificationService.unreadCount$.subscribe(count => {
        console.log('Unread count updated in component:', count);
        this.unreadCount = count;
      })
    );
    
    console.log('=== End NotificationBellComponent.ngOnInit ===');
  }

  ngOnDestroy(): void {
    console.log('=== NotificationBellComponent.ngOnDestroy ===');
    console.log('Unsubscribing from', this.subscriptions.length, 'subscriptions');
    this.subscriptions.forEach(sub => sub.unsubscribe());
    console.log('=== End NotificationBellComponent.ngOnDestroy ===');
  }

  onNotificationClick(notification: Notification): void {
    console.log('=== onNotificationClick ===');
    console.log('Notification clicked:', notification);
    
    if (!notification.isRead) {
      console.log('Marking notification as read');
      this.notificationService.markNotificationAsRead(notification.id);
    }
    
    // Navegar para a entidade relacionada
    if (notification.relatedEntityType === 'TravelRequest' && notification.relatedEntityId) {
      this.openTravelRequestApprovalDialog(notification.relatedEntityId);
    }
    
    console.log('=== End onNotificationClick ===');
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
            this.notificationService.loadNotifications(); // Recarregar notificações
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
    console.log('=== onMarkAllAsRead ===');
    console.log('Marking all notifications as read');
    this.notificationService.markAllNotificationsAsRead();
    console.log('=== End onMarkAllAsRead ===');
  }

  onViewAllNotifications(): void {
    console.log('=== onViewAllNotifications ===');
    this.router.navigate(['/notifications']);
    console.log('=== End onViewAllNotifications ===');
  }

  getNotificationIcon(type: string): string {
    console.log('=== getNotificationIcon ===');
    console.log('Type:', type);
    
    let icon: string;
    switch (type) {
      case 'info':
        icon = 'info';
        break;
      case 'success':
        icon = 'check_circle';
        break;
      case 'warning':
        icon = 'warning';
        break;
      case 'error':
        icon = 'error';
        break;
      default:
        icon = 'notifications';
        break;
    }
    
    console.log('Icon:', icon);
    console.log('=== End getNotificationIcon ===');
    return icon;
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
    console.log('=== formatDate ===');
    console.log('Date string:', dateString);
    
    const date = new Date(dateString);
    const now = new Date();
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));
    
    let result: string;
    if (diffInMinutes < 1) {
      result = 'Agora';
    } else if (diffInMinutes < 60) {
      result = `${diffInMinutes} min atrás`;
    } else if (diffInMinutes < 1440) {
      const hours = Math.floor(diffInMinutes / 60);
      result = `${hours}h atrás`;
    } else {
      const days = Math.floor(diffInMinutes / 1440);
      result = `${days}d atrás`;
    }
    
    console.log('Formatted date:', result);
    console.log('=== End formatDate ===');
    return result;
  }

  trackByNotificationId(index: number, notification: Notification): string {
    console.log('=== trackByNotificationId ===');
    console.log('Index:', index, 'Notification ID:', notification.id);
    console.log('=== End trackByNotificationId ===');
    return notification.id;
  }

  getMainMessage(notification: Notification): string {
    // Se a notificação tem uma mensagem específica, use-a
    if (notification.message) {
      return notification.message;
    }
    
    // Se tem um título específico, use-o como fallback
    if (notification.title) {
      return notification.title;
    }
    
    // Fallback para mensagens baseadas no tipo
    switch (notification.type) {
      case 'success':
        return 'Sua requisição de viagem foi aprovada';
      case 'warning':
        return 'Sua requisição de viagem foi rejeitada';
      case 'info':
        return 'Nova notificação';
      case 'error':
        return 'Erro na requisição de viagem';
      default:
        return 'Nova notificação';
    }
  }

  getRouteInfo(notification: Notification): { origem: string; destino: string } | null {
    if (notification.relatedEntityType === 'TravelRequest' && notification.relatedEntityId) {
      // Aqui você pode implementar a lógica para buscar os dados da rota
      // Por enquanto, retornamos null para não quebrar o template
      return null;
    }
    return null;
  }
} 