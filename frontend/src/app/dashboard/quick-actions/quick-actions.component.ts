import { Component, output } from '@angular/core';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';

import { CheckInService } from '../../check-ins/check-in.service';
import {
  MemberFormDialogComponent,
  MemberFormDialogData
} from '../../members/member-form-dialog/member-form-dialog.component';
import { MemberSaveResult } from '../../members/member.model';
import { IconComponent } from '../../shared/components/icon/icon.component';

/** Quick Actions card: New Member, Manual Check-in, Register Class. */
@Component({
  selector: 'app-quick-actions',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './quick-actions.component.html',
  styleUrl: './quick-actions.component.css'
})
export class QuickActionsComponent {
  /** Emitted after a member was created (the dashboard reloads its numbers). */
  readonly memberCreated = output<void>();

  constructor(
    private readonly router: Router,
    private readonly dialog: MatDialog,
    private readonly checkInService: CheckInService
  ) {}

  newMember(): void {
    this.dialog
      .open<MemberFormDialogComponent, MemberFormDialogData, MemberSaveResult>(
        MemberFormDialogComponent,
        {
          data: { mode: 'add' },
          panelClass: 'titan-dialog',
          width: '480px'
        }
      )
      .afterClosed()
      .subscribe((saved) => {
        if (saved) {
          this.memberCreated.emit();
        }
      });
  }

  manualCheckIn(): void {
    void this.checkInService.openDialog();
  }

  registerClass(): void {
    this.router.navigate(['/classes']);
  }
}
