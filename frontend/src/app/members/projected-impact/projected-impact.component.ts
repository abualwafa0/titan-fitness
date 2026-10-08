import { DatePipe } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';

import { IconComponent } from '../../shared/components/icon/icon.component';

/** Right panel of the freeze screen: what will change + Confirm Freeze / Cancel. */
@Component({
  selector: 'app-projected-impact',
  standalone: true,
  imports: [DatePipe, IconComponent],
  templateUrl: './projected-impact.component.html',
  styleUrl: './projected-impact.component.css'
})
export class ProjectedImpactComponent {
  readonly originalEndDate = input<string | null>(null);
  readonly durationMonths = input<number | null>(null);
  readonly newEndDate = input<string | null>(null);
  readonly canConfirm = input<boolean>(false);
  readonly saving = input<boolean>(false);

  readonly confirm = output<void>();
  readonly cancelled = output<void>();

  readonly durationText = computed(() => {
    const months = this.durationMonths();

    if (!months) {
      return '—';
    }

    return months === 1 ? '1 Month' : `${months} Months`;
  });
}
