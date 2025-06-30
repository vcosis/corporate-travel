import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService } from '../user.service';
import { PasswordRequirementsService } from '../../core/password-requirements.service';
import { PasswordRequirementsComponent } from '../../shared/password-requirements/password-requirements.component';

@Component({
  selector: 'app-create-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    ReactiveFormsModule,
    PasswordRequirementsComponent
  ],
  template: `
    <h2 mat-dialog-title>Novo Usuário</h2>
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <mat-dialog-content>
        <mat-form-field appearance="outline" style="width: 100%; margin-bottom: 16px;">
          <mat-label>Nome</mat-label>
          <input matInput formControlName="name" required>
          <mat-error *ngIf="form.get('name')?.hasError('required')">
            Nome é obrigatório
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" style="width: 100%; margin-bottom: 16px;">
          <mat-label>Email</mat-label>
          <input matInput formControlName="email" type="email" required>
          <mat-error *ngIf="form.get('email')?.hasError('required')">
            Email é obrigatório
          </mat-error>
          <mat-error *ngIf="form.get('email')?.hasError('email')">
            Email inválido
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" style="width: 100%; margin-bottom: 16px;">
          <mat-label>Senha</mat-label>
          <input matInput formControlName="password" type="password" required>
          <mat-error *ngIf="form.get('password')?.hasError('required')">
            Senha é obrigatória
          </mat-error>
          <mat-error *ngIf="form.get('password')?.hasError('minlength')">
            Senha deve ter pelo menos 8 caracteres
          </mat-error>
        </mat-form-field>

        <!-- Mostrar requisitos de senha quando necessário -->
        <app-password-requirements 
          [password]="form.get('password')?.value" 
          [showRequirements]="showPasswordRequirements">
        </app-password-requirements>

        <mat-form-field appearance="outline" style="width: 100%;">
          <mat-label>Perfil</mat-label>
          <mat-select formControlName="role" required>
            <mat-option value="User">User</mat-option>
            <mat-option value="Manager">Manager</mat-option>
            <mat-option value="Admin">Admin</mat-option>
          </mat-select>
          <mat-error *ngIf="form.get('role')?.hasError('required')">
            Perfil é obrigatório
          </mat-error>
        </mat-form-field>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button type="button" (click)="onCancel()">Cancelar</button>
        <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || loading">
          {{ loading ? 'Criando...' : 'Criar' }}
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    mat-dialog-content {
      min-width: 400px;
    }
  `]
})
export class CreateUserDialogComponent {
  form: FormGroup;
  loading = false;
  showPasswordRequirements = false;

  constructor(
    private dialogRef: MatDialogRef<CreateUserDialogComponent>,
    private fb: FormBuilder,
    private userService: UserService,
    private passwordService: PasswordRequirementsService
  ) {
    this.form = this.fb.group({
      name: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      role: ['User', [Validators.required]]
    });

    // Monitorar mudanças na senha para validação
    this.form.get('password')?.valueChanges.subscribe(password => {
      if (password) {
        const validation = this.passwordService.validatePassword(password);
        this.showPasswordRequirements = !validation.isValid && password.length > 0;
      } else {
        this.showPasswordRequirements = false;
      }
    });
  }

  onSubmit() {
    if (this.form.valid) {
      const password = this.form.value.password;
      const validation = this.passwordService.validatePassword(password);
      
      if (!validation.isValid) {
        // Mostrar erro de validação
        return;
      }

      this.loading = true;
      const userData = this.form.value;

      this.userService.registerUser(userData).subscribe({
        next: () => {
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Erro ao criar usuário:', error);
          this.loading = false;
        }
      });
    }
  }

  onCancel() {
    this.dialogRef.close(false);
  }
} 