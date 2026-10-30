import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { NoorApiError } from '../api-error';
import { API_BASE } from '../api.config';
import { AuthResponse } from '../models/api.models';
import { SessionService } from '../services/session.service';

let refreshPromise: Promise<string | null> | null = null;

async function refreshAccessToken(session: SessionService): Promise<string | null> {
  if (refreshPromise) return refreshPromise;

  const refreshToken = session.refreshToken();
  if (!refreshToken) return null;

  refreshPromise = (async () => {
    try {
      const response = await fetch(`${API_BASE}/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken })
      });

      if (!response.ok) {
        session.clear();
        return null;
      }

      const data = (await response.json()) as AuthResponse;
      session.updateTokens(data.accessToken, data.refreshToken, data.user);
      return data.accessToken;
    } catch {
      return null;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const session = inject(SessionService);

  const headers = request.headers.set('X-Cart-Key', session.cartKey());
  const token = session.accessToken();
  const authorized = token
    ? request.clone({ headers: headers.set('Authorization', `Bearer ${token}`) })
    : request.clone({ headers });

  return next(authorized).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        token &&
        !request.url.includes('/auth/')
      ) {
        return from(refreshAccessToken(session)).pipe(
          switchMap((newToken) => {
            if (!newToken) {
              return throwError(
                () =>
                  new NoorApiError(
                    'Your session has expired. Please sign in again.',
                    401
                  )
              );
            }
            return next(
              authorized.clone({
                headers: authorized.headers.set('Authorization', `Bearer ${newToken}`)
              })
            );
          })
        );
      }

      return throwError(() => error);
    })
  );
};
