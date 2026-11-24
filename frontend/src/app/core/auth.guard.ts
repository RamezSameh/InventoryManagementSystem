import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { map } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * يمنع الوصول للصفحات المحمية بدون تسجيل دخول، ويشيك على انتهاء التوكن
 * (مش على وجوده بس): لو التوكن منتهي بيحاول تجديده مرة واحدة من الـ refresh
 * token قبل ما يحوّل لصفحة الدخول.
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.token()) {
    router.navigate(['/login']);
    return false;
  }
  if (!auth.isTokenExpired()) return true;
  return auth.refreshAccessToken().pipe(
    map(ok => {
      if (!ok) router.navigate(['/login']);
      return ok;
    })
  );
};
