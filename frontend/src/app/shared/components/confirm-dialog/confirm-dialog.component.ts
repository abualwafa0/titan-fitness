import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  /** Makes the confirm button red (destructive action). */
  danger?: boolean;
}

/**
 * Yes/No dialog. Closes with true (confirmed) or false (cancelled).
 *
 *   this.dialog
 *     .open(ConfirmDialogComponent, {
 *       data: { title: 'Discard changes?', message: '...' },
 *       panelClass: 'titan-dialog',
 *       width: '420px'
 *     })
 *     .afterClosed()
 *     .subscribe((confirmed) => { ... });
 */
@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css'
})
export class ConfirmDialogComponent {
  constructor(
    private readonly dialogRef: MatDialogRef<ConfirmDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) public readonly data: ConfirmDialogData
  ) {}

  confirm(): void {
    this.dialogRef.close(true);
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
