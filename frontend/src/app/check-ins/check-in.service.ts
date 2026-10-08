import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { MemberLookup } from '../members/member.model';
import { CheckInRequest, CheckInResponse } from './check-in.model';

/**
 * Check-in API + the check-in dialog opener.
 * `checkInVersion` goes up after every admitted check-in so the member list
 * and the dashboard reload themselves.
 */
@Injectable({ providedIn: 'root' })
export class CheckInService {
  private readonly versionState = signal(0);

  readonly checkInVersion = this.versionState.asReadonly();

  constructor(
    private readonly http: HttpClient,
    private readonly dialog: MatDialog
  ) {}

  checkIn(request: CheckInRequest): Observable<CheckInResponse> {
    return this.http.post<CheckInResponse>(`${environment.apiUrl}/check-ins`, request);
  }

  /** Called by the dialog after an admitted check-in. */
  notifyCheckedIn(): void {
    this.versionState.update((value) => value + 1);
  }

  /** Opens the Manual Check-in dialog (the component is loaded on demand). */
  async openDialog(member?: MemberLookup): Promise<void> {
    const { CheckInDialogComponent } = await import(
      './check-in-dialog/check-in-dialog.component'
    );

    this.dialog.open(CheckInDialogComponent, {
      data: { member: member ?? null },
      panelClass: 'titan-dialog',
      width: '520px',
      autoFocus: false
    });
  }
}
