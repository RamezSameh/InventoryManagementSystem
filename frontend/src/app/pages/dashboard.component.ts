import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, Product, DashboardSummary } from '../core/api.service';
import { RouterLink } from '@angular/router';

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
  <div class="stats">
    <div class="stat-card">
      <div class="stat-icon purple">▤</div>
      <div><span>إجمالي المنتجات</span><strong>{{summary?.totalProducts ?? (products.length || '—')}}</strong><small class="positive">محدث الآن</small></div>
    </div>
    <div class="stat-card">
      <div class="stat-icon green">◈</div>
      <div><span>قيمة المخزون</span><strong>{{(summary?.totalStockValue ?? value)|number:'1.0-0'}} <i>ر.س</i></strong><small class="positive">بأسعار الشراء</small></div>
    </div>
    <div class="stat-card">
      <div class="stat-icon orange">⌁</div>
      <div><span>منتجات منخفضة</span><strong>{{summary?.lowStockCount ?? low.length}}</strong><small class="negative">تحتاج إلى متابعة</small></div>
    </div>
    <div class="stat-card">
      <div class="stat-icon blue">◫</div>
      <div><span>حركات اليوم</span><strong>{{summary?.todayMovements ?? '—'}}</strong><small class="positive">عمليات مسجلة اليوم</small></div>
    </div>
  </div>
  <div class="section-grid">
    <div class="panel">
      <div class="panel-head"><div><h2>آخر المنتجات</h2><p>نظرة سريعة على المنتجات المضافة مؤخراً</p></div><a routerLink="/products">عرض الكل ←</a></div>
      <table>
        <thead><tr><th>المنتج</th><th>SKU</th><th>التصنيف</th><th>المخزون</th><th>السعر</th></tr></thead>
        <tbody>
          <tr *ngFor="let p of products.slice(0,5)">
            <td><span class="product-dot">▦</span><b>{{p.name}}</b></td>
            <td class="muted">{{p.sku}}</td>
            <td><span class="tag">{{p.categoryName}}</span></td>
            <td><strong [class.low-text]="p.totalStock<=p.minimumStock">{{p.totalStock}}</strong><small class="muted"> / {{p.minimumStock}} حد أدنى</small></td>
            <td>{{p.sellingPrice|number:'1.0-2'}} ر.س</td>
          </tr>
        </tbody>
      </table>
      <div class="empty" *ngIf="!products.length">لا توجد بيانات. شغّل الـ API ثم حدّث الصفحة.</div>
    </div>
    <div class="panel alert-panel">
      <div class="panel-head"><div><h2>تنبيهات المخزون</h2><p>منتجات تحتاج إلى إعادة طلب</p></div><span class="alert-count">{{summary?.lowStockCount ?? low.length}}</span></div>
      <div class="alert-item" *ngFor="let item of low.slice(0,4)">
        <div class="alert-icon">!</div>
        <div><b>{{item.productName}}</b><small>المتبقي {{item.quantity}} وحدات · الحد {{item.minimumStock}}</small></div>
        <span class="status-dot"></span>
      </div>
      <div class="empty" *ngIf="!low.length">لا توجد تنبيهات حالياً</div>
      <a class="outline full" routerLink="/stock">إدارة المخزون</a>
    </div>
  </div>`
})
export class DashboardComponent {
  api = inject(ApiService);
  products: Product[] = [];
  low: any[] = [];
  value = 0;
  summary: DashboardSummary | null = null;

  ngOnInit() {
    this.api.products().subscribe({
      next: x => { this.products = x ?? []; this.value = this.products.reduce((a, p) => a + p.totalStock * p.purchasePrice, 0); },
      error: () => this.products = []
    });
    this.api.lowStock().subscribe({ next: x => this.low = x ?? [], error: () => this.low = [] });
    this.api.dashboardSummary().subscribe({
      next: x => this.summary = x,
      error: () => this.summary = null // fallback: القيم المحسوبة محلياً أعلاه
    });
  }
}
