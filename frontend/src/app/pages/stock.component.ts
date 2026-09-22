import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService, Product, Warehouse } from '../core/api.service';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
  <div class="page-head"><div><h2>حركة المخزون</h2><p>إضافة ومتابعة عمليات دخول وخروج المنتجات</p></div></div>
  <div class="stock-layout">
    <div class="panel form-panel">
      <h3>إضافة حركة جديدة</h3>
      <p class="muted">سجّل عملية شراء أو بيع أو تحويل أو تسوية للمخزون</p>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>المنتج
          <select formControlName="productId">
            <option [ngValue]="0" disabled>اختر المنتج</option>
            <option *ngFor="let p of products" [ngValue]="p.id">{{p.name}} ({{p.sku}})</option>
          </select>
        </label>
        <label>المستودع
          <select formControlName="warehouseId">
            <option [ngValue]="0" disabled>اختر المستودع</option>
            <option *ngFor="let w of warehouses" [ngValue]="w.id">{{w.name}}</option>
          </select>
        </label>
        <label>نوع الحركة
          <select formControlName="type">
            <option [ngValue]="1">شراء - إدخال</option>
            <option [ngValue]="2">بيع - إخراج</option>
            <option [ngValue]="4">تحويل - إخراج لمستودع آخر</option>
            <option [ngValue]="5">تسوية</option>
            <option [ngValue]="6">مرتجع</option>
          </select>
        </label>
        <label *ngIf="isTransferOut()">المستودع الوجهة
          <select formControlName="destinationWarehouseId">
            <option [ngValue]="null" disabled>اختر مستودع الوجهة</option>
            <option *ngFor="let w of warehouses" [ngValue]="w.id" [disabled]="w.id===form.controls.warehouseId.value">{{w.name}}</option>
          </select>
        </label>
        <label>الكمية<input type="number" formControlName="quantity" min="1"></label>
        <label>المرجع <span class="optional">اختياري</span><input formControlName="reference" placeholder="رقم الفاتورة أو المرجع"></label>
        <button class="primary full" [disabled]="form.invalid">حفظ الحركة</button>
        <p class="success" *ngIf="success">تم حفظ الحركة بنجاح</p>
        <p class="error" *ngIf="error">تعذر حفظ الحركة. تحقق من البيانات والكمية المتاحة.</p>
      </form>
    </div>
    <div class="panel">
      <div class="panel-head"><div><h3>منتجات منخفضة المخزون</h3><p>تحتاج إلى إعادة طلب</p></div><span class="alert-count">{{low.length}}</span></div>
      <div class="alert-item" *ngFor="let x of low">
        <div class="alert-icon">!</div>
        <div><b>{{x.productName}}</b><small>{{x.warehouseName}} · متبقي {{x.quantity}}</small></div>
      </div>
      <div class="empty" *ngIf="!low.length">المخزون ضمن المستويات الطبيعية</div>
    </div>
  </div>`
})
export class StockComponent {
  private fb = inject(FormBuilder);
  api = inject(ApiService);
  low: any[] = [];
  products: Product[] = [];
  warehouses: Warehouse[] = [];
  success = false;
  error = false;

  form = this.fb.group({
    productId: [0, [Validators.required, Validators.min(1)]],
    warehouseId: [0, [Validators.required, Validators.min(1)]],
    type: [1, Validators.required],
    quantity: [1, [Validators.required, Validators.min(1)]],
    reference: [''],
    destinationWarehouseId: [null as number | null]
  });

  isTransferOut() { return this.form.controls.type.value === 4; }

  ngOnInit() {
    this.api.lowStock().subscribe({ next: x => this.low = x ?? [], error: () => this.low = [] });
    this.api.products().subscribe({ next: x => this.products = x ?? [], error: () => this.products = [] });
    this.api.warehouses().subscribe({ next: x => this.warehouses = x ?? [], error: () => this.warehouses = [] });
    this.form.controls.type.valueChanges.subscribe(t => {
      const c = this.form.controls.destinationWarehouseId;
      if (t === 4) c.setValidators([Validators.required, Validators.min(1)]);
      else { c.clearValidators(); c.setValue(null); }
      c.updateValueAndValidity();
    });
  }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.success = false;
    this.error = false;
    const v = this.form.value;
    const payload: Record<string, unknown> = {
      productId: v.productId,
      warehouseId: v.warehouseId,
      quantity: v.quantity,
      type: v.type,
      reference: v.reference || null
    };
    if (v.type === 4) payload['destinationWarehouseId'] = v.destinationWarehouseId;
    this.api.addMovement(payload).subscribe({
      next: () => {
        this.success = true;
        this.form.patchValue({ quantity: 1, reference: '', destinationWarehouseId: null });
        this.api.lowStock().subscribe({ next: x => this.low = x ?? [], error: () => undefined });
      },
      error: () => this.error = true
    });
  }
}
