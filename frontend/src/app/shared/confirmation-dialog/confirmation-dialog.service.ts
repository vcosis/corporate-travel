import { Injectable } from '@angular/core';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { ConfirmationDialogComponent, ConfirmationDialogData } from './confirmation-dialog';

@Injectable({
  providedIn: 'root'
})
export class ConfirmationDialogService {
  constructor(private dialog: MatDialog) {}

  confirm(data: ConfirmationDialogData, config?: Partial<MatDialogConfig>): Observable<boolean> {
    const defaultConfig: MatDialogConfig = {
      width: '400px',
      maxWidth: '90vw',
      data: data,
      disableClose: true,
      autoFocus: false,
      restoreFocus: true,
      panelClass: 'confirmation-dialog-panel'
    };

    const dialogConfig = { ...defaultConfig, ...config };
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, dialogConfig);

    return dialogRef.afterClosed();
  }

  confirmDelete(itemName: string = 'item', itemDetails?: string): Observable<boolean> {
    const message = itemDetails 
      ? `Tem certeza que deseja excluir "${itemDetails}"? Esta ação não pode ser desfeita.`
      : `Tem certeza que deseja excluir este ${itemName}? Esta ação não pode ser desfeita.`;

    return this.confirm({
      title: 'Confirmar Exclusão',
      message: message,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      confirmColor: 'warn',
      type: 'error'
    });
  }

  confirmWarning(title: string, message: string, confirmText?: string): Observable<boolean> {
    return this.confirm({
      title: title,
      message: message,
      confirmText: confirmText || 'Continuar',
      cancelText: 'Cancelar',
      confirmColor: 'warn',
      type: 'warning'
    });
  }

  confirmInfo(title: string, message: string, confirmText?: string): Observable<boolean> {
    return this.confirm({
      title: title,
      message: message,
      confirmText: confirmText || 'OK',
      cancelText: 'Cancelar',
      confirmColor: 'primary',
      type: 'info'
    });
  }

  confirmSuccess(title: string, message: string, confirmText?: string): Observable<boolean> {
    return this.confirm({
      title: title,
      message: message,
      confirmText: confirmText || 'OK',
      cancelText: 'Cancelar',
      confirmColor: 'primary',
      type: 'success'
    });
  }
} 