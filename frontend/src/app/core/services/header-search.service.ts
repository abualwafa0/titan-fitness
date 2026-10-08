import { Injectable, signal } from '@angular/core';

const DEFAULT_PLACEHOLDER = 'Search...';
const DEBOUNCE_MS = 300;

/**
 * State of the search box in the header.
 * - `draft` is what the user is typing
 * - `term` is the committed value (updated 300 ms after the user stops typing);
 *   pages read `term` to filter their list.
 */
@Injectable({ providedIn: 'root' })
export class HeaderSearchService {
  readonly draft = signal<string>('');
  readonly term = signal<string>('');
  readonly placeholder = signal<string>(DEFAULT_PLACEHOLDER);

  private timer: ReturnType<typeof setTimeout> | null = null;

  /** Called by the header input on every keystroke. */
  onInput(value: string): void {
    this.draft.set(value);
    this.clearTimer();

    this.timer = setTimeout(() => {
      this.term.set(value.trim());
      this.timer = null;
    }, DEBOUNCE_MS);
  }

  /** Sets both the typed text and the committed term immediately. */
  setTerm(value: string): void {
    this.clearTimer();
    this.draft.set(value);
    this.term.set(value.trim());
  }

  setPlaceholder(text: string): void {
    this.placeholder.set(text);
  }

  /** Clears the search and restores the default placeholder. */
  reset(): void {
    this.clearTimer();
    this.draft.set('');
    this.term.set('');
    this.placeholder.set(DEFAULT_PLACEHOLDER);
  }

  private clearTimer(): void {
    if (this.timer !== null) {
      clearTimeout(this.timer);
      this.timer = null;
    }
  }
}
