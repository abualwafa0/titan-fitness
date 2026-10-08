import { Component, input, model } from '@angular/core';
import { MatTooltip } from '@angular/material/tooltip';

export interface OptionItem {
  value: string | number;
  label: string;
  disabled?: boolean;
  /** Shown as a tooltip (typically why the option is disabled). */
  tooltip?: string;
}

export type OptionGroupVariant = 'toggle' | 'radio-card';

/**
 * Single-choice group.
 *  - 'toggle'     : segmented buttons (freeze duration, Day/Week)
 *  - 'radio-card' : cards with a radio dot (class duration, access scope)
 *
 *   <app-option-group [options]="options" variant="toggle" [(value)]="months" />
 */
@Component({
  selector: 'app-option-group',
  standalone: true,
  imports: [MatTooltip],
  templateUrl: './option-group.component.html',
  styleUrl: './option-group.component.css'
})
export class OptionGroupComponent {
  readonly options = input<OptionItem[]>([]);
  readonly variant = input<OptionGroupVariant>('toggle');
  readonly disabled = input<boolean>(false);

  readonly value = model<string | number | null>(null);

  select(option: OptionItem): void {
    if (this.disabled() || option.disabled) {
      return;
    }

    this.value.set(option.value);
  }
}
