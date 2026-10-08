import { Component, computed, input, output } from '@angular/core';

import { IconComponent } from '../icon/icon.component';

const MAX_VISIBLE_PAGES = 5;

/**
 * "Showing 1 to 10 of 48 entries" + numbered pages.
 * <app-paginator [total]="48" [page]="1" [pageSize]="10" (pageChange)="go($event)" />
 */
@Component({
  selector: 'app-paginator',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './paginator.component.html',
  styleUrl: './paginator.component.css'
})
export class PaginatorComponent {
  readonly total = input<number>(0);
  readonly page = input<number>(1);
  readonly pageSize = input<number>(10);

  readonly pageChange = output<number>();

  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.total() / this.pageSize()))
  );

  readonly from = computed(() =>
    this.total() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1
  );

  readonly to = computed(() =>
    Math.min(this.page() * this.pageSize(), this.total())
  );

  readonly pages = computed<number[]>(() => {
    const totalPages = this.totalPages();
    const start = Math.max(
      1,
      Math.min(this.page() - 2, totalPages - (MAX_VISIBLE_PAGES - 1))
    );
    const end = Math.min(totalPages, start + MAX_VISIBLE_PAGES - 1);

    const result: number[] = [];

    for (let value = start; value <= end; value++) {
      result.push(value);
    }

    return result;
  });

  go(target: number): void {
    if (target < 1 || target > this.totalPages() || target === this.page()) {
      return;
    }

    this.pageChange.emit(target);
  }
}
