import { Pipe, PipeTransform } from '@angular/core';

/** 12 -> "12 months", 1 -> "1 month". */
@Pipe({
  name: 'durationLabel',
  standalone: true
})
export class DurationLabelPipe implements PipeTransform {
  transform(months: number | null | undefined): string {
    if (months === null || months === undefined) {
      return '—';
    }

    return `${months} ${months === 1 ? 'month' : 'months'}`;
  }
}
