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
  loadingApprove = false;
  loadingReject = false;
  canApprove = false;

  constructor(
    public dialogRef: MatDialogRef<TravelRequestApprovalDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TravelRequestApprovalDialogData,
    private travelRequestService: TravelRequestService,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {
    this.travelRequest = data.travelRequest;
  }

  ngOnInit(): void {
    this.checkPermissions();
  }

  private checkPermissions(): void {
    // Apenas Admin e Manager podem aprovar/rejeitar
    const hasManagerRole = this.authService.hasRole('Admin') || this.authService.hasRole('Manager');
    
    // Verificar se o usuário atual não é o mesmo que criou a solicitação
    const currentUserId = this.authService.getCurrentUserId();
    const isOwnRequest = currentUserId === this.travelRequest.requestingUserId;
    
    // Só pode aprovar se tem role de manager/admin E não é a própria solicitação
    this.canApprove = hasManagerRole && !isOwnRequest;
  }

  approve(): void {
    this.loadingApprove = true;
    this.travelRequestService.approve(this.travelRequest.id).subscribe({
      next: () => {
        setTimeout(() => {
          this.dialogRef.close('approved');
          this.loadingApprove = false;
        }, 900);
      },
      error: () => {
        this.snackBar.open('Erro ao aprovar solicitação.', 'Fechar', { duration: 3000 });
        this.loadingApprove = false;
      }
    });
  }

  reject(): void {
    this.loadingReject = true;
    this.travelRequestService.reject(this.travelRequest.id).subscribe({
      next: () => {
        setTimeout(() => {
          this.dialogRef.close('rejected');
          this.loadingReject = false;
        }, 900);
      },
      error: () => {
        this.snackBar.open('Erro ao reprovar solicitação.', 'Fechar', { duration: 3000 });
        this.loadingReject = false;
      }
    });
  }

  close(): void {
    this.dialogRef.close();
  }

  getStatusColor(status: any): string {
    if (status === null || status === undefined) return 'default';
    
    if (typeof status === 'object' && status !== null && 'name' in status) {
      return this.getStatusColor(status.name);
    }
    
    const value = String(status).toLowerCase();
    switch (value) {
      case '0':
      case 'pending':
        return 'warn';
      case '1':
      case 'approved':
        return 'accent';
      case '2':
      case 'rejected':
        return 'warn';
      default:
        return 'default';
    }
  }

  getStatusIcon(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'approved':
      case 'aprovado':
        return 'check_circle';
      case 'rejected':
      case 'rejeitado':
        return 'cancel';
      case 'pending':
      case 'pendente':
      default:
        return 'hourglass_empty';
    }
  }

  getStatusDescription(status: any): string {
    if (status === null || status === undefined) return '-';
    if (typeof status === 'object' && status !== null && 'name' in status) {
      return this.getStatusDescription(status.name);
    }
    const value = String(status).toLowerCase();
    switch (value) {
      case '0':
      case 'pending':
      case 'pendente':
        return 'Pendente';
      case '1':
      case 'approved':
      case 'aprovado':
        return 'Aprovado';
      case '2':
      case 'rejected':
      case 'rejeitado':
        return 'Reprovado';
      default:
        return value;
    }
  }
} 