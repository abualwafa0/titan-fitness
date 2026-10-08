import { Component, computed, signal } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';

import { ListContextService } from '../../core/services/list-context.service';
import { getHttpStatus } from '../../core/utils/form-errors.util';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StateMessageComponent } from '../../shared/components/state-message/state-message.component';
import { ActivityListComponent } from '../activity-list/activity-list.component';
import { IdentityCardComponent } from '../identity-card/identity-card.component';
import {
  MemberFormDialogComponent,
  MemberFormDialogData
} from '../member-form-dialog/member-form-dialog.component';
import { PlanCardComponent } from '../plan-card/plan-card.component';
import { UsageCardComponent } from '../usage-card/usage-card.component';
import { MemberProfile, MemberSaveResult } from '../member.model';
import { MemberService } from '../member.service';

type LoadState = 'loading' | 'ready' | 'notFound' | 'error';

/** Member Profile page (parent of identity-card, plan-card, usage-card and activity-list). */
@Component({
  selector: 'app-member-profile',
  standalone: true,
  imports: [
    IconComponent,
    PageHeaderComponent,
    StateMessageComponent,
    IdentityCardComponent,
    PlanCardComponent,
    UsageCardComponent,
    ActivityListComponent
  ],
  templateUrl: './member-profile.component.html',
  styleUrl: './member-profile.component.css'
})
export class MemberProfileComponent {
  readonly loadState = signal<LoadState>('loading');
  readonly profile = signal<MemberProfile | null>(null);

  readonly membership = computed(() => this.profile()?.currentMembership ?? null);

  readonly backQueryParams: Params;

  private readonly memberId: number | null;

  constructor(
    route: ActivatedRoute,
    private readonly dialog: MatDialog,
    private readonly memberService: MemberService,
    listContext: ListContextService
  ) {
    this.backQueryParams = listContext.get('members');

    const id = Number(route.snapshot.paramMap.get('id'));

    this.memberId = Number.isInteger(id) && id > 0 ? id : null;

    if (this.memberId === null) {
      this.loadState.set('notFound');
    } else {
      this.load();
    }
  }

  load(): void {
    if (this.memberId === null) {
      return;
    }

    this.loadState.set('loading');

    this.memberService.getProfile(this.memberId).subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.loadState.set('ready');
      },
      error: (error: unknown) => {
        this.loadState.set(getHttpStatus(error) === 404 ? 'notFound' : 'error');
      }
    });
  }

  editProfile(): void {
    const profile = this.profile();

    if (!profile) {
      return;
    }

    this.dialog
      .open<MemberFormDialogComponent, MemberFormDialogData, MemberSaveResult>(
        MemberFormDialogComponent,
        {
          data: { mode: 'edit', member: profile },
          panelClass: 'titan-dialog',
          width: '480px'
        }
      )
      .afterClosed()
      .subscribe((saved) => {
        if (saved) {
          this.load();
        }
      });
  }
}
