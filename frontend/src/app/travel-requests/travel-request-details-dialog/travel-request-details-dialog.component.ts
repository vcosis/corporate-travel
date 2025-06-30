import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule, MatCardHeader, MatCardTitle, MatCardContent } from '@angular/material/card';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TravelRequestService, TravelRequest } from '../travel-request.service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-travel-request-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule,
    MatCardModule,
    MatCardHeader,
    MatCardTitle,
    MatCardContent,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './travel-request-details-dialog.component.html',
  styleUrls: ['./travel-request-details-dialog.component.scss']
})
export class TravelRequestDetailsDialogComponent {
  travelRequest: TravelRequest;
  canApprove = false;
  loading = false;
  loadingApprove = false;
  loadingReject = false;

  constructor(
    public dialogRef: MatDialogRef<TravelRequestDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { travelRequest: TravelRequest },
    private travelRequestService: TravelRequestService,
    private snackBar: MatSnackBar,
    private authService: AuthService
  ) {
    this.travelRequest = data.travelRequest;
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

  close(): void {
    this.dialogRef.close();
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
} 