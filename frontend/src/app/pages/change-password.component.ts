import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '../core/api.service';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
  <div class="page-head">
    <div><h2>تغيير كلمة المرور</h2><p>كلمة المرور الجديدة لازم تكون 6 أحرف على الأقل وتحتوي حرف كبير وحرف صغير ورقم ورمز</p></div>
  </div>
  <div class="panel form-panel">
    <form [formGroup]="form" (ngSubmit)="submit()">
      <label>كلمة المرور الحالية<input type="password" formControlName="currentPassword"></label>
      <label>كلمة المرور الجديدة<input type="password" formControlName="newPassword"></label>
      <label>تأكيد كلمة المرور الجديدة<input type="password" formControlName="confirmPassword"></label>
      <p class="error" *ngIf="error">{{error}}</p>
      <p class="success" *ngIf="done">تم تغيير كلمة المرور بنجاح.</p>
      <div class="modal-actions">
        <button type="submit" class="primary" [disabled]="form.invalid||saving">{{saving?'جارٍ الحفظ...':'تغيير كلمة المرور'}}</button>
      </div>
    </form>
  </div>`
})
export class ChangePasswordComponent {
  api = inject(ApiService);
  private fb = inject(FormBuilder);
  saving = false;
  done = false;
  error = '';

  form = this.fb.nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required]
  });

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    const v = this.form.getRawValue();
    if (v.newPassword !== v.confirmPassword) {
      this.error = 'كلمة المرور الجديدة وتأكيدها غير متطابقين.';
      return;
    }
    this.saving = true;
    this.error = '';
    this.done = false;
    this.api.changePassword(v.currentPassword, v.newPassword).subscribe({
      next: () => { this.saving = false; this.done = true; this.form.reset(); },
      error: (e) => {
        this.saving = false;
        const desc = e?.error?.[0]?.description ?? e?.error?.error;
        this.error = desc ?? 'تعذر تغيير كلمة المرور. تحقق من كلمة المرور الحالية.';
      }
    });
  }
}
