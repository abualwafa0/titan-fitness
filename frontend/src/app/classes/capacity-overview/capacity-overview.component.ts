import { Component, computed, input } from '@angular/core';

import { IconComponent } from '../../shared/components/icon/icon.component';

/** Right card of the class schedule: total bookings and average fill rate. */
@Component({
  selector: 'app-capacity-overview',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './capacity-overview.component.html',
  styleUrl: './capacity-overview.component.css'
})
export class CapacityOverviewComponent {
  readonly totalBookings = input<number>(0);
  readonly fillRate = input<number>(0);

  readonly roundedRate = computed(() => Math.max(0, Math.min(100, Math.round(this.fillRate()))));
}
