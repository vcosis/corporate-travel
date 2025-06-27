import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE } from '@angular/material/core';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TravelRequestService } from '../travel-request.service';

// Configuração de formato de data brasileiro
export const MY_DATE_FORMATS = {
  parse: {
    dateInput: 'DD/MM/YYYY',
  },
  display: {
    dateInput: 'DD/MM/YYYY',
    monthYearLabel: 'MMM YYYY',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM YYYY',
  },
};

@Component({
  selector: 'app-create-travel-request-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    ReactiveFormsModule
  ],
  providers: [
    { provide: MAT_DATE_LOCALE, useValue: 'pt-BR' },
    { provide: MAT_DATE_FORMATS, useValue: MY_DATE_FORMATS }
  ],
  template: `
    <h2 mat-dialog-title>Nova Requisição de Viagem</h2>
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <mat-dialog-content>
        <mat-form-field appearance="outline" style="width: 100%; margin-bottom: 16px;">
          <mat-label>Origem</mat-label>
          <input matInput formControlName="origin" required>
          <mat-error *ngIf="form.get('origin')?.hasError('required')">
            Origem é obrigatória
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" style="width: 100%; margin-bottom: 16px;">
          <mat-label>Destino</mat-label>
          <input matInput formControlName="destination" required>
          <mat-error *ngIf="form.get('destination')?.hasError('required')">
            Destino é obrigatório
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" style="width: 100%; margin-bottom: 16px;">
          <mat-label>Data de Início</mat-label>
          <input matInput [matDatepicker]="startPicker" formControlName="startDate" required placeholder="dd/mm/aaaa">
          <mat-datepicker-toggle matSuffix [for]="startPicker"></mat-datepicker-toggle>
          <mat-datepicker #startPicker></mat-datepicker>
          <mat-error *ngIf="form.get('startDate')?.hasError('required')">
            Data de início é obrigatória
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" style="width: 100%; margin-bottom: 16px;">
          <mat-label>Data de Fim</mat-label>
          <input matInput [matDatepicker]="endPicker" formControlName="endDate" required placeholder="dd/mm/aaaa">
          <mat-datepicker-toggle matSuffix [for]="endPicker"></mat-datepicker-toggle>
          <mat-datepicker #endPicker></mat-datepicker>
          <mat-error *ngIf="form.get('endDate')?.hasError('required')">
            Data de fim é obrigatória
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" style="width: 100%;">
          <mat-label>Motivo</mat-label>
          <textarea matInput formControlName="reason" rows="3" required></textarea>
          <mat-error *ngIf="form.get('reason')?.hasError('required')">
            Motivo é obrigatório
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
      min-width: 500px;
    }
  `]
})
export class CreateTravelRequestDialogComponent {
  form: FormGroup;
  loading = false;

  constructor(
    private dialogRef: MatDialogRef<CreateTravelRequestDialogComponent>,
    private fb: FormBuilder,
    private travelRequestService: TravelRequestService
  ) {
    this.form = this.fb.group({
      origin: ['', [Validators.required]],
      destination: ['', [Validators.required]],
      startDate: ['', [Validators.required]],
      endDate: ['', [Validators.required]],
      reason: ['', [Validators.required]]
    });
  }

  onSubmit() {
    if (this.form.valid) {
      this.loading = true;
      const formValue = this.form.value;
      
      // Formatar as datas para o formato ISO string
      const travelRequestData = {
        ...formValue,
        startDate: formValue.startDate ? new Date(formValue.startDate).toISOString() : null,
        endDate: formValue.endDate ? new Date(formValue.endDate).toISOString() : null
      };

      console.log('Dados da requisição:', travelRequestData);

      this.travelRequestService.create(travelRequestData).subscribe({
        next: () => {
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Erro ao criar requisição de viagem:', error);
          this.loading = false;
        }
      });
    }
  }

  onCancel() {
    this.dialogRef.close(false);
  }
} 