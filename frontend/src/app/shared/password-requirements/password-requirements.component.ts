import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { PasswordRequirementsService } from '../../core/password-requirements.service';

@Component({
  selector: 'app-password-requirements',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="password-requirements" *ngIf="showRequirements">
      <div class="requirements-title">
        <mat-icon>info</mat-icon>
        <span>Requisitos da senha:</span>
      </div>
      
      <div class="requirements-list">
        <div class="requirement-item" [class.valid]="isValid('length')" [class.invalid]="!isValid('length')">
          <mat-icon>{{ isValid('length') ? 'check_circle' : 'error' }}</mat-icon>
          <span>Pelo menos 8 caracteres</span>
        </div>
        
        <div class="requirement-item" [class.valid]="isValid('digit')" [class.invalid]="!isValid('digit')">
          <mat-icon>{{ isValid('digit') ? 'check_circle' : 'error' }}</mat-icon>
          <span>Pelo menos um dígito</span>
        </div>
        
        <div class="requirement-item" [class.valid]="isValid('lowercase')" [class.invalid]="!isValid('lowercase')">
          <mat-icon>{{ isValid('lowercase') ? 'check_circle' : 'error' }}</mat-icon>
          <span>Pelo menos uma letra minúscula</span>
        </div>
        
        <div class="requirement-item" [class.valid]="isValid('uppercase')" [class.invalid]="!isValid('uppercase')">
          <mat-icon>{{ isValid('uppercase') ? 'check_circle' : 'error' }}</mat-icon>
          <span>Pelo menos uma letra maiúscula</span>
        </div>
        
        <div class="requirement-item" [class.valid]="isValid('special')" [class.invalid]="!isValid('special')">
          <mat-icon>{{ isValid('special') ? 'check_circle' : 'error' }}</mat-icon>
          <span>Pelo menos um caractere especial</span>
        </div>
      </div>

      <div class="password-strength" *ngIf="password">
        <div class="strength-bar">
          <div class="strength-fill" [class]="strength.strength" [style.width.%]="strength.percentage"></div>
        </div>
        <span class="strength-text">{{ getStrengthText() }}</span>
      </div>
    </div>
  `,
  styles: [`
    .password-requirements {
      margin-top: 8px;
      padding: 12px;
      background-color: #f5f5f5;
      border-radius: 4px;
      border-left: 4px solid #2196f3;
    }

    .requirements-title {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 8px;
      font-weight: 500;
      color: #333;
    }

    .requirements-title mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
      color: #2196f3;
    }

    .requirements-list {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .requirement-item {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 12px;
      transition: color 0.2s ease;
    }

    .requirement-item mat-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
    }

    .requirement-item.valid {
      color: #4caf50;
    }

    .requirement-item.invalid {
      color: #f44336;
    }

    .password-strength {
      margin-top: 12px;
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .strength-bar {
      flex: 1;
      height: 4px;
      background-color: #e0e0e0;
      border-radius: 2px;
      overflow: hidden;
    }

    .strength-fill {
      height: 100%;
      transition: width 0.3s ease, background-color 0.3s ease;
    }

    .strength-fill.weak {
      background-color: #f44336;
    }

    .strength-fill.medium {
      background-color: #ff9800;
    }

    .strength-fill.strong {
      background-color: #4caf50;
    }

    .strength-text {
      font-size: 12px;
      font-weight: 500;
      min-width: 60px;
    }

    .strength-text.weak {
      color: #f44336;
    }

    .strength-text.medium {
      color: #ff9800;
    }

    .strength-text.strong {
      color: #4caf50;
    }
  `]
})
export class PasswordRequirementsComponent implements OnInit {
  @Input() password: string = '';
  @Input() showRequirements: boolean = true;

  strength: { strength: 'weak' | 'medium' | 'strong'; percentage: number } = { strength: 'weak', percentage: 0 };

  constructor(private passwordService: PasswordRequirementsService) {}

  ngOnInit() {}

  ngOnChanges() {
    if (this.password) {
      this.strength = this.passwordService.getPasswordStrength(this.password);
    }
  }

  isValid(requirement: string): boolean {
    if (!this.password) return false;

    switch (requirement) {
      case 'length':
        return this.password.length >= 8;
      case 'digit':
        return /\d/.test(this.password);
      case 'lowercase':
        return /[a-z]/.test(this.password);
      case 'uppercase':
        return /[A-Z]/.test(this.password);
      case 'special':
        return /[^a-zA-Z0-9]/.test(this.password);
      default:
        return false;
    }
  }

  getStrengthText(): string {
    switch (this.strength.strength) {
      case 'weak':
        return 'Fraca';
      case 'medium':
        return 'Média';
      case 'strong':
        return 'Forte';
      default:
        return '';
    }
  }
} 