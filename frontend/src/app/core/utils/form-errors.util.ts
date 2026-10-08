import { HttpErrorResponse } from '@angular/common/http';
import { AbstractControl, FormGroup } from '@angular/forms';

import { ApiProblem } from '../models/api-error.model';

/** Error key used on a control for a message that came from the server. */
export const SERVER_ERROR_KEY = 'server';

/** HTTP status of an error, or null when it is not an HTTP error. */
export function getHttpStatus(error: unknown): number | null {
  return error instanceof HttpErrorResponse ? error.status : null;
}

/** Parsed ProblemDetails body of an HTTP error, or null. */
export function getApiProblem(error: unknown): ApiProblem | null {
  if (!(error instanceof HttpErrorResponse)) {
    return null;
  }

  const body: unknown = error.error;

  return body !== null && typeof body === 'object'
    ? (body as ApiProblem)
    : null;
}

/** Human readable message of an API error (detail first, then title). */
export function getApiMessage(error: unknown): string | null {
  const problem = getApiProblem(error);

  if (problem?.detail) {
    return problem.detail;
  }

  if (problem?.title) {
    return problem.title;
  }

  return null;
}

/**
 * Puts the field errors of a 400/422 response onto the matching controls
 * (matched by name, ignoring case). Returns the messages that could not be
 * matched to any control so the screen can show them in a banner.
 */
export function applyServerErrors(
  form: FormGroup,
  error: unknown
): string[] {
  const unmatched: string[] = [];
  const problem = getApiProblem(error);

  if (!problem?.errors) {
    return unmatched;
  }

  for (const [field, messages] of Object.entries(problem.errors)) {
    const control = findControl(form, field);
    const text = messages.join(' ');

    if (control) {
      control.setErrors({
        ...(control.errors ?? {}),
        [SERVER_ERROR_KEY]: text
      });

      control.markAsTouched();
    } else {
      unmatched.push(text);
    }
  }

  return unmatched;
}

/** Removes any server error from the controls before a new submit. */
export function clearServerErrors(form: FormGroup): void {
  for (const control of Object.values(form.controls)) {
    const errors = control.errors;

    if (errors && SERVER_ERROR_KEY in errors) {
      const rest = { ...errors };
      delete rest[SERVER_ERROR_KEY];

      control.setErrors(Object.keys(rest).length > 0 ? rest : null);
    }
  }
}

function findControl(
  form: FormGroup,
  field: string
): AbstractControl | null {
  const key = Object.keys(form.controls)
    .find((name) => name.toLowerCase() === field.toLowerCase());

  return key ? form.controls[key] : null;
}
