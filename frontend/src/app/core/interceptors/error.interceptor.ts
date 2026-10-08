import {
  HttpErrorResponse,
  HttpInterceptorFn
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { ApiProblem } from '../models/api-error.model';
import { AuthService } from '../services/auth.service';
import { RetryService } from '../services/retry.service';
import { ToastService } from '../services/toast.service';

/**
 * - Adds the auth token header to every request (when a token exists).
 * - Handles HTTP errors in ONE place (toasts / redirects).
 * - Always re-throws the error so each screen can show its own inline message.
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const toast = inject(ToastService);
  const retry = inject(RetryService);

  const token = auth.token();

  const authorizedRequest = token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  return next(authorizedRequest).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        handleError(error, request.method, auth, router, toast, retry);
      }

      return throwError(() => error);
    })
  );
};

function handleError(
  error: HttpErrorResponse,
  method: string,
  auth: AuthService,
  router: Router,
  toast: ToastService,
  retry: RetryService
): void {
  const problem = readProblem(error);
  const isPageLoad = method === 'GET';

  switch (error.status) {
    case 0:
      toast.error("Can't reach the server. Check your connection.");
      break;

    case 400:
    case 422:
      // Field errors are mapped onto the form by the screen; no toast then.
      if (!hasFieldErrors(problem)) {
        toast.error(
          problem?.detail ?? problem?.title ?? 'The request is not valid.'
        );
      }
      break;

    case 401:
      auth.clearSession();

      if (!router.url.startsWith('/login')) {
        void router.navigate(['/login'], {
          queryParams: { returnUrl: router.url }
        });
      }
      break;

    case 403:
      if (isPageLoad) {
        void router.navigate(['/access-denied']);
      } else {
        toast.error("You don't have permission to do this.");
      }
      break;

    case 404:
      // Page loads show their own "not found" state.
      if (!isPageLoad) {
        toast.error('The item no longer exists.');
      }
      break;

    case 409:
      toast.error(
        problem?.detail ?? 'The item already exists or was changed by someone else.'
      );
      break;

    default:
      if (error.status >= 500) {
        // On list/page loads the toast offers a Retry that reloads the screen.
        toast.error(
          'Something went wrong. Please try again.',
          isPageLoad ? { label: 'Retry', handler: () => retry.request() } : undefined
        );
      }
      break;
  }
}

function readProblem(error: HttpErrorResponse): ApiProblem | null {
  const body: unknown = error.error;

  return body !== null && typeof body === 'object'
    ? (body as ApiProblem)
    : null;
}

function hasFieldErrors(problem: ApiProblem | null): boolean {
  return !!problem?.errors && Object.keys(problem.errors).length > 0;
}
