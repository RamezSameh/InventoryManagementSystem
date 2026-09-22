import { Injectable } from '@angular/core';

const TOKEN_KEY = 'inventory_token';
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
  token(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.token();
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
    location.href = '/login';
  }
}
