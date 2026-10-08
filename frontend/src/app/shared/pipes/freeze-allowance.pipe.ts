import { Pipe, PipeTransform } from '@angular/core';

/**
 * (60, 3) -> "60 days / 3 freezes"
 * (15, 1) -> "15 days / 1 freeze"
 * (0, 0)  -> "None"
 */
@Pipe({
  name: 'freezeAllowance',
  standalone: true
})
export class FreezeAllowancePipe implements PipeTransform {
  transform(
    days: number | null | undefined,
    freezes: number | null | undefined
  ): string {
    if (!days) {
      return 'None';
    }

    const count = freezes ?? 0;

    return `${days} days / ${count} ${count === 1 ? 'freeze' : 'freezes'}`;
  }
}
