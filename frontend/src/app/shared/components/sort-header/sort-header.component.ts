import { Component, input, output } from '@angular/core';

import { IconComponent } from '../icon/icon.component';

export type SortDirection = 'asc' | 'desc';

export interface SortChange {
  column: string;
  direction: SortDirection;
}

/**
 * Clickable column title. Use it inside a <th>.
 * <app-sort-header column="name" label="Member Name"
 *                  [activeColumn]="sortBy()" [direction]="sortDirection()"
 *                  (sortChange)="onSort($event)" />
 */
@Component({
  selector: 'app-sort-header',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './sort-header.component.html',
  styleUrl: './sort-header.component.css'
})
export class SortHeaderComponent {
  readonly column = input.required<string>();
  readonly label = input<string>('');
  readonly activeColumn = input<string | null>(null);
  readonly direction = input<SortDirection>('asc');

  readonly sortChange = output<SortChange>();

  onClick(): void {
    const isActive = this.activeColumn() === this.column();

    this.sortChange.emit({
      column: this.column(),
      direction: isActive && this.direction() === 'asc' ? 'desc' : 'asc'
    });
  }
}
