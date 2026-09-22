import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
  <div class="shell">
    <aside>
      <div class="brand"><span class="brand-mark">▦</span><div><strong>مخزني</strong><small>إدارة المخزون</small></div></div>
      <nav>
        <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}">⌂ <span>لوحة التحكم</span></a>
        <a routerLink="/products" routerLinkActive="active">▤ <span>المنتجات</span></a>
        <a routerLink="/categories" routerLinkActive="active">▦ <span>التصنيفات</span></a>
        <a routerLink="/warehouses" routerLinkActive="active">▣ <span>المستودعات</span></a>
        <a routerLink="/stock" routerLinkActive="active">⇄ <span>حركة المخزون</span></a>
        <a routerLink="/movements" routerLinkActive="active">≣ <span>سجل الحركات</span></a>
      </nav>
      <div class="aside-footer">
        <div class="avatar">م</div>
        <div><b>مدير النظام</b><small>{{ role() ?? 'مستخدم' }}</small></div>
        <button (click)="logout()" title="تسجيل الخروج">↪</button>
      </div>
    </aside>
    <main>
      <header>
        <div><span class="eyebrow">نظام إدارة المخزون</span><h1>مرحباً بك، مدير النظام</h1></div>
        <div class="header-actions">
          <span class="date">{{ today }}</span>
          <a routerLink="/stock" class="bell" title="تنبيهات المخزون">♧</a>
        </div>
      </header>
      <section class="content"><router-outlet /></section>
    </main>
  </div>`
})
export class AppComponent {
  private auth = inject(AuthService);
  today = new Date().toLocaleDateString('ar-EG', { day: 'numeric', month: 'long', year: 'numeric' });
  role() { return this.auth.role(); }
  logout() { this.auth.logout(); }
}
