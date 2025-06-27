import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Observable } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { NotificationBellComponent } from '../shared/notification-bell/notification-bell.component';
import { UserMenuComponent } from '../shared/user-menu/user-menu.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    NotificationBellComponent,
    UserMenuComponent
  ],
  templateUrl: './layout.html',
  styleUrls: ['./layout.scss']
})
export class LayoutComponent implements OnInit {
  user$: Observable<any>;
  isHandset$: Observable<boolean> = new Observable();
  collapsed = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.user$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    // Initialize handset detection if needed
  }

  hasRole(role: string): boolean {
    const user = this.authService.getCurrentUser();
    return user?.roles?.includes(role) || false;
  }

  toggleSidebar(): void {
    this.collapsed = !this.collapsed;
  }
}
