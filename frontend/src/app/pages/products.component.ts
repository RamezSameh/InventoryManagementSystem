import { Component, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ApiService, Product, Category } from '../core/api.service';
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
      <thead><tr><th>المنتج</th><th>SKU</th><th>التصنيف</th><th>سعر الشراء</th><th>سعر البيع</th><th>المخزون</th><th>الحالة</th></tr></thead>
      <tbody>
        <tr *ngFor="let p of products">
          <td><span class="product-dot">▦</span><b>{{p.name}}</b></td>
          <td>{{p.sku}}</td>
          <td><span class="tag">{{p.categoryName}}</span></td>
          <td>{{p.purchasePrice|number:'1.0-2'}} ر.س</td>
          <td>{{p.sellingPrice|number:'1.0-2'}} ر.س</td>
          <td><b>{{p.totalStock}}</b></td>
          <td><span class="badge" [class.warn]="p.totalStock<=p.minimumStock">{{p.totalStock<=p.minimumStock?'مخزون منخفض':'متوفر'}}</span></td>
        </tr>
      </tbody>
    </table>
    <div class="empty" *ngIf="!products.length">لا توجد منتجات أو تعذر الاتصال بالخادم.</div>
  </div>

  <div class="modal-overlay" *ngIf="showAdd" (click)="closeAdd()">
    <div class="modal" (click)="$event.stopPropagation()">
      <h3>إضافة منتج جديد</h3>
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
        <label>سعر الشراء<input type="number" formControlName="purchasePrice" min="0" step="0.01"></label>
        <label>سعر البيع<input type="number" formControlName="sellingPrice" min="0" step="0.01"></label>
        <label>الحد الأدنى للمخزون<input type="number" formControlName="minimumStock" min="0"></label>
        <label>الوصف <span class="optional">اختياري</span><input formControlName="description"></label>
        <p class="error" *ngIf="error">{{error}}</p>
        <div class="modal-actions">
          <button type="submit" class="primary" [disabled]="form.invalid||saving">{{saving?'جارٍ الحفظ...':'حفظ المنتج'}}</button>
          <button type="button" class="btn-secondary" (click)="closeAdd()">إلغاء</button>
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
  searchTerm = '';
  showAdd = false;
  saving = false;
  error = '';
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
    categoryId: [0, [Validators.required, Validators.min(1)]]
  });

  constructor() {
    this.sub = this.search$.pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(q => this.load(q));
  }

  canManage() { return this.auth.canManage(); }

  ngOnInit() {
    this.load();
    this.api.categories().subscribe({ next: x => this.categories = x ?? [], error: () => this.categories = [] });
  }

  ngOnDestroy() { this.sub.unsubscribe(); }

  load(search?: string) {
    this.api.products(search || undefined).subscribe({ next: x => this.products = x ?? [], error: () => this.products = [] });
  }

  onSearch(v: string) { this.search$.next(v); }

  openAdd() {
    this.error = '';
    this.form.reset({ name: '', sku: '', barcode: '', description: '', purchasePrice: 0, sellingPrice: 0, minimumStock: 0, categoryId: 0 });
    this.showAdd = true;
  }

  closeAdd() { this.showAdd = false; }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true;
    this.error = '';
    this.api.createProduct(this.form.getRawValue()).subscribe({
      next: () => { this.saving = false; this.showAdd = false; this.load(this.searchTerm || undefined); },
      error: () => { this.saving = false; this.error = 'تعذر حفظ المنتج. تحقق من البيانات (قد يكون SKU مكرراً).'; }
    });
  }
}
