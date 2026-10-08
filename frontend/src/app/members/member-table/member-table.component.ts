import { Component, computed, input, output } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { CheckInService } from '../../check-ins/check-in.service';
import { AvatarComponent } from '../../shared/components/avatar/avatar.component';
import { RowMenuComponent, RowMenuItem } from '../../shared/components/row-menu/row-menu.component';
import { SortChange, SortDirection, SortHeaderComponent } from '../../shared/components/sort-header/sort-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { HighlightPipe } from '../../shared/pipes/highlight.pipe';
import { LastVisitPipe } from '../../shared/pipes/last-visit.pipe';
import { MemberListItem, formatMemberNumber } from '../member.model';

interface MemberRow {
  member: MemberListItem;
  items: RowMenuItem[];
}

/** Table of the member directory (child component: data comes in, sort events go out). */
@Component({
  selector: 'app-member-table',
  standalone: true,
  imports: [
    RouterLink,
    AvatarComponent,
    HighlightPipe,
    LastVisitPipe,
    RowMenuComponent,
    SortHeaderComponent,
    StatusBadgeComponent
  ],
  templateUrl: './member-table.component.html',
  styleUrl: './member-table.component.css'
})
export class MemberTableComponent {
  readonly members = input<MemberListItem[]>([]);
  readonly searchTerm = input<string>('');
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection>('asc');

  readonly sortChange = output<SortChange>();

  /** Each row with its menu (built once per data change, not on every render). */
  readonly rows = computed<MemberRow[]>(() =>
    this.members().map((member) => ({ member, items: this.buildItems(member) }))
  );

  constructor(
    private readonly router: Router,
    private readonly checkInService: CheckInService
  ) {}

  idLabel(member: MemberListItem): string {
    return formatMemberNumber(member.membershipNumber);
  }

  onAction(member: MemberListItem, action: string): void {
    switch (action) {
      case 'view':
        this.router.navigate(['/members', member.memberId]);
        break;

      case 'checkIn':
        void this.checkInService.openDialog({
          memberId: member.memberId,
          fullName: member.fullName,
          membershipNumber: member.membershipNumber,
          photo: null,
          status: member.status
        });
        break;

      case 'book':
        // The class schedule opens in "booking for this member" mode (Book Session has the member pre-selected).
        this.router.navigate(['/classes'], {
          queryParams: { bookFor: member.memberId, branchId: member.homeBranchId }
        });
        break;

      case 'freeze':
        this.router.navigate(['/members', member.memberId, 'freeze']);
        break;

      default:
        break;
    }
  }

  private buildItems(member: MemberListItem): RowMenuItem[] {
    const notActive = this.notActiveReason(member);
    const noFreezes =
      !notActive && (member.freezesRemaining ?? 0) <= 0
        ? 'No freezes remaining on this plan'
        : null;

    return [
      { id: 'view', label: 'View Profile' },
      {
        id: 'checkIn',
        label: 'Check-In',
        disabled: notActive !== null,
        disabledReason: notActive ?? undefined
      },
      {
        id: 'book',
        label: 'Book Class',
        disabled: notActive !== null,
        disabledReason: notActive ?? undefined
      },
      {
        id: 'freeze',
        label: 'Freeze Membership',
        disabled: notActive !== null || noFreezes !== null,
        disabledReason: notActive ?? noFreezes ?? undefined
      }
    ];
  }

  /** Why the member cannot check in / freeze, or null when the membership is Active. */
  private notActiveReason(member: MemberListItem): string | null {
    switch (member.status) {
      case 'Active':
        return null;
      case 'Frozen':
        return 'Membership is Frozen';
      case 'Expired':
        return 'Membership expired';
      case null:
        return 'No membership';
      default:
        return `Membership is ${member.status}`;
    }
  }
}
