import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../auth/auth.service';
import { ThemeService } from '../../core/theme.service';
import { UserAvatarComponent } from '../user-avatar/user-avatar.component';

@Component({
  selector: 'app-user-menu',
  standalone: true,
  imports: [
    CommonModule,
    MatMenuModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    MatSlideToggleModule,
    FormsModule,
    UserAvatarComponent
  ],
  template: `
    <button 
      mat-icon-button 
      [matMenuTriggerFor]="userMenu" 
      class="user-menu-trigger"
      [attr.aria-label]="'Menu do usuário ' + (user?.name || '')">
      <app-user-avatar 
        [name]="user?.name || ''" 
        [email]="user?.email || ''" 
        size="medium">
      </app-user-avatar>
    </button>

    <mat-menu #userMenu="matMenu" class="user-menu-panel">
      <div class="user-menu-header">
        <app-user-avatar 
          [name]="user?.name || ''" 
          [email]="user?.email || ''" 
          size="large">
        </app-user-avatar>
        <div class="user-info">
          <div class="user-name">{{ user?.name || 'Usuário' }}</div>
          <div class="user-email">{{ user?.email || '' }}</div>
          <div class="user-roles" *ngIf="user?.roles?.length">
            {{ user.roles.join(', ') }}
          </div>
        </div>
      </div>

      <mat-divider></mat-divider>

      <button mat-menu-item (click)="goToProfile()">
        <mat-icon>person</mat-icon>
        <span>Perfil</span>
      </button>

      <mat-divider></mat-divider>

      <div class="theme-toggle-container">
        <div class="theme-toggle-label">
          <mat-icon>dark_mode</mat-icon>
          <span>Modo Escuro</span>
        </div>
        <mat-slide-toggle 
          [checked]="isDarkMode" 
          (change)="toggleTheme()"
          color="primary">
        </mat-slide-toggle>
      </div>

      <mat-divider></mat-divider>

      <button mat-menu-item (click)="logout()" class="logout-button">
        <mat-icon>logout</mat-icon>
        <span>Sair</span>
      </button>
    </mat-menu>
  `,
  styleUrls: ['./user-menu.component.scss']
})
export class UserMenuComponent {
  @Input() user: any;

  constructor(
    private authService: AuthService,
    private themeService: ThemeService,
    private router: Router
  ) {}

  get isDarkMode(): boolean {
    return this.themeService.getCurrentTheme() === 'dark';
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  goToProfile(): void {
    this.router.navigate(['/profile']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
} 