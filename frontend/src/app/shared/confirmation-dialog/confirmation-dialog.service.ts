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
      ? `Are you sure you want to delete "${itemDetails}"? This action cannot be undone.`
      : `Are you sure you want to delete this ${itemName}? This action cannot be undone.`;

    return this.confirm({
      title: 'Confirm Deletion',
      message: message,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      confirmColor: 'warn',
      type: 'error',
      icon: 'delete_forever'
    });
  }

  confirmWarning(title: string, message: string, confirmText?: string): Observable<boolean> {
    return this.confirm({
      title: title,
      message: message,
      confirmText: confirmText || 'Continue',
      cancelText: 'Cancel',
      confirmColor: 'warn',
      type: 'warning'
    });
  }

  confirmInfo(title: string, message: string, confirmText?: string): Observable<boolean> {
    return this.confirm({
      title: title,
      message: message,
      confirmText: confirmText || 'OK',
      cancelText: 'Cancel',
      confirmColor: 'primary',
      type: 'info'
    });
  }

  confirmSuccess(title: string, message: string, confirmText?: string): Observable<boolean> {
    return this.confirm({
      title: title,
      message: message,
      confirmText: confirmText || 'OK',
      cancelText: 'Cancel',
      confirmColor: 'primary',
      type: 'success'
    });
  }
} 