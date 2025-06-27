import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserService, User } from '../user.service';

export interface EditUserDialogData {
  user: User;
}

@Component({
  selector: 'app-edit-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    ReactiveFormsModule
  ],
  template: `
    <h2 mat-dialog-title>Editar Usuário</h2>
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
          {{ loading ? 'Salvando...' : 'Salvar' }}
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
export class EditUserDialogComponent {
  form: FormGroup;
  loading = false;

  constructor(
    private dialogRef: MatDialogRef<EditUserDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EditUserDialogData,
    private fb: FormBuilder,
    private userService: UserService
  ) {
    this.form = this.fb.group({
      id: [data.user.id],
      name: [data.user.name, [Validators.required]],
      email: [data.user.email, [Validators.required, Validators.email]],
      role: [data.user.roles[0] || 'User', [Validators.required]]
    });
  }

  onSubmit() {
    if (this.form.valid) {
      this.loading = true;
      const userData = this.form.value;

      this.userService.updateUser(userData).subscribe({
        next: () => {
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Erro ao atualizar usuário:', error);
          this.loading = false;
        }
      });
    }
  }

  onCancel() {
    this.dialogRef.close(false);
  }
} 