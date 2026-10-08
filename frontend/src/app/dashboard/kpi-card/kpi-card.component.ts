import { Component, input } from '@angular/core';

import { IconComponent, IconName } from '../../shared/components/icon/icon.component';

export type KpiTone = 'positive' | 'negative' | 'neutral';

/** One statistic tile: label, big value and a small note under it. */
@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './kpi-card.component.html',
  styleUrl: './kpi-card.component.css'
})
export class KpiCardComponent {
  readonly label = input<string>('');
  readonly value = input<number | string>(0);
  readonly icon = input<IconName>('chart');
  readonly note = input<string>('');
  readonly tone = input<KpiTone>('neutral');
}
