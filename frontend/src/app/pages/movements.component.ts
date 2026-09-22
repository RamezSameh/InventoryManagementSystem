import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, StockMovement, Product, Warehouse } from '../core/api.service';

const TYPE_LABELS: Record<number, string> = {
  1: 'شراء - إدخال', 2: 'بيع - إخراج', 3: 'تحويل - إدخال',
  4: 'تحويل - إخراج', 5: 'تسوية', 6: 'مرتجع'
};

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <div class="page-head">
    <div><h2>سجل حركات المخزون</h2><p>استعرض جميع عمليات الدخول والخروج والتحويلات</p></div>
  </div>
  <div class="panel">
    <div class="filter-row">
      <label>المنتج
        <select [(ngModel)]="f.productId" (ngModelChange)="resetAndLoad()">
          <option [ngValue]="0">كل المنتجات</option>
          <option *ngFor="let p of products" [ngValue]="p.id">{{p.name}}</option>
        </select>
      </label>
      <label>المستودع
        <select [(ngModel)]="f.warehouseId" (ngModelChange)="resetAndLoad()">
          <option [ngValue]="0">كل المستودعات</option>
          <option *ngFor="let w of warehouses" [ngValue]="w.id">{{w.name}}</option>
        </select>
      </label>
      <label>نوع الحركة
        <select [(ngModel)]="f.type" (ngModelChange)="resetAndLoad()">
          <option [ngValue]="0">كل الأنواع</option>
          <option *ngFor="let t of typeOptions" [ngValue]="t.value">{{t.label}}</option>
        </select>
      </label>
      <label>من تاريخ<input type="date" [(ngModel)]="f.from" (ngModelChange)="resetAndLoad()"></label>
      <label>إلى تاريخ<input type="date" [(ngModel)]="f.to" (ngModelChange)="resetAndLoad()"></label>
      <label>&nbsp;<button class="btn-ghost" (click)="clearFilters()">مسح الفلاتر</button></label>
    </div>
    <table>
      <thead><tr><th>التاريخ</th><th>المنتج</th><th>المستودع</th><th>النوع</th><th>الكمية</th><th>المرجع</th></tr></thead>
      <tbody>
        <tr *ngFor="let m of items">
          <td class="muted">{{fmtDate(m.date)}}</td>
          <td><b>{{m.productName || ('#' + m.productId)}}</b></td>
          <td>{{m.warehouseName || ('#' + m.warehouseId)}}</td>
          <td><span class="tag">{{typeLabel(m.type)}}</span></td>
          <td><b>{{m.quantity}}</b></td>
          <td class="muted">{{m.reference || '—'}}</td>
        </tr>
      </tbody>
    </table>
    <div class="empty" *ngIf="!items.length && !loading">لا توجد حركات مطابقة أو تعذر الاتصال بالخادم.</div>
    <div class="empty" *ngIf="loading">جارٍ التحميل...</div>
    <div class="pager" *ngIf="total > pageSize">
      <button class="btn-ghost" [disabled]="page<=1" (click)="goPage(page-1)">→ السابق</button>
      <span>صفحة {{page}} من {{totalPages}}</span>
      <button class="btn-ghost" [disabled]="page>=totalPages" (click)="goPage(page+1)">التالي ←</button>
    </div>
  </div>`
})
export class MovementsComponent {
  api = inject(ApiService);
  items: StockMovement[] = [];
  products: Product[] = [];
  warehouses: Warehouse[] = [];
  typeOptions = Object.entries(TYPE_LABELS).map(([value, label]) => ({ value: +value, label }));
  loading = false;
  page = 1;
  pageSize = 20;
  total = 0;
  f = { productId: 0, warehouseId: 0, type: 0, from: '', to: '' };

  get totalPages() { return Math.max(1, Math.ceil(this.total / this.pageSize)); }

  typeLabel(t: number) { return TYPE_LABELS[t] ?? ('نوع ' + t); }

  fmtDate(d: string) {
    try { return new Date(d).toLocaleString('ar-EG', { dateStyle: 'medium', timeStyle: 'short' }); }
    catch { return d; }
  }

  ngOnInit() {
    this.api.products().subscribe({ next: x => this.products = x ?? [], error: () => this.products = [] });
    this.api.warehouses().subscribe({ next: x => this.warehouses = x ?? [], error: () => this.warehouses = [] });
    this.load();
  }

  resetAndLoad() { this.page = 1; this.load(); }

  clearFilters() {
    this.f = { productId: 0, warehouseId: 0, type: 0, from: '', to: '' };
    this.resetAndLoad();
  }

  goPage(p: number) { this.page = p; this.load(); }

  load() {
    this.loading = true;
    this.api.movements({
      productId: this.f.productId || undefined,
      warehouseId: this.f.warehouseId || undefined,
      type: this.f.type || undefined,
      from: this.f.from || undefined,
      to: this.f.to || undefined,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: res => {
        const n = this.normalize(res);
        this.items = n.items;
        this.total = n.total;
        this.loading = false;
      },
      error: () => { this.items = []; this.total = 0; this.loading = false; }
    });
  }

  /** يقبل شكل الاستجابة سواء كان مصفوفة أو كائن صفحات {items,total} */
  private normalize(res: unknown): { items: StockMovement[]; total: number } {
    if (Array.isArray(res)) return { items: res as StockMovement[], total: (res as unknown[]).length };
    const r = res as { items?: StockMovement[]; total?: number } | null;
    const items = r?.items ?? [];
    return { items, total: r?.total ?? items.length };
  }
}
