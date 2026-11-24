import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, UserDto } from '../core/api.service';
import { AuthService } from '../core/auth.service';

const ROLES = ['Admin', 'Manager', 'Cashier'];

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <div class="page-head">
    <div><h2>إدارة المستخدمين</h2><p>تغيير أدوار المستخدمين وتعطيل الحسابات أو إعادة تفعيلها (للأدمن فقط)</p></div>
  </div>
  <div class="panel" *ngIf="isAdmin(); else notAdmin">
    <table>
      <thead><tr><th>الاسم</th><th>البريد الإلكتروني</th><th>الدور</th><th>الحالة</th><th>إجراءات</th></tr></thead>
      <tbody>
        <tr *ngFor="let u of users">
          <td><span class="product-dot">◉</span><b>{{u.fullName || '—'}}</b></td>
          <td class="muted" dir="ltr">{{u.email}}</td>
          <td>
            <select [ngModel]="u.roles[0]" (ngModelChange)="changeRole(u, $event)" [disabled]="u.id === myId()">
              <option *ngFor="let r of roles" [value]="r">{{r}}</option>
            </select>
          </td>
          <td><span class="badge" [class.warn]="u.isLockedOut">{{u.isLockedOut ? 'معطّل' : 'نشط'}}</span></td>
          <td>
            <div class="row-actions">
              <button class="btn-ghost" *ngIf="!u.isLockedOut" [disabled]="u.id === myId()" (click)="setLockout(u, true)">تعطيل</button>
              <button class="btn-ghost" *ngIf="u.isLockedOut" (click)="setLockout(u, false)">تفعيل</button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    <div class="empty" *ngIf="!users.length && !loading">لا يوجد مستخدمون أو تعذر الاتصال بالخادم.</div>
    <div class="empty" *ngIf="loading">جارٍ التحميل...</div>
    <p class="error" *ngIf="error">{{error}}</p>
  </div>
  <ng-template #notAdmin>
    <div class="panel"><div class="empty">هذه الصفحة للأدمن فقط.</div></div>
  </ng-template>`
})
export class UsersComponent {
  api = inject(ApiService);
  private auth = inject(AuthService);
  users: UserDto[] = [];
  roles = ROLES;
  loading = false;
  error = '';

  isAdmin() { return this.auth.role() === 'Admin'; }

  /** معرّف المستخدم الحالي من الـ JWT (sub) — لمنع تعطيل/تغيير دور النفس */
  myId(): string {
    const t = this.auth.token();
    if (!t) return '';
    try {
      const payload = JSON.parse(atob(t.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
      return typeof payload['sub'] === 'string' ? payload['sub'] : '';
    } catch { return ''; }
  }

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.api.users().subscribe({
      next: x => { this.users = x ?? []; this.loading = false; },
      error: () => { this.users = []; this.loading = false; }
    });
  }

  changeRole(u: UserDto, role: string) {
    if (role === u.roles[0]) return;
    this.error = '';
    this.api.changeUserRole(u.id, role).subscribe({
      next: () => this.load(),
      error: () => { this.error = 'تعذر تغيير الدور.'; this.load(); }
    });
  }

  setLockout(u: UserDto, lockedOut: boolean) {
    if (!confirm(lockedOut ? `تعطيل حساب "${u.email}"؟` : `إعادة تفعيل حساب "${u.email}"؟`)) return;
    this.error = '';
    this.api.setUserLockout(u.id, lockedOut).subscribe({
      next: () => this.load(),
      error: () => { this.error = 'تعذر تنفيذ العملية.'; this.load(); }
    });
  }
}
