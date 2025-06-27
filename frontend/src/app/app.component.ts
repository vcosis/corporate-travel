import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './auth/auth.service';
import { NotificationService } from './core/notification.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'corporate-travel';

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    console.log('=== AppComponent.ngOnInit ===');
    
    // Inicializar conexão SignalR se o usuário estiver autenticado
    this.authService.currentUser$.subscribe(user => {
      console.log('Current user changed:', user);
      
      if (user) {
        const token = this.authService.getToken();
        console.log('Token exists:', !!token);
        
        if (token) {
          console.log('Starting SignalR connection...');
          this.notificationService.startConnection(token)
            .then(() => {
              console.log('Conexão SignalR estabelecida com sucesso');
            })
            .catch(error => {
              console.error('Erro ao conectar SignalR:', error);
            });
        }
      } else {
        console.log('User not authenticated, stopping SignalR connection');
        this.notificationService.stopConnection();
      }
    });
    
    console.log('=== End AppComponent.ngOnInit ===');
  }
}
