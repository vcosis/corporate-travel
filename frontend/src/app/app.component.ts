import { Component, OnInit } from '@angular/core';
import { AuthService } from './auth/auth.service';
import { NotificationService } from './core/notification.service';
import { ThemeService } from './core/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private themeService: ThemeService
  ) {}

  ngOnInit(): void {
    // Subscribe to current user changes
    this.authService.currentUser$.subscribe(user => {
      const token = this.authService.getToken();
      
      if (user && token) {
        this.notificationService.startConnection(token);
      } else {
        this.notificationService.stopConnection();
      }
    });

    // Initialize theme
    this.themeService.initializeTheme();
  }
}
