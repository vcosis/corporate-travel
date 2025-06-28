import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import * as signalR from '@microsoft/signalr';

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  requesterName?: string;
  requesterAvatarUrl?: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private hubConnection?: signalR.HubConnection;
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  private unreadCountSubject = new BehaviorSubject<number>(0);
  
  public notifications$ = this.notificationsSubject.asObservable();
  public unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient) {}

  public startConnection(token: string): Promise<void> {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.signalRHubUrl}?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    this.setupSignalRHandlers();

    return this.hubConnection.start()
      .then(() => {
        // Connection started successfully
      })
      .catch(error => {
        throw error;
      });
  }

  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  private setupSignalRHandlers(): void {
    if (!this.hubConnection) {
      return;
    }

    // Receber nova notificação
    this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
      // Verificar se a notificação já existe na lista
      const currentNotifications = this.notificationsSubject.value;
      const existingNotification = currentNotifications.find(n => n.id === notification.id);
      
      if (existingNotification) {
        return;
      }
      
      // Adicionar à lista de notificações
      this.notificationsSubject.next([notification, ...currentNotifications]);
      
      // Incrementar contador de não lidas
      const currentCount = this.unreadCountSubject.value;
      this.unreadCountSubject.next(currentCount + 1);
      
      // Mostrar toast/notificação visual
      this.showNotificationToast(notification);
    });

    // Atualizar contador de não lidas
    this.hubConnection.on('UpdateUnreadCount', (count: number) => {
      this.unreadCountSubject.next(count);
    });

    // Log de conexão estabelecida
    this.hubConnection.onreconnected((connectionId) => {
      // Reconnected
    });

    // Log de desconexão
    this.hubConnection.onclose((error) => {
      // Connection closed
    });
  }

  private showNotificationToast(notification: Notification): void {
    // Verificar se já existe um toast para esta notificação
    const existingToast = document.querySelector(`[data-notification-id="${notification.id}"]`);
    if (existingToast) {
      return;
    }
    
    // Criar elemento de toast
    const toast = document.createElement('div');
    toast.className = 'notification-toast';
    toast.setAttribute('data-notification-id', notification.id);
    toast.innerHTML = `
      <div class="notification-toast-content">
        <h4>${notification.title}</h4>
        <p>${notification.message}</p>
      </div>
    `;
    
    // Adicionar ao DOM
    document.body.appendChild(toast);
    
    // Remover após 5 segundos
    setTimeout(() => {
      if (toast.parentNode) {
        toast.parentNode.removeChild(toast);
      }
    }, 5000);
  }

  // API Methods
  public getNotifications(includeRead: boolean = false): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${environment.apiUrl}/notifications?includeRead=${includeRead}`);
  }

  public getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${environment.apiUrl}/notifications/unread-count`);
  }

  public markAsRead(notificationId: string): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/notifications/${notificationId}/mark-as-read`, {});
  }

  public markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/notifications/mark-all-as-read`, {});
  }

  // Load initial data
  public loadNotifications(): void {
    this.getNotifications().subscribe({
      next: (notifications) => {
        this.notificationsSubject.next(notifications);
      },
      error: (error) => {
        // Error loading notifications
      }
    });

    this.getUnreadCount().subscribe({
      next: (response) => {
        this.unreadCountSubject.next(response.count);
      },
      error: (error) => {
        // Error loading unread count
      }
    });
  }

  // Manual methods for updating state
  public markNotificationAsRead(notificationId: string): void {
    this.markAsRead(notificationId).subscribe({
      next: () => {
        const notifications = this.notificationsSubject.value.map(n => 
          n.id === notificationId ? { ...n, isRead: true, readAt: new Date().toISOString() } : n
        );
        this.notificationsSubject.next(notifications);
        
        // Recalcular contador
        const unreadCount = notifications.filter(n => !n.isRead).length;
        this.unreadCountSubject.next(unreadCount);
      },
      error: (error) => {
        // Error marking notification as read
      }
    });
  }

  public markAllNotificationsAsRead(): void {
    this.markAllAsRead().subscribe({
      next: () => {
        const notifications = this.notificationsSubject.value.map(n => ({
          ...n,
          isRead: true,
          readAt: new Date().toISOString()
        }));
        this.notificationsSubject.next(notifications);
        this.unreadCountSubject.next(0);
      },
      error: (error) => {
        // Error marking all notifications as read
      }
    });
  }

  public markNotificationAsUnread(notificationId: string): void {
    const notifications = this.notificationsSubject.value.map(n => 
      n.id === notificationId ? { ...n, isRead: false, readAt: undefined } : n
    );
    this.notificationsSubject.next(notifications);
    
    // Recalcular contador
    const unreadCount = notifications.filter(n => !n.isRead).length;
    this.unreadCountSubject.next(unreadCount);
  }
} 