import { Component, computed, input } from '@angular/core';
import { TitleCasePipe } from '@angular/common';

type BadgeTone =
  | 'green'
  | 'blue'
  | 'red'
  | 'grey'
  | 'outline'
  | 'filled'
  | 'red-outline'
  | 'navy-outline';

const TONES: Record<string, BadgeTone> = {
  Active: 'green',
  InProgress: 'navy-outline',
  Published: 'green',
  Frozen: 'blue',
  Expired: 'red',
  Retired: 'red',
  Full: 'red',
  Inactive: 'grey',
  Pending: 'grey',
  Upcoming: 'outline',
  Completed: 'filled',
  Cancelled: 'red-outline'
};

/**
 * Small coloured pill with a dot.
 * <app-status-badge status="Active" />
 * <app-status-badge status="InProgress" label="Active" />
 */
@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [TitleCasePipe],
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.css'
})
export class StatusBadgeComponent {
  readonly status = input.required<string>();
  readonly label = input<string | null>(null);

  readonly text = computed(
    () => this.label() ?? this.status().replace(/([a-z])([A-Z])/g, '$1 $2')
  );

  readonly badgeClass = computed(
    () => `badge ${TONES[this.status()] ?? 'grey'}`
  );
}
