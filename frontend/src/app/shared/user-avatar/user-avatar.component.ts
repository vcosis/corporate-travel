import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-user-avatar',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="user-avatar" [class]="size" [style.background-color]="avatarBackgroundColor">
      <ng-container *ngIf="initials; else defaultIcon">
        <span class="initials">{{ initials }}</span>
      </ng-container>
      <ng-template #defaultIcon>
        <mat-icon>person</mat-icon>
      </ng-template>
    </div>
  `,
  styleUrls: ['./user-avatar.component.scss']
})
export class UserAvatarComponent implements OnInit {
  @Input() name: string = '';
  @Input() email: string = '';
  @Input() size: 'small' | 'medium' | 'large' = 'medium';
  @Input() backgroundColor?: string;

  initials: string = '';

  ngOnInit(): void {
    this.generateInitials();
  }

  private generateInitials(): void {
    if (this.name) {
      const nameParts = this.name.trim().split(' ');
      if (nameParts.length >= 2) {
        this.initials = (nameParts[0][0] + nameParts[nameParts.length - 1][0]).toUpperCase();
      } else {
        this.initials = this.name[0].toUpperCase();
      }
    } else if (this.email) {
      this.initials = this.email[0].toUpperCase();
    }
  }

  get avatarBackgroundColor(): string {
    // Sempre azul
    return '#1976d2';
  }
} 