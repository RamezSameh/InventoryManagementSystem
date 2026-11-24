import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
  <div class="page-head">
    <div><h2>تنبيهات انخفاض المخزون</h2><p>الأصناف التي وصلت للحد الأدنى أو أقل — راجعها وأعد الطلب قبل نفادها</p></div>
    <a routerLink="/stock" class="btn-ghost">إدارة المخزون</a>
  </div>
  <div class="panel">
    <table>
      <thead><tr><th>المنتج</th><th>SKU</th><th>المستودع</th><th>المتبقي</th><th>الحد الأدنى</th><th>العجز</th></tr></thead>
      <tbody>
        <tr *ngFor="let item of items">
          <td><span class="product-dot">⌁</span><b>{{item.productName}}</b></td>
          <td class="muted">{{item.sku}}</td>
          <td>{{item.warehouseName}}</td>
          <td><strong class="low-text">{{item.quantity}}</strong></td>
          <td>{{item.minimumStock}}</td>
          <td><span class="badge warn">{{item.minimumStock - item.quantity}} وحدة</span></td>
        </tr>
      </tbody>
    </table>
    <div class="empty" *ngIf="!items.length && !loading">لا توجد تنبيهات حالياً — كل الأصناف فوق الحد الأدنى.</div>
    <div class="empty" *ngIf="loading">جارٍ التحميل...</div>
  </div>`
})
export class LowStockComponent {
  api = inject(ApiService);
  items: any[] = [];
  loading = false;

  ngOnInit() {
    this.loading = true;
    this.api.lowStock().subscribe({
      next: x => { this.items = x ?? []; this.loading = false; },
      error: () => { this.items = []; this.loading = false; }
    });
  }
}
