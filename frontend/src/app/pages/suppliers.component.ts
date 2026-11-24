import { Component, inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ApiService, Supplier } from '../core/api.service';
import { AuthService } from '../core/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  template: `
  <div class="page-head">
    <div><h2>الموردون</h2><p>إدارة الموردين وبيانات التواصل معهم</p></div>
    <button class="primary" *ngIf="canManage()" (click)="openAdd()">＋ إضافة مورد</button>
  </div>
  <div class="panel">
    <div class="toolbar">
      <div class="search">⌕ <input placeholder="ابحث باسم المورد..." [(ngModel)]="searchTerm" (ngModelChange)="onSearch($event)"></div>
    </div>
    <table>
      <thead><tr><th>الاسم</th><th>الهاتف</th><th>البريد الإلكتروني</th><th>العنوان</th><th *ngIf="canManage()">إجراءات</th></tr></thead>
      <tbody>
        <tr *ngFor="let s of items">
          <td><span class="product-dot">▣</span><b>{{s.name}}</b></td>
          <td class="muted" dir="ltr">{{s.phone || '—'}}</td>
          <td class="muted" dir="ltr">{{s.email || '—'}}</td>
          <td class="muted">{{s.address || '—'}}</td>
          <td *ngIf="canManage()">
            <div class="row-actions">
              <button class="btn-ghost" (click)="openEdit(s)">تعديل</button>
              <button class="btn-danger" (click)="remove(s)">حذف</button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    <div class="empty" *ngIf="!items.length">لا توجد موردون أو تعذر الاتصال بالخادم.</div>
  </div>

  <div class="modal-overlay" *ngIf="showForm" (click)="closeForm()">
    <div class="modal" (click)="$event.stopPropagation()">
      <h3>{{editing ? 'تعديل المورد' : 'إضافة مورد جديد'}}</h3>
      <p class="muted">أدخل بيانات المورد</p>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>اسم المورد<input formControlName="name" placeholder="مثال: شركة التقنية"></label>
        <label>الهاتف <span class="optional">اختياري</span><input formControlName="phone" dir="ltr"></label>
        <label>البريد الإلكتروني <span class="optional">اختياري</span><input formControlName="email" type="email" dir="ltr"></label>
        <label>العنوان <span class="optional">اختياري</span><input formControlName="address"></label>
        <p class="error" *ngIf="error">{{error}}</p>
        <div class="modal-actions">
          <button type="submit" class="primary" [disabled]="form.invalid||saving">{{saving?'جارٍ الحفظ...':'حفظ'}}</button>
          <button type="button" class="btn-secondary" (click)="closeForm()">إلغاء</button>
        </div>
      </form>
    </div>
  </div>`
})
export class SuppliersComponent implements OnDestroy {
  api = inject(ApiService);
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  items: Supplier[] = [];
  searchTerm = '';
  showForm = false;
  editing: Supplier | null = null;
  saving = false;
  error = '';
  private search$ = new Subject<string>();
  private sub: Subscription;

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    phone: [''],
    email: ['', Validators.email],
    address: ['']
  });

  constructor() {
    this.sub = this.search$.pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(q => this.load(q));
  }

  canManage() { return this.auth.canManage(); }

  ngOnInit() { this.load(); }

  ngOnDestroy() { this.sub.unsubscribe(); }

  load(search?: string) {
    this.api.suppliers(search || undefined).subscribe({ next: x => this.items = x ?? [], error: () => this.items = [] });
  }

  onSearch(v: string) { this.search$.next(v); }

  openAdd() {
    this.editing = null;
    this.error = '';
    this.form.reset({ name: '', phone: '', email: '', address: '' });
    this.showForm = true;
  }

  openEdit(s: Supplier) {
    this.editing = s;
    this.error = '';
    this.form.reset({ name: s.name, phone: s.phone ?? '', email: s.email ?? '', address: s.address ?? '' });
    this.showForm = true;
  }

  closeForm() { this.showForm = false; }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true;
    this.error = '';
    const v = this.form.getRawValue();
    const payload = {
      name: v.name,
      phone: v.phone || undefined,
      email: v.email || undefined,
      address: v.address || undefined
    };
    const req = this.editing
      ? this.api.updateSupplier(this.editing.id, payload)
      : this.api.createSupplier(payload);
    req.subscribe({
      next: () => { this.saving = false; this.showForm = false; this.load(this.searchTerm || undefined); },
      error: () => { this.saving = false; this.error = 'تعذر حفظ المورد. حاول مرة أخرى.'; }
    });
  }

  remove(s: Supplier) {
    if (!confirm(`حذف المورد "${s.name}"؟`)) return;
    this.api.deleteSupplier(s.id).subscribe({ next: () => this.load(this.searchTerm || undefined), error: () => alert('تعذر حذف المورد.') });
  }
}
