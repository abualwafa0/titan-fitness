import { Component, Inject, OnInit, Signal, computed, output, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { Branch } from '../../core/models/branch.model';
import { BranchService } from '../../core/services/branch.service';
import { ClickOutsideDirective } from '../../shared/directives/click-outside.directive';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { TrainerFilters } from '../trainer.model';
import { TrainerService } from '../trainer.service';

export interface TrainerFilterDialogData {
  filters: TrainerFilters;
}

/**
 * "Filter Trainers" dialog. It emits the chosen filters through
 * `filtersApplied` (the opener subscribes) and then closes itself.
 */
@Component({
  selector: 'app-trainer-filter-dialog',
  standalone: true,
  imports: [IconComponent, ClickOutsideDirective],
  templateUrl: './trainer-filter-dialog.component.html',
  styleUrl: './trainer-filter-dialog.component.css'
})
export class TrainerFilterDialogComponent implements OnInit {
  readonly filtersApplied = output<TrainerFilters>();

  readonly branches: Signal<Branch[]>;

  readonly selectedBranchIds = signal<number[]>([]);
  readonly selectedSpecialties = signal<string[]>([]);
  readonly activeChecked = signal(false);
  readonly inactiveChecked = signal(false);

  readonly branchOpen = signal(false);

  readonly branchSummary = computed(() => {
    const names = this.branches()
      .filter((branch) => this.selectedBranchIds().includes(branch.branchId))
      .map((branch) => branch.branchName);

    if (names.length === 0) {
      return 'All branches';
    }

    return names.length <= 2 ? names.join(', ') : `${names.length} branches selected`;
  });

  readonly specialtyOptions = signal<string[]>([]);
  readonly specialtyText = signal('');

  readonly visibleSpecialties = computed(() => {
    const text = this.specialtyText().trim().toLowerCase();

    return this.specialtyOptions().filter((option) =>
      option.toLowerCase().includes(text)
    );
  });

  constructor(
    private readonly dialogRef: MatDialogRef<TrainerFilterDialogComponent>,
    @Inject(MAT_DIALOG_DATA) private readonly data: TrainerFilterDialogData,
    private readonly trainerService: TrainerService,
    branchService: BranchService
  ) {
    this.branches = branchService.branches;

    const filters = data.filters;

    this.selectedBranchIds.set([...filters.branchIds]);
    this.selectedSpecialties.set([...filters.specialties]);
    this.activeChecked.set(filters.isActive === true);
    this.inactiveChecked.set(filters.isActive === false);
  }

  ngOnInit(): void {
    this.trainerService.getSpecialties().subscribe({
      next: (options) => this.specialtyOptions.set(options),
      error: () => {
        // The error interceptor already shows the message.
      }
    });
  }

  toggleBranch(branchId: number): void {
    this.selectedBranchIds.update((ids) =>
      ids.includes(branchId) ? ids.filter((id) => id !== branchId) : [...ids, branchId]
    );
  }

  toggleSpecialty(specialty: string): void {
    this.selectedSpecialties.update((items) =>
      items.includes(specialty)
        ? items.filter((item) => item !== specialty)
        : [...items, specialty]
    );
  }

  onSpecialtyInput(event: Event): void {
    this.specialtyText.set((event.target as HTMLInputElement).value);
  }

  apply(): void {
    const active = this.activeChecked();
    const inactive = this.inactiveChecked();

    this.filtersApplied.emit({
      branchIds: this.selectedBranchIds(),
      specialties: this.selectedSpecialties(),
      // both or neither ticked = no status filter
      isActive: active === inactive ? null : active
    });

    this.dialogRef.close();
  }

  clear(): void {
    this.selectedBranchIds.set([]);
    this.selectedSpecialties.set([]);
    this.activeChecked.set(false);
    this.inactiveChecked.set(false);
    this.specialtyText.set('');
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
