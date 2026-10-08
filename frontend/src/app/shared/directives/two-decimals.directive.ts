import { Directive, ElementRef, HostListener, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgControl } from '@angular/forms';

/**
 * Shows a number input with two decimals (899 -> "899.00") when it is not being typed in.
 * Use it on a money field: <input type="number" formControlName="price" appTwoDecimals />
 * The form value stays a plain number.
 */
@Directive({
  selector: 'input[appTwoDecimals]',
  standalone: true
})
export class TwoDecimalsDirective implements OnInit {
  private readonly input = inject<ElementRef<HTMLInputElement>>(ElementRef).nativeElement;
  private readonly control = inject(NgControl);

  constructor() {
    // Loaded / patched values (the user is not typing then).
    this.control.valueChanges?.pipe(takeUntilDestroyed()).subscribe(() => {
      if (document.activeElement !== this.input) {
        this.format();
      }
    });
  }

  ngOnInit(): void {
    setTimeout(() => this.format());
  }

  @HostListener('blur')
  format(): void {
    const value = this.control.value;

    if (typeof value === 'number' && Number.isFinite(value)) {
      this.input.value = value.toFixed(2);
    }
  }
}
