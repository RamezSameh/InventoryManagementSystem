import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService, Warehouse } from '../core/api.service';
import { AuthService } from '../core/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
  <div class="page-head">
    <div><h2>المستودعات</h2><p>إدارة مستودعات التخزين ومواقعها</p></div>
    <button class="primary" *ngIf="canManage()" (click)="openAdd()">＋ إضافة مستودع</button>
  </div>
  <div class="panel">
    <table>
      <thead><tr><th>الاسم</th><th>الموقع</th><th *ngIf="canManage()">إجراءات</th></tr></thead>
      <tbody>
        <tr *ngFor="let w of items">
          <td><span class="product-dot">▣</span><b>{{w.name}}</b></td>
          <td class="muted">{{w.location || '—'}}</td>
          <td *ngIf="canManage()">
            <div class="row-actions">
              <button class="btn-ghost" (click)="openEdit(w)">تعديل</button>
              <button class="btn-danger" (click)="remove(w)">حذف</button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    <div class="empty" *ngIf="!items.length">لا توجد مستودعات أو تعذر الاتصال بالخادم.</div>
  </div>

  <div class="modal-overlay" *ngIf="showForm" (click)="closeForm()">
    <div class="modal" (click)="$event.stopPropagation()">
      <h3>{{editing ? 'تعديل المستودع' : 'إضافة مستودع جديد'}}</h3>
      <p class="muted">أدخل بيانات المستودع</p>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>اسم المستودع<input formControlName="name" placeholder="مثال: المستودع الرئيسي"></label>
        <label>الموقع <span class="optional">اختياري</span><input formControlName="location" placeholder="مثال: الرياض - المنطقة الصناعية"></label>
        <p class="error" *ngIf="error">{{error}}</p>
        <div class="modal-actions">
          <button type="submit" class="primary" [disabled]="form.invalid||saving">{{saving?'جارٍ الحفظ...':'حفظ'}}</button>
          <button type="button" class="btn-secondary" (click)="closeForm()">إلغاء</button>
        </div>
      </form>
    </div>
  </div>`
})
export class WarehousesComponent {
  api = inject(ApiService);
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  items: Warehouse[] = [];
  showForm = false;
  editing: Warehouse | null = null;
  saving = false;
  error = '';

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    location: ['']
  });

  canManage() { return this.auth.canManage(); }

  ngOnInit() { this.load(); }

  load() {
    this.api.warehouses().subscribe({ next: x => this.items = x ?? [], error: () => this.items = [] });
  }

  openAdd() {
    this.editing = null;
    this.error = '';
    this.form.reset({ name: '', location: '' });
    this.showForm = true;
  }

  openEdit(w: Warehouse) {
    this.editing = w;
    this.error = '';
    this.form.reset({ name: w.name, location: w.location ?? '' });
    this.showForm = true;
  }

  closeForm() { this.showForm = false; }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true;
    this.error = '';
    const v = this.form.getRawValue();
    const payload = { name: v.name, location: v.location || undefined };
    const req = this.editing
      ? this.api.updateWarehouse(this.editing.id, payload)
      : this.api.createWarehouse(payload);
    req.subscribe({
      next: () => { this.saving = false; this.showForm = false; this.load(); },
      error: () => { this.saving = false; this.error = 'تعذر حفظ المستودع. حاول مرة أخرى.'; }
    });
  }

  remove(w: Warehouse) {
    if (!confirm(`حذف المستودع "${w.name}"؟`)) return;
    this.api.deleteWarehouse(w.id).subscribe({ next: () => this.load(), error: () => alert('تعذر حذف المستودع.') });
  }
}
