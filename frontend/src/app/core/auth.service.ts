import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, finalize, map, of, share } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginResponse } from './api.service';

const TOKEN_KEY = 'inventory_token';
const REFRESH_TOKEN_KEY = 'inventory_refresh_token';
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

function decodePayload(token: string): Record<string, unknown> | null {
  try {
    const part = token.split('.')[1];
    const json = atob(part.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;
  /** طلب تجديد واحد مشترك — عشان كذا 401 في نفس اللحظة ميعملوش كذا تجديد (التوكن القديم بيتلغي عند التجديد). */
  private refreshInFlight$: Observable<boolean> | null = null;

  token(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  refreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  storeTokens(token: string, refreshToken: string): void {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  }

  /** هل الـ access token منتهي (أو على وشك الانتهاء — هامش 30 ثانية)؟ */
  isTokenExpired(): boolean {
    const t = this.token();
    if (!t) return true;
    const exp = decodePayload(t)?.['exp'];
    if (typeof exp !== 'number') return true;
    return exp * 1000 <= Date.now() + 30_000;
  }

  isLoggedIn(): boolean {
    return !!this.token() && !this.isTokenExpired();
  }

  /** يجدّد الـ access token من الـ refresh token (طلب واحد مشترك لو اتنادى كذا مرة مع بعض). */
  refreshAccessToken(): Observable<boolean> {
    if (this.refreshInFlight$) return this.refreshInFlight$;
    const rt = this.refreshToken();
    if (!rt) return of(false);
    const req$ = this.http.post<LoginResponse>(`${this.base}/auth/refresh`, { refreshToken: rt }).pipe(
      map(r => { this.storeTokens(r.token, r.refreshToken); return true; }),
      catchError(() => of(false)),
      finalize(() => { this.refreshInFlight$ = null; }),
      share()
    );
    this.refreshInFlight$ = req$;
    return req$;
  }

  /** الدور المستخرج من الـ JWT (Admin / Manager / Cashier) */
  role(): string | null {
    const t = this.token();
    if (!t) return null;
    const p = decodePayload(t);
    if (!p) return null;
    const r = p[ROLE_CLAIM] ?? p['role'];
    if (Array.isArray(r)) return typeof r[0] === 'string' ? r[0] : null;
    return typeof r === 'string' ? r : null;
  }

  /** هل يملك صلاحيات الإدارة (إضافة/تعديل/حذف)؟ */
  canManage(): boolean {
    const r = this.role();
    return r === 'Admin' || r === 'Manager';
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    location.href = '/login';
  }
}
