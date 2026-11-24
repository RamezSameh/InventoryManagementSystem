import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Product {
  id: number; name: string; sku: string; barcode?: string;
  purchasePrice: number; sellingPrice: number; minimumStock: number;
  categoryId: number; categoryName: string; totalStock: number;
  description?: string | null; supplierId?: number | null; supplierName?: string | null;
}
export interface Category { id: number; name: string; description?: string | null }
export interface Warehouse { id: number; name: string; location?: string | null }
export interface Supplier { id: number; name: string; phone?: string | null; email?: string | null; address?: string | null }
export interface LoginResponse { token: string; refreshToken: string }
export interface PagedResult<T> { items: T[]; total: number; page: number; pageSize: number }
export interface DashboardSummary {
  totalProducts: number; totalStockValue: number;
  lowStockCount: number; todayMovements: number;
}
export interface StockMovement {
  id: number; productId: number; productName?: string;
  warehouseId: number; warehouseName?: string;
  quantity: number; type: number; reference?: string | null;
  date: string; notes?: string | null; createdBy?: string | null;
}
export interface UserDto {
  id: string; email: string; fullName: string;
  roles: string[]; isLockedOut: boolean; createdAt: string;
}
export interface InventoryReport {
  byCategory: { categoryName: string; productCount: number; totalQuantity: number; totalValue: number }[];
  byWarehouse: { warehouseName: string; productCount: number; totalQuantity: number; totalValue: number }[];
  totalValue: number;
}

/** ينزّل ملف CSV جاي من الـ API (blob) باسم معيّن. */
export function downloadCsv(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}
export interface CreateProductPayload {
  name: string; sku: string; barcode?: string; description?: string;
  purchasePrice: number; sellingPrice: number; minimumStock: number; categoryId: number;
  supplierId?: number | null;
}
export interface MovementFilter {
  productId?: number; warehouseId?: number; type?: number;
  from?: string; to?: string; page?: number; pageSize?: number;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  login(email: string, password: string) {
    return this.http.post<LoginResponse>(`${this.base}/auth/login`, { email, password });
  }

  refresh(refreshToken: string) {
    return this.http.post<LoginResponse>(`${this.base}/auth/refresh`, { refreshToken });
  }

  changePassword(currentPassword: string, newPassword: string) {
    return this.http.post(`${this.base}/auth/change-password`, { currentPassword, newPassword });
  }

  users(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(`${this.base}/users`);
  }
  changeUserRole(id: string, role: string) {
    return this.http.put(`${this.base}/users/${id}/role`, { role });
  }
  setUserLockout(id: string, lockedOut: boolean) {
    return this.http.put(`${this.base}/users/${id}/lockout`, { lockedOut });
  }

  /** أول صفحة فقط — للقوائم المنسدلة في باقي الصفحات.
   * الـ API بيرجّع كائن صفحات {items,total,...} من المرحلة 2، فبنفكّ الـ items
   * هنا عشان كل اللي بيستخدم products() يفضل شايف مصفوفة زي الأول. */
  products(search?: string): Observable<Product[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<Product> | Product[]>(`${this.base}/products`, { params }).pipe(
      map(r => Array.isArray(r) ? r : (r.items ?? []))
    );
  }

  /** صفحة منتجات كاملة مع العدد الإجمالي */
  productsPaged(search?: string, page = 1, pageSize = 20): Observable<PagedResult<Product>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<Product>>(`${this.base}/products`, { params });
  }

  productById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.base}/products/${id}`);
  }

  createProduct(payload: CreateProductPayload) {
    return this.http.post(`${this.base}/products`, payload);
  }
  updateProduct(id: number, payload: CreateProductPayload) {
    return this.http.put(`${this.base}/products/${id}`, { id, ...payload });
  }
  deleteProduct(id: number) {
    return this.http.delete(`${this.base}/products/${id}`);
  }

  categories(): Observable<Category[]> {
    return this.http.get<Category[]>(`${this.base}/categories`);
  }
  createCategory(payload: { name: string; description?: string }) {
    return this.http.post(`${this.base}/categories`, payload);
  }
  updateCategory(id: number, payload: { name: string; description?: string }) {
    return this.http.put(`${this.base}/categories/${id}`, { id, ...payload });
  }
  deleteCategory(id: number) {
    return this.http.delete(`${this.base}/categories/${id}`);
  }

  suppliers(search?: string): Observable<Supplier[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<Supplier[]>(`${this.base}/suppliers`, { params });
  }
  createSupplier(payload: { name: string; phone?: string; email?: string; address?: string }) {
    return this.http.post(`${this.base}/suppliers`, payload);
  }
  updateSupplier(id: number, payload: { name: string; phone?: string; email?: string; address?: string }) {
    return this.http.put(`${this.base}/suppliers/${id}`, { id, ...payload });
  }
  deleteSupplier(id: number) {
    return this.http.delete(`${this.base}/suppliers/${id}`);
  }

  warehouses(): Observable<Warehouse[]> {
    return this.http.get<Warehouse[]>(`${this.base}/warehouses`);
  }
  createWarehouse(payload: { name: string; location?: string }) {
    return this.http.post(`${this.base}/warehouses`, payload);
  }
  updateWarehouse(id: number, payload: { name: string; location?: string }) {
    return this.http.put(`${this.base}/warehouses/${id}`, { id, ...payload });
  }
  deleteWarehouse(id: number) {
    return this.http.delete(`${this.base}/warehouses/${id}`);
  }

  lowStock() {
    return this.http.get<any[]>(`${this.base}/stock/low-stock`);
  }

  addMovement(payload: any) {
    return this.http.post(`${this.base}/stock/movement`, payload);
  }

  movements(f: MovementFilter): Observable<unknown> {
    let params = new HttpParams();
    if (f.productId) params = params.set('productId', f.productId);
    if (f.warehouseId) params = params.set('warehouseId', f.warehouseId);
    if (f.type) params = params.set('type', f.type);
    if (f.from) params = params.set('from', f.from);
    if (f.to) params = params.set('to', f.to);
    params = params.set('page', f.page ?? 1).set('pageSize', f.pageSize ?? 20);
    return this.http.get(`${this.base}/stock/movements`, { params });
  }

  dashboardSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.base}/dashboard/summary`);
  }

  inventoryReport(): Observable<InventoryReport> {
    return this.http.get<InventoryReport>(`${this.base}/dashboard/inventory-report`);
  }

  exportProducts(search?: string) {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get(`${this.base}/products/export`, { params, responseType: 'blob' });
  }

  exportMovements(f: MovementFilter) {
    let params = new HttpParams();
    if (f.productId) params = params.set('productId', f.productId);
    if (f.warehouseId) params = params.set('warehouseId', f.warehouseId);
    if (f.type) params = params.set('type', f.type);
    if (f.from) params = params.set('from', f.from);
    if (f.to) params = params.set('to', f.to);
    return this.http.get(`${this.base}/stock/movements/export`, { params, responseType: 'blob' });
  }
}
