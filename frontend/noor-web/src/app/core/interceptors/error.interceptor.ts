import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NoorApiError } from '../api-error';
import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
  const toast = inject(ToastService);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof NoorApiError) {
        return throwError(() => error);
      }

      let message = 'Something went wrong. Please try again.';
      let status = 0;
      let payload: unknown;

      if (error instanceof HttpErrorResponse) {
        status = error.status;
        payload = error.error;

        if (status === 0) {
          message = 'We could not reach the atelier. Check your connection and try again.';
        } else if (typeof payload === 'string' && payload.trim()) {
          message = payload;
        } else if (payload && typeof payload === 'object') {
          const problem = payload as {
            detail?: string;
            message?: string;
            title?: string;
            errors?: Record<string, string[]>;
          };
          const validation = problem.errors
            ? Object.values(problem.errors).flat().find((item) => Boolean(item))
            : undefined;
          message = validation || problem.detail || problem.message || problem.title || message;
        }

        if (status === 401 && !message) message = 'Please sign in to continue.';
        if (status === 403) message = message || 'You do not have access to that.';
        if (status === 429) {
          message = 'Too many attempts. Please wait a moment and try again.';
        }
      }

      const silent = request.headers.has('X-No-Toast');
      if (!silent) {
        toast.show(message, 'error', 5200);
      }

      return throwError(() => new NoorApiError(message, status, payload));
    })
  );
};
