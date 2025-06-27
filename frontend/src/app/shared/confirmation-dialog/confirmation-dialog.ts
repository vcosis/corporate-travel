import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmationDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: 'primary' | 'accent' | 'warn';
  icon?: string;
  type?: 'info' | 'warning' | 'error' | 'success';
}

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="confirmation-dialog">
      <div class="dialog-header" [class]="data.type || 'warning'">
        <mat-icon class="header-icon" *ngIf="getIcon()">
          {{ getIcon() }}
        </mat-icon>
        <h2 mat-dialog-title class="dialog-title">
          {{ data.title }}
        </h2>
      </div>
      
      <mat-dialog-content class="dialog-content">
        <p class="dialog-message">{{ data.message }}</p>
      </mat-dialog-content>
      
      <mat-dialog-actions align="end" class="dialog-actions">
        <button 
          mat-button 
          mat-dialog-close 
          class="cancel-button"
          cdkFocusInitial>
          {{ data.cancelText || 'Cancel' }}
        </button>
        <button 
          mat-raised-button 
          [color]="data.confirmColor || 'warn'"
          [mat-dialog-close]="true"
          class="confirm-button">
          <mat-icon *ngIf="data.type === 'error' || data.confirmColor === 'warn'">delete</mat-icon>
          {{ data.confirmText || 'Confirm' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .confirmation-dialog {
      min-width: 400px;
      max-width: 500px;
    }
    
    .dialog-header {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px 24px 0 24px;
      margin-bottom: 8px;
    }
    
    .dialog-header.warning {
      color: var(--pending-bg);
    }
    
    .dialog-header.error {
      color: var(--warn-color);
    }
    
    .dialog-header.info {
      color: var(--primary-color);
    }
    
    .dialog-header.success {
      color: var(--success-color);
    }
    
    .header-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
    }
    
    .dialog-title {
      margin: 0;
      font-size: 20px;
      font-weight: 500;
      line-height: 1.2;
      color: var(--text-primary);
    }
    
    .dialog-content {
      padding: 0 24px 16px 24px;
    }
    
    .dialog-message {
      margin: 0;
      font-size: 14px;
      line-height: 1.5;
      color: var(--text-primary);
    }
    
    .dialog-actions {
      padding: 8px 24px 24px 24px;
      gap: 8px;
    }
    
    .cancel-button {
      min-width: 80px;
      outline: none !important;
      border: none !important;
      color: var(--text-secondary);
    }
    
    .cancel-button:focus {
      outline: none !important;
      border: none !important;
      box-shadow: none !important;
    }
    
    .confirm-button {
      min-width: 100px;
      outline: none !important;
      border: none !important;
    }
    
    .confirm-button:focus {
      outline: none !important;
      border: none !important;
      box-shadow: none !important;
    }
    
    .confirm-button mat-icon {
      margin-right: 4px;
      font-size: 18px;
      width: 18px;
      height: 18px;
    }
    
    // Remove bordas azuis de todos os botões Material no dialog
    :host ::ng-deep .mat-mdc-button,
    :host ::ng-deep .mat-mdc-raised-button {
      outline: none !important;
      border: none !important;
    }
    
    :host ::ng-deep .mat-mdc-button:focus,
    :host ::ng-deep .mat-mdc-raised-button:focus {
      outline: none !important;
      border: none !important;
      box-shadow: none !important;
    }
    
    @media (max-width: 600px) {
      .confirmation-dialog {
        min-width: 300px;
        max-width: 90vw;
      }
      
      .dialog-header {
        padding: 12px 16px 0 16px;
      }
      
      .dialog-content {
        padding: 0 16px 12px 16px;
      }
      
      .dialog-actions {
        padding: 8px 16px 16px 16px;
      }
    }

    // Remove borda azul do container do dialog
    :host ::ng-deep .mat-mdc-dialog-container,
    :host ::ng-deep .mat-mdc-dialog-container:focus,
    :host ::ng-deep .mat-mdc-dialog-container.cdk-focused {
      outline: none !important;
      box-shadow: none !important;
    }
  `]
})
export class ConfirmationDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmationDialogData
  ) {}

  getIcon(): string | null {
    if (this.data.icon) {
      return this.data.icon;
    }
    
    switch (this.data.type) {
      case 'warning':
        return 'warning';
      case 'error':
        return 'error';
      case 'info':
        return 'info';
      case 'success':
        return 'check_circle';
      default:
        return 'warning';
    }
  }
} 