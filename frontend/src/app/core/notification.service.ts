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
    console.log('=== NotificationService.startConnection ===');
    console.log('Token exists:', !!token);
    console.log('API URL:', environment.apiUrl);
    
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.signalRHubUrl}?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    console.log('Hub connection created');
    this.setupSignalRHandlers();

    return this.hubConnection.start()
      .then(() => {
        console.log('SignalR connection started successfully');
        console.log('Connection state:', this.hubConnection?.state);
      })
      .catch(error => {
        console.error('Error starting SignalR connection:', error);
        throw error;
      });
  }

  public stopConnection(): void {
    console.log('=== stopConnection ===');
    if (this.hubConnection) {
      console.log('Stopping SignalR connection...');
      this.hubConnection.stop();
      console.log('SignalR connection stopped');
    } else {
      console.log('No SignalR connection to stop');
    }
    console.log('=== End stopConnection ===');
  }

  private setupSignalRHandlers(): void {
    if (!this.hubConnection) {
      console.log('No hub connection available for setting up handlers');
      return;
    }

    console.log('Setting up SignalR handlers');

    // Receber nova notificação
    this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
      console.log('=== SignalR: ReceiveNotification ===');
      console.log('Nova notificação recebida:', notification);
      console.log('Notification ID:', notification.id);
      console.log('Notification Title:', notification.title);
      console.log('Notification Message:', notification.message);
      
      // Verificar se a notificação já existe na lista
      const currentNotifications = this.notificationsSubject.value;
      const existingNotification = currentNotifications.find(n => n.id === notification.id);
      
      if (existingNotification) {
        console.log('Notification already exists, skipping duplicate');
        console.log('=== End SignalR: ReceiveNotification (duplicate) ===');
        return;
      }
      
      // Adicionar à lista de notificações
      this.notificationsSubject.next([notification, ...currentNotifications]);
      
      // Incrementar contador de não lidas
      const currentCount = this.unreadCountSubject.value;
      this.unreadCountSubject.next(currentCount + 1);
      
      // Mostrar toast/notificação visual
      this.showNotificationToast(notification);
      console.log('=== End SignalR: ReceiveNotification ===');
    });

    // Atualizar contador de não lidas
    this.hubConnection.on('UpdateUnreadCount', (count: number) => {
      console.log('=== SignalR: UpdateUnreadCount ===');
      console.log('Novo contador de não lidas:', count);
      this.unreadCountSubject.next(count);
      console.log('=== End SignalR: UpdateUnreadCount ===');
    });

    // Log de conexão estabelecida
    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR reconnected with connection ID:', connectionId);
    });

    // Log de desconexão
    this.hubConnection.onclose((error) => {
      console.log('SignalR connection closed:', error);
    });

    console.log('SignalR handlers setup completed');
  }

  private showNotificationToast(notification: Notification): void {
    console.log('=== showNotificationToast ===');
    console.log('Creating toast for notification:', notification);
    console.log('Notification ID:', notification.id);
    console.log('Notification Title:', notification.title);
    
    // Verificar se já existe um toast para esta notificação
    const existingToast = document.querySelector(`[data-notification-id="${notification.id}"]`);
    if (existingToast) {
      console.log('Toast already exists for this notification, skipping');
      console.log('=== End showNotificationToast (duplicate) ===');
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
    
    console.log('Toast element created:', toast);
    
    // Adicionar ao DOM
    document.body.appendChild(toast);
    console.log('Toast added to DOM');
    
    // Remover após 5 segundos
    setTimeout(() => {
      if (toast.parentNode) {
        toast.parentNode.removeChild(toast);
        console.log('Toast removed from DOM');
      }
    }, 5000);
    
    console.log('=== End showNotificationToast ===');
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
    console.log('=== loadNotifications ===');
    
    this.getNotifications().subscribe({
      next: (notifications) => {
        console.log('Notifications loaded from API:', notifications);
        this.notificationsSubject.next(notifications);
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
      }
    });

    this.getUnreadCount().subscribe({
      next: (response) => {
        console.log('Unread count loaded from API:', response.count);
        this.unreadCountSubject.next(response.count);
      },
      error: (error) => {
        console.error('Error loading unread count:', error);
      }
    });
    
    console.log('=== End loadNotifications ===');
  }

  // Manual methods for updating state
  public markNotificationAsRead(notificationId: string): void {
    console.log('=== markNotificationAsRead ===');
    console.log('Marking notification as read:', notificationId);
    
    this.markAsRead(notificationId).subscribe({
      next: () => {
        console.log('Notification marked as read successfully');
        const notifications = this.notificationsSubject.value.map(n => 
          n.id === notificationId ? { ...n, isRead: true, readAt: new Date().toISOString() } : n
        );
        this.notificationsSubject.next(notifications);
        
        // Recalcular contador
        const unreadCount = notifications.filter(n => !n.isRead).length;
        this.unreadCountSubject.next(unreadCount);
        console.log('Updated unread count:', unreadCount);
      },
      error: (error) => {
        console.error('Error marking notification as read:', error);
      }
    });
    
    console.log('=== End markNotificationAsRead ===');
  }

  public markAllNotificationsAsRead(): void {
    console.log('=== markAllNotificationsAsRead ===');
    
    this.markAllAsRead().subscribe({
      next: () => {
        console.log('All notifications marked as read successfully');
        const notifications = this.notificationsSubject.value.map(n => ({
          ...n,
          isRead: true,
          readAt: new Date().toISOString()
        }));
        this.notificationsSubject.next(notifications);
        this.unreadCountSubject.next(0);
        console.log('Updated all notifications and set unread count to 0');
      },
      error: (error) => {
        console.error('Error marking all notifications as read:', error);
      }
    });
    
    console.log('=== End markAllNotificationsAsRead ===');
  }
} 