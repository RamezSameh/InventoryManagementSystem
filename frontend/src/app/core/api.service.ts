import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Product {
  id: number; name: string; sku: string; barcode?: string;
  purchasePrice: number; sellingPrice: number; minimumStock: number;
  categoryName: string; totalStock: number;
}
export interface Category { id: number; name: string; description?: string | null }
export interface Warehouse { id: number; name: string; location?: string | null }
export interface LoginResponse { token: string }
export interface DashboardSummary {
  totalProducts: number; totalStockValue: number;
  lowStockCount: number; todayMovements: number;
}
export interface StockMovement {
  id: number; productId: number; productName?: string;
  warehouseId: number; warehouseName?: string;
  quantity: number; type: number; reference?: string | null;
  date: string; notes?: string | null;
}
export interface CreateProductPayload {
  name: string; sku: string; barcode?: string; description?: string;
  purchasePrice: number; sellingPrice: number; minimumStock: number; categoryId: number;
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

  products(search?: string): Observable<Product[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    return this.http.get<Product[]>(`${this.base}/products`, { params });
  }

  createProduct(payload: CreateProductPayload) {
    return this.http.post(`${this.base}/products`, payload);
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
}
