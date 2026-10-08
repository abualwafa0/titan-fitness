import { Component, computed, input } from '@angular/core';

/**
 * "Freezes Used 1 / 3" or "Guest Passes 2 / 5" with a progress bar and the
 * remaining amount ("2 freezes remaining").
 */
@Component({
  selector: 'app-usage-card',
  standalone: true,
  templateUrl: './usage-card.component.html',
  styleUrl: './usage-card.component.css'
})
export class UsageCardComponent {
  readonly title = input<string>('');
  readonly used = input<number>(0);
  readonly total = input<number>(0);
  readonly singular = input<string>('item');
  readonly plural = input<string>('items');

  readonly remaining = computed(() => Math.max(0, this.total() - this.used()));

  readonly percent = computed(() =>
    this.total() <= 0 ? 0 : Math.min(100, Math.round((this.used() / this.total()) * 100))
  );

  readonly remainingText = computed(() => {
    const remaining = this.remaining();

    return `${remaining} ${remaining === 1 ? this.singular() : this.plural()} remaining`;
  });
}
