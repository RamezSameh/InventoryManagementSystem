import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * يضيف الـ Bearer token لكل طلب، ولو الخادم رد 401 (التوكن انتهى) يجدّد
 * التوكن من الـ refresh token ويعيد الطلب مرة واحدة بس.
 * طلبات المصادقة نفسها (/auth/login و/auth/refresh) مبتتجدّدش عشان مندخلش في حلقة.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.token();
  const authed = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authed).pipe(
    catchError((err: unknown) => {
      if (!(err instanceof HttpErrorResponse) || err.status !== 401) return throwError(() => err);
      if (req.url.includes('/auth/') || req.headers.has('X-Retry')) return throwError(() => err);
      if (!auth.refreshToken()) {
        auth.logout();
        return throwError(() => err);
      }
      return auth.refreshAccessToken().pipe(
        switchMap(ok => {
          if (!ok) {
            auth.logout();
            return throwError(() => err);
          }
          const retry = req.clone({
            setHeaders: { Authorization: `Bearer ${auth.token() ?? ''}`, 'X-Retry': 'true' }
          });
          return next(retry);
        }),
        catchError(() => {
          auth.logout();
          return throwError(() => err);
        })
      );
    })
  );
};
