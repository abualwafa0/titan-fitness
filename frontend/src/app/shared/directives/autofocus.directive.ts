import { AfterViewInit, Directive, ElementRef } from '@angular/core';

const FIELD_SELECTOR =
  'input:not([disabled]):not([readonly]):not([type="hidden"]), ' +
  'select:not([disabled]), ' +
  'textarea:not([disabled]):not([readonly])';

const MAX_ATTEMPTS = 10;
const RETRY_DELAY_MS = 50;

/**
 * Focuses the first enabled field inside the host element.
 * Use it on a dialog form: <form appAutofocus> ...
 * If the fields are rendered later (after data loads) it retries a few times.
 */
@Directive({
  selector: '[appAutofocus]',
  standalone: true
})
export class AutofocusDirective implements AfterViewInit {
  constructor(private readonly host: ElementRef<HTMLElement>) {}

  ngAfterViewInit(): void {
    this.tryFocus(1);
  }

  private tryFocus(attempt: number): void {
    setTimeout(() => {
      const target =
        this.host.nativeElement.querySelector<HTMLElement>(FIELD_SELECTOR);

      if (target) {
        target.focus();
        return;
      }

      if (attempt < MAX_ATTEMPTS) {
        this.tryFocus(attempt + 1);
      }
    }, attempt === 1 ? 0 : RETRY_DELAY_MS);
  }
}
