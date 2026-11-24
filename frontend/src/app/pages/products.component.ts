import { Component, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ApiService, Product, Category, Supplier } from '../core/api.service';
import { AuthService } from '../core/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  template: `
  <div class="page-head">
    <div><h2>المنتجات</h2><p>إدارة جميع المنتجات والأصناف في المخزون</p></div>
    <button class="primary" *ngIf="canManage()" (click)="openAdd()">＋ إضافة منتج</button>
  </div>
  <div class="panel">
    <div class="toolbar">
      <div class="search">⌕ <input placeholder="ابحث باسم المنتج أو SKU..." [(ngModel)]="searchTerm" (ngModelChange)="onSearch($event)"></div>
    </div>
    <table>
      <thead><tr><th>المنتج</th><th>SKU</th><th>التصنيف</th><th>المورد</th><th>سعر الشراء</th><th>سعر البيع</th><th>المخزون</th><th>الحالة</th><th *ngIf="canManage()">إجراءات</th></tr></thead>
      <tbody>
        <tr *ngFor="let p of products">
          <td><span class="product-dot">▦</span><b>{{p.name}}</b></td>
          <td>{{p.sku}}</td>
          <td><span class="tag">{{p.categoryName}}</span></td>
          <td class="muted">{{p.supplierName || '—'}}</td>
          <td>{{p.purchasePrice|number:'1.0-2'}} ر.س</td>
          <td>{{p.sellingPrice|number:'1.0-2'}} ر.س</td>
          <td><b>{{p.totalStock}}</b></td>
          <td><span class="badge" [class.warn]="p.totalStock<=p.minimumStock">{{p.totalStock<=p.minimumStock?'مخزون منخفض':'متوفر'}}</span></td>
          <td *ngIf="canManage()">
            <div class="row-actions">
              <button class="btn-ghost" (click)="openEdit(p)">تعديل</button>
              <button class="btn-danger" (click)="remove(p)">حذف</button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    <div class="empty" *ngIf="!products.length">لا توجد منتجات أو تعذر الاتصال بالخادم.</div>
    <div class="pager" *ngIf="total > pageSize">
      <button class="btn-ghost" [disabled]="page<=1" (click)="goPage(page-1)">→ السابق</button>
      <span>صفحة {{page}} من {{totalPages}}</span>
      <button class="btn-ghost" [disabled]="page>=totalPages" (click)="goPage(page+1)">التالي ←</button>
    </div>
  </div>

  <div class="modal-overlay" *ngIf="showForm" (click)="closeForm()">
    <div class="modal" (click)="$event.stopPropagation()">
      <h3>{{editing ? 'تعديل المنتج' : 'إضافة منتج جديد'}}</h3>
      <p class="muted">أدخل بيانات المنتج الأساسية</p>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>اسم المنتج<input formControlName="name" placeholder="مثال: قهوة مختصة"></label>
        <label>SKU<input formControlName="sku" placeholder="مثال: COF-001" dir="ltr"></label>
        <label>الباركود <span class="optional">اختياري</span><input formControlName="barcode" dir="ltr"></label>
        <label>التصنيف
          <select formControlName="categoryId">
            <option [ngValue]="0" disabled>اختر التصنيف</option>
            <option *ngFor="let c of categories" [ngValue]="c.id">{{c.name}}</option>
          </select>
        </label>
        <label>المورد <span class="optional">اختياري</span>
          <select formControlName="supplierId">
            <option [ngValue]="0">بدون مورد</option>
            <option *ngFor="let s of suppliers" [ngValue]="s.id">{{s.name}}</option>
          </select>
        </label>
        <label>سعر الشراء<input type="number" formControlName="purchasePrice" min="0" step="0.01"></label>
        <label>سعر البيع<input type="number" formControlName="sellingPrice" min="0" step="0.01"></label>
        <label>الحد الأدنى للمخزون<input type="number" formControlName="minimumStock" min="0"></label>
        <label>الوصف <span class="optional">اختياري</span><input formControlName="description"></label>
        <p class="error" *ngIf="error">{{error}}</p>
        <div class="modal-actions">
          <button type="submit" class="primary" [disabled]="form.invalid||saving">{{saving?'جارٍ الحفظ...':'حفظ المنتج'}}</button>
          <button type="button" class="btn-secondary" (click)="closeForm()">إلغاء</button>
        </div>
      </form>
    </div>
  </div>`
})
export class ProductsComponent implements OnDestroy {
  api = inject(ApiService);
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  products: Product[] = [];
  categories: Category[] = [];
  suppliers: Supplier[] = [];
  searchTerm = '';
  showForm = false;
  editing: Product | null = null;
  saving = false;
  error = '';
  page = 1;
  pageSize = 20;
  total = 0;
  private search$ = new Subject<string>();
  private sub: Subscription;

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    sku: ['', Validators.required],
    barcode: [''],
    description: [''],
    purchasePrice: [0, [Validators.required, Validators.min(0)]],
    sellingPrice: [0, [Validators.required, Validators.min(0)]],
    minimumStock: [0, [Validators.required, Validators.min(0)]],
    categoryId: [0, [Validators.required, Validators.min(1)]],
    supplierId: [0]
  });

  constructor() {
    this.sub = this.search$.pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(() => { this.page = 1; this.load(); });
  }

  canManage() { return this.auth.canManage(); }

  get totalPages() { return Math.max(1, Math.ceil(this.total / this.pageSize)); }

  ngOnInit() {
    this.load();
    this.api.categories().subscribe({ next: x => this.categories = x ?? [], error: () => this.categories = [] });
    this.api.suppliers().subscribe({ next: x => this.suppliers = x ?? [], error: () => this.suppliers = [] });
  }

  ngOnDestroy() { this.sub.unsubscribe(); }

  load() {
    this.api.productsPaged(this.searchTerm || undefined, this.page, this.pageSize).subscribe({
      next: x => { this.products = x?.items ?? []; this.total = x?.total ?? 0; },
      error: () => { this.products = []; this.total = 0; }
    });
  }

  onSearch(v: string) { this.search$.next(v); }

  goPage(p: number) { this.page = p; this.load(); }

  openAdd() {
    this.editing = null;
    this.error = '';
    this.form.reset({ name: '', sku: '', barcode: '', description: '', purchasePrice: 0, sellingPrice: 0, minimumStock: 0, categoryId: 0, supplierId: 0 });
    this.showForm = true;
  }

  openEdit(p: Product) {
    this.editing = p;
    this.error = '';
    this.form.reset({
      name: p.name, sku: p.sku, barcode: p.barcode ?? '', description: p.description ?? '',
      purchasePrice: p.purchasePrice, sellingPrice: p.sellingPrice, minimumStock: p.minimumStock,
      categoryId: p.categoryId, supplierId: p.supplierId ?? 0
    });
    this.showForm = true;
  }

  closeForm() { this.showForm = false; }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true;
    this.error = '';
    const v = this.form.getRawValue();
    const payload = {
      name: v.name, sku: v.sku, barcode: v.barcode || undefined, description: v.description || undefined,
      purchasePrice: v.purchasePrice, sellingPrice: v.sellingPrice, minimumStock: v.minimumStock,
      categoryId: v.categoryId, supplierId: v.supplierId || null
    };
    const req = this.editing
      ? this.api.updateProduct(this.editing.id, payload)
      : this.api.createProduct(payload);
    req.subscribe({
      next: () => { this.saving = false; this.showForm = false; this.load(); },
      error: () => { this.saving = false; this.error = 'تعذر حفظ المنتج. تحقق من البيانات (قد يكون SKU مكرراً).'; }
    });
  }

  remove(p: Product) {
    if (!confirm(`حذف المنتج "${p.name}"؟`)) return;
    this.api.deleteProduct(p.id).subscribe({
      next: () => {
        if (this.products.length === 1 && this.page > 1) this.page--;
        this.load();
      },
      error: () => alert('تعذر حذف المنتج.')
    });
  }
}
