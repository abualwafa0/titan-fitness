import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

export interface ToastAction {
  label: string;
  handler: () => void;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  constructor(private readonly snackBar: MatSnackBar) {}

  success(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3500,
      panelClass: 'toast-success',
      horizontalPosition: 'center',
      verticalPosition: 'bottom'
    });
  }

  info(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 4000,
      panelClass: 'toast-info',
      horizontalPosition: 'center',
      verticalPosition: 'bottom'
    });
  }

  error(message: string, action?: ToastAction): void {
    const reference = this.snackBar.open(message, action?.label ?? 'Close', {
      duration: action ? 8000 : 5000,
      panelClass: 'toast-error',
      horizontalPosition: 'center',
      verticalPosition: 'bottom'
    });

    if (action) {
      reference.onAction().subscribe(() => action.handler());
    }
  }
}
