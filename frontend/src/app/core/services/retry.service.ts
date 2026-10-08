import { Injectable, signal } from '@angular/core';

/**
 * "Retry" on the server-error toast: list screens read `tick()` inside their
 * reload effect, so calling `request()` makes them load again.
 */
@Injectable({ providedIn: 'root' })
export class RetryService {
  readonly tick = signal(0);

  request(): void {
    this.tick.update((value) => value + 1);
  }
}
