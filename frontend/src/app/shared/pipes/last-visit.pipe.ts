import { Pipe, PipeTransform } from '@angular/core';

import {
  addDays,
  formatDateMedium,
  formatTime12,
  parseLocalDateTime,
  todayIso,
  toIsoDate
} from '../../core/utils/date.util';

/**
 * API date-time -> "Today, 08:30 AM" / "Yesterday" / "Oct 12, 2023".
 * null -> "—".
 */
@Pipe({
  name: 'lastVisit',
  standalone: true
})
export class LastVisitPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) {
      return '—';
    }

    const visit = parseLocalDateTime(value);
    const visitDate = toIsoDate(visit);
    const today = todayIso();

    if (visitDate === today) {
      return `Today, ${formatTime12(visit)}`;
    }

    if (visitDate === addDays(today, -1)) {
      return 'Yesterday';
    }

    return formatDateMedium(visitDate);
  }
}
