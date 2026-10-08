import { Component, input } from '@angular/core';
import { AbstractControl } from '@angular/forms';

import { SERVER_ERROR_KEY } from '../../../core/utils/form-errors.util';

/**
 * Shows the first validation message of a control (only after the user touched
 * it or changed it).
 *
 *   <app-field-error [control]="form.controls.email" label="Email" />
 *
 * A control error whose value is a string (or { message: string }) is shown
 * as is, so custom validators can return their own text. The `server` error
 * (set by applyServerErrors) is shown first.
 */
@Component({
  selector: 'app-field-error',
  standalone: true,
  templateUrl: './field-error.component.html',
  styleUrl: './field-error.component.css'
})
export class FieldErrorComponent {
  readonly control = input<AbstractControl | null>(null);
  readonly label = input<string>('This field');

  /** Called from the template on every change detection (control state is not a signal). */
  message(): string | null {
    const control = this.control();

    if (!control || !control.errors || !(control.touched || control.dirty)) {
      return null;
    }

    const errors = control.errors;
    const label = this.label();

    const server = errors[SERVER_ERROR_KEY];

    if (typeof server === 'string' && server) {
      return server;
    }

    for (const [key, value] of Object.entries(errors)) {
      if (key === SERVER_ERROR_KEY) {
        continue;
      }

      const text = this.textFor(key, value, label);

      if (text) {
        return text;
      }
    }

    return `${label} is not valid.`;
  }

  private textFor(key: string, value: unknown, label: string): string | null {
    switch (key) {
      case 'required':
        return `${label} is required.`;

      case 'minlength':
        return `${label} must be at least ${this.read(value, 'requiredLength')} characters.`;

      case 'maxlength':
        return `${label} cannot exceed ${this.read(value, 'requiredLength')} characters.`;

      case 'email':
        return 'Enter a valid email address.';

      case 'pattern':
        return `${label} is not valid.`;

      case 'min':
        return `${label} must be at least ${this.read(value, 'min')}.`;

      case 'max':
        return `${label} cannot exceed ${this.read(value, 'max')}.`;

      default:
        break;
    }

    if (typeof value === 'string') {
      return value;
    }

    if (value !== null && typeof value === 'object') {
      const message = (value as { message?: unknown }).message;

      if (typeof message === 'string') {
        return message;
      }
    }

    return null;
  }

  private read(value: unknown, property: string): string {
    if (value !== null && typeof value === 'object') {
      const found = (value as Record<string, unknown>)[property];

      if (typeof found === 'number' || typeof found === 'string') {
        return String(found);
      }
    }

    return '';
  }
}
