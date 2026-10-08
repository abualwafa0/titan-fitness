import { Component, computed, input, signal } from '@angular/core';

import { IconComponent, IconName } from '../../shared/components/icon/icon.component';
import {
  formatDateMedium,
  formatTime12,
  parseLocalDateTime,
  toIsoDate,
  todayIso
} from '../../core/utils/date.util';
import { MemberActivity } from '../member.model';

const COLLAPSED_COUNT = 3;

/** Recent Activity: the first 3 items, "View All" shows all of them. */
@Component({
  selector: 'app-activity-list',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './activity-list.component.html',
  styleUrl: './activity-list.component.css'
})
export class ActivityListComponent {
  readonly activities = input<MemberActivity[]>([]);

  readonly expanded = signal(false);

  readonly visible = computed(() =>
    this.expanded() ? this.activities() : this.activities().slice(0, COLLAPSED_COUNT)
  );

  readonly canExpand = computed(() => this.activities().length > COLLAPSED_COUNT);

  toggle(): void {
    this.expanded.update((value) => !value);
  }

  /** "Today" or "Oct 10, 2023" (first line of the time column). */
  dayLabel(value: string): string {
    const date = parseLocalDateTime(value);
    const iso = toIsoDate(date);

    return iso === todayIso() ? 'Today' : formatDateMedium(iso);
  }

  /** "08:45 AM" (second line of the time column). */
  timeLabel(value: string): string {
    return formatTime12(parseLocalDateTime(value));
  }

  iconFor(type: string): IconName {
    return type === 'Class Attendance' ? 'classes' : 'user-check';
  }
}
