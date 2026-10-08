import { Component, computed, input, output } from '@angular/core';

export type StateKind = 'loading' | 'empty' | 'error';

const DEFAULT_MESSAGES: Record<StateKind, string> = {
  loading: 'Loading...',
  empty: 'No results found.',
  error: 'Something went wrong.'
};

/**
 * Loading / empty / error block used by every list and details screen.
 * The error state shows a Retry button that emits `retry`.
 *
 *   <app-state-message kind="error" message="Could not load members."
 *                      (retry)="reload()" />
 */
@Component({
  selector: 'app-state-message',
  standalone: true,
  templateUrl: './state-message.component.html',
  styleUrl: './state-message.component.css'
})
export class StateMessageComponent {
  readonly kind = input.required<StateKind>();
  readonly message = input<string>('');

  readonly retry = output<void>();

  readonly text = computed(
    () => this.message() || DEFAULT_MESSAGES[this.kind()]
  );

  onRetry(): void {
    this.retry.emit();
  }
}
