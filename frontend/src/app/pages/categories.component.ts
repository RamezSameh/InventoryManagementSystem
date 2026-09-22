import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService, Category } from '../core/api.service';
import { AuthService } from '../core/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
  <div class="page-head">
    <div><h2>التصنيفات</h2><p>تنظيم المنتجات في تصنيفات</p></div>
    <button class="primary" *ngIf="canManage()" (click)="openAdd()">＋ إضافة تصنيف</button>
  </div>
  <div class="panel">
    <table>
      <thead><tr><th>الاسم</th><th>الوصف</th><th *ngIf="canManage()">إجراءات</th></tr></thead>
      <tbody>
        <tr *ngFor="let c of items">
          <td><span class="product-dot">▦</span><b>{{c.name}}</b></td>
          <td class="muted">{{c.description || '—'}}</td>
          <td *ngIf="canManage()">
            <div class="row-actions">
              <button class="btn-ghost" (click)="openEdit(c)">تعديل</button>
              <button class="btn-danger" (click)="remove(c)">حذف</button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
    <div class="empty" *ngIf="!items.length">لا توجد تصنيفات أو تعذر الاتصال بالخادم.</div>
  </div>

  <div class="modal-overlay" *ngIf="showForm" (click)="closeForm()">
    <div class="modal" (click)="$event.stopPropagation()">
      <h3>{{editing ? 'تعديل التصنيف' : 'إضافة تصنيف جديد'}}</h3>
      <p class="muted">أدخل بيانات التصنيف</p>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>اسم التصنيف<input formControlName="name" placeholder="مثال: مشروبات"></label>
        <label>الوصف <span class="optional">اختياري</span><input formControlName="description"></label>
        <p class="error" *ngIf="error">{{error}}</p>
        <div class="modal-actions">
          <button type="submit" class="primary" [disabled]="form.invalid||saving">{{saving?'جارٍ الحفظ...':'حفظ'}}</button>
          <button type="button" class="btn-secondary" (click)="closeForm()">إلغاء</button>
        </div>
      </form>
    </div>
  </div>`
})
export class CategoriesComponent {
  api = inject(ApiService);
  private auth = inject(AuthService);
  private fb = inject(FormBuilder);
  items: Category[] = [];
  showForm = false;
  editing: Category | null = null;
  saving = false;
  error = '';

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: ['']
  });

  canManage() { return this.auth.canManage(); }

  ngOnInit() { this.load(); }

  load() {
    this.api.categories().subscribe({ next: x => this.items = x ?? [], error: () => this.items = [] });
  }

  openAdd() {
    this.editing = null;
    this.error = '';
    this.form.reset({ name: '', description: '' });
    this.showForm = true;
  }

  openEdit(c: Category) {
    this.editing = c;
    this.error = '';
    this.form.reset({ name: c.name, description: c.description ?? '' });
    this.showForm = true;
  }

  closeForm() { this.showForm = false; }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true;
    this.error = '';
    const v = this.form.getRawValue();
    const payload = { name: v.name, description: v.description || undefined };
    const req = this.editing
      ? this.api.updateCategory(this.editing.id, payload)
      : this.api.createCategory(payload);
    req.subscribe({
      next: () => { this.saving = false; this.showForm = false; this.load(); },
      error: () => { this.saving = false; this.error = 'تعذر حفظ التصنيف. حاول مرة أخرى.'; }
    });
  }

  remove(c: Category) {
    if (!confirm(`حذف التصنيف "${c.name}"؟`)) return;
    this.api.deleteCategory(c.id).subscribe({ next: () => this.load(), error: () => alert('تعذر حذف التصنيف.') });
  }
}
