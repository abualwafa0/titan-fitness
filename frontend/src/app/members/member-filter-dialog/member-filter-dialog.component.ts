import { Component, Inject, Signal, output, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { Branch } from '../../core/models/branch.model';
import { BranchService } from '../../core/services/branch.service';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { MEMBER_FILTER_STATUSES, MemberFilters } from '../member.model';

export interface MemberFilterDialogData {
  filters: MemberFilters;
}

/**
 * "Filter Members" dialog: branches + membership statuses. It emits the chosen
 * filters through `filtersApplied` (the opener subscribes) and then closes itself.
 */
@Component({
  selector: 'app-member-filter-dialog',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './member-filter-dialog.component.html',
  styleUrl: './member-filter-dialog.component.css'
})
export class MemberFilterDialogComponent {
  readonly filtersApplied = output<MemberFilters>();

  readonly branches: Signal<Branch[]>;
  readonly statusOptions = MEMBER_FILTER_STATUSES;

  readonly selectedBranchIds = signal<number[]>([]);
  readonly selectedStatuses = signal<string[]>([]);

  constructor(
    private readonly dialogRef: MatDialogRef<MemberFilterDialogComponent>,
    @Inject(MAT_DIALOG_DATA) data: MemberFilterDialogData,
    branchService: BranchService
  ) {
    this.branches = branchService.branches;
    this.selectedBranchIds.set([...data.filters.branchIds]);
    this.selectedStatuses.set([...data.filters.statuses]);
  }

  toggleBranch(branchId: number): void {
    this.selectedBranchIds.update((ids) =>
      ids.includes(branchId) ? ids.filter((id) => id !== branchId) : [...ids, branchId]
    );
  }

  toggleStatus(status: string): void {
    this.selectedStatuses.update((items) =>
      items.includes(status) ? items.filter((item) => item !== status) : [...items, status]
    );
  }

  apply(): void {
    this.filtersApplied.emit({
      branchIds: this.selectedBranchIds(),
      statuses: this.selectedStatuses()
    });

    this.dialogRef.close();
  }

  clear(): void {
    this.selectedBranchIds.set([]);
    this.selectedStatuses.set([]);
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
