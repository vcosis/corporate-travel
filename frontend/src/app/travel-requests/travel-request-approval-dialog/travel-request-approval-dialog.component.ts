import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TravelRequestService, TravelRequest } from '../travel-request.service';
import { AuthService } from '../../auth/auth.service';

export interface TravelRequestApprovalDialogData {
  travelRequest: TravelRequest;
}

@Component({
  selector: 'app-travel-request-approval-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './travel-request-approval-dialog.component.html',
  styleUrls: ['./travel-request-approval-dialog.component.scss']
})
export class TravelRequestApprovalDialogComponent implements OnInit {
  travelRequest: TravelRequest;
  loading = false;
  currentUser: any;
  canApprove = false;

  constructor(
    public dialogRef: MatDialogRef<TravelRequestApprovalDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TravelRequestApprovalDialogData,
    private travelRequestService: TravelRequestService,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {
    this.travelRequest = data.travelRequest;
    this.currentUser = this.authService.getCurrentUser();
  }

  ngOnInit(): void {
    // Verificar se o usuário atual pode aprovar (gerente ou admin)
    this.canApprove = this.currentUser?.roles?.some((role: string) => 
      ['Manager', 'Admin'].includes(role)
    ) || false;
  }

  approve(): void {
    this.loading = true;
    this.travelRequestService.approve(this.travelRequest.id).subscribe({
      next: () => {
        this.snackBar.open('Requisição de viagem aprovada com sucesso!', 'Fechar', { duration: 3000 });
        this.dialogRef.close(true);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error approving travel request:', error);
        this.snackBar.open('Erro ao aprovar requisição de viagem', 'Fechar', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  reject(): void {
    this.loading = true;
    this.travelRequestService.reject(this.travelRequest.id).subscribe({
      next: () => {
        this.snackBar.open('Requisição de viagem rejeitada', 'Fechar', { duration: 3000 });
        this.dialogRef.close(true);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error rejecting travel request:', error);
        this.snackBar.open('Erro ao rejeitar requisição de viagem', 'Fechar', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  close(): void {
    this.dialogRef.close();
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'warn';
      case 'approved':
        return 'accent';
      case 'rejected':
        return 'warn';
      default:
        return 'primary';
    }
  }

  getStatusIcon(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending':
        return 'schedule';
      case 'approved':
        return 'check_circle';
      case 'rejected':
        return 'cancel';
      default:
        return 'help';
    }
  }

  formatDate(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  }

  formatDateTime(dateString: string): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getDurationDays(): number {
    const startDate = new Date(this.travelRequest.startDate);
    const endDate = new Date(this.travelRequest.endDate);
    const diffTime = Math.abs(endDate.getTime() - startDate.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  }
} 