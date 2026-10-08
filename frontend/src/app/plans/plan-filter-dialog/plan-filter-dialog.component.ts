import { Component, Inject, OnInit, computed, output, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { IconComponent } from '../../shared/components/icon/icon.component';
import { PlanAccessScope, PlanFilters } from '../plan.model';
import { PlanService } from '../plan.service';

export interface PlanFilterDialogData {
  filters: PlanFilters;
}

/**
 * "Filter Plans" dialog. It emits the chosen filters through `filtersApplied`
 * (the opener subscribes) and then closes itself.
 */
@Component({
  selector: 'app-plan-filter-dialog',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './plan-filter-dialog.component.html',
  styleUrl: './plan-filter-dialog.component.css'
})
export class PlanFilterDialogComponent implements OnInit {
  readonly filtersApplied = output<PlanFilters>();

  readonly durationOptions = signal<number[]>([]);
  readonly selectedDurations = signal<number[]>([]);
  readonly accessScope = signal<PlanAccessScope | null>(null);

  readonly minBound = signal(0);
  readonly maxBound = signal(1000);
  readonly minPrice = signal<number | null>(null);
  readonly maxPrice = signal<number | null>(null);

  readonly sliderMin = computed(() => this.minPrice() ?? this.minBound());
  readonly sliderMax = computed(() => this.maxPrice() ?? this.maxBound());

  readonly publishedChecked = signal(false);
  readonly retiredChecked = signal(false);

  readonly accessChoices: { value: PlanAccessScope | null; label: string }[] = [
    { value: null, label: 'Any' },
    { value: 'AllBranches', label: 'All branches' },
    { value: 'HomeBranchOnly', label: 'Home branch only' }
  ];

  constructor(
    private readonly dialogRef: MatDialogRef<PlanFilterDialogComponent>,
    @Inject(MAT_DIALOG_DATA) private readonly data: PlanFilterDialogData,
    private readonly planService: PlanService
  ) {
    const filters = data.filters;

    this.selectedDurations.set([...filters.durations]);
    this.accessScope.set(filters.accessScope);
    this.minPrice.set(filters.minPrice);
    this.maxPrice.set(filters.maxPrice);
    this.publishedChecked.set(filters.isPublished === true);
    this.retiredChecked.set(filters.isPublished === false);
  }

  ngOnInit(): void {
    this.planService.getFilterOptions().subscribe({
      next: (options) => {
        this.durationOptions.set([...options.durations].sort((a, b) => a - b));
        this.minBound.set(Math.floor(options.minPrice));
        this.maxBound.set(Math.max(Math.ceil(options.maxPrice), Math.floor(options.minPrice) + 1));
      },
      error: () => {
        // The error interceptor already shows the message.
      }
    });
  }

  toggleDuration(months: number): void {
    this.selectedDurations.update((items) =>
      items.includes(months) ? items.filter((item) => item !== months) : [...items, months]
    );
  }

  durationText(months: number): string {
    return months === 1 ? '1 month' : `${months} months`;
  }

  onMinInput(event: Event): void {
    this.minPrice.set(this.parsePrice((event.target as HTMLInputElement).value));
  }

  onMaxInput(event: Event): void {
    this.maxPrice.set(this.parsePrice((event.target as HTMLInputElement).value));
  }

  onMinSlider(event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);

    this.minPrice.set(Math.min(value, this.sliderMax()));
  }

  onMaxSlider(event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);

    this.maxPrice.set(Math.max(value, this.sliderMin()));
  }

  apply(): void {
    let min = this.minPrice();
    let max = this.maxPrice();

    // Values equal to the catalogue bounds do not narrow anything.
    if (min !== null && min <= this.minBound()) {
      min = null;
    }

    if (max !== null && max >= this.maxBound()) {
      max = null;
    }

    if (min !== null && max !== null && min > max) {
      [min, max] = [max, min];
    }

    const published = this.publishedChecked();
    const retired = this.retiredChecked();

    this.filtersApplied.emit({
      durations: this.selectedDurations(),
      accessScope: this.accessScope(),
      minPrice: min,
      maxPrice: max,
      // both or neither ticked = no status filter
      isPublished: published === retired ? null : published
    });

    this.dialogRef.close();
  }

  clear(): void {
    this.selectedDurations.set([]);
    this.accessScope.set(null);
    this.minPrice.set(null);
    this.maxPrice.set(null);
    this.publishedChecked.set(false);
    this.retiredChecked.set(false);
  }

  cancel(): void {
    this.dialogRef.close();
  }

  private parsePrice(text: string): number | null {
    if (text.trim() === '') {
      return null;
    }

    const value = Number(text);

    return Number.isFinite(value) && value >= 0 ? value : null;
  }
}
