import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE } from '../api.config';
import {
  AdminCategory,
  AdminCustomer,
  AdminCustomerQuery,
  AdminOrderQuery,
  AdminProductListItem,
  AdminProductQuery,
  CategoryUpsertRequest,
  Coupon,
  CouponUpsertRequest,
  Dashboard,
  OrderDetail,
  OrderSummary,
  PagedResult,
  ProductDetail,
  ProductUpsertRequest,
  UpdateOrderStatusRequest,
  UploadResponse
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);

  getDashboard(): Observable<Dashboard> {
    return this.http.get<Dashboard>(`${API_BASE}/admin/dashboard`);
  }

  getProducts(query: AdminProductQuery): Observable<PagedResult<AdminProductListItem>> {
    let params = new HttpParams();
    if (query.search) params = params.set('search', query.search);
    if (query.categoryId) params = params.set('categoryId', query.categoryId);
    if (query.isActive !== undefined) params = params.set('isActive', query.isActive);
    if (query.page) params = params.set('page', query.page);
    if (query.pageSize) params = params.set('pageSize', query.pageSize);
    return this.http.get<PagedResult<AdminProductListItem>>(`${API_BASE}/admin/products`, { params });
  }

  getProduct(id: number): Observable<ProductDetail> {
    return this.http.get<ProductDetail>(`${API_BASE}/admin/products/${id}`);
  }

  createProduct(request: ProductUpsertRequest): Observable<ProductDetail> {
    return this.http.post<ProductDetail>(`${API_BASE}/admin/products`, request);
  }

  updateProduct(id: number, request: ProductUpsertRequest): Observable<ProductDetail> {
    return this.http.put<ProductDetail>(`${API_BASE}/admin/products/${id}`, request);
  }

  deleteProduct(id: number): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(
      `${API_BASE}/admin/products/${id}`
    );
  }

  getCategories(): Observable<AdminCategory[]> {
    return this.http.get<AdminCategory[]>(`${API_BASE}/admin/categories`);
  }

  createCategory(request: CategoryUpsertRequest): Observable<AdminCategory> {
    return this.http.post<AdminCategory>(`${API_BASE}/admin/categories`, request);
  }

  updateCategory(id: number, request: CategoryUpsertRequest): Observable<AdminCategory> {
    return this.http.put<AdminCategory>(`${API_BASE}/admin/categories/${id}`, request);
  }

  deleteCategory(id: number): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(
      `${API_BASE}/admin/categories/${id}`
    );
  }

  getOrders(query: AdminOrderQuery): Observable<PagedResult<OrderSummary>> {
    let params = new HttpParams();
    if (query.search) params = params.set('search', query.search);
    if (query.status) params = params.set('status', query.status);
    if (query.page) params = params.set('page', query.page);
    if (query.pageSize) params = params.set('pageSize', query.pageSize);
    return this.http.get<PagedResult<OrderSummary>>(`${API_BASE}/admin/orders`, { params });
  }

  getOrder(id: string): Observable<OrderDetail> {
    return this.http.get<OrderDetail>(`${API_BASE}/admin/orders/${id}`);
  }

  updateOrderStatus(id: string, request: UpdateOrderStatusRequest): Observable<OrderDetail> {
    return this.http.put<OrderDetail>(`${API_BASE}/admin/orders/${id}/status`, request);
  }

  getCoupons(): Observable<Coupon[]> {
    return this.http.get<Coupon[]>(`${API_BASE}/admin/coupons`);
  }

  createCoupon(request: CouponUpsertRequest): Observable<Coupon> {
    return this.http.post<Coupon>(`${API_BASE}/admin/coupons`, request);
  }

  updateCoupon(id: number, request: CouponUpsertRequest): Observable<Coupon> {
    return this.http.put<Coupon>(`${API_BASE}/admin/coupons/${id}`, request);
  }

  deleteCoupon(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE}/admin/coupons/${id}`);
  }

  getCustomers(query: AdminCustomerQuery): Observable<PagedResult<AdminCustomer>> {
    let params = new HttpParams();
    if (query.search) params = params.set('search', query.search);
    if (query.page) params = params.set('page', query.page);
    if (query.pageSize) params = params.set('pageSize', query.pageSize);
    return this.http.get<PagedResult<AdminCustomer>>(`${API_BASE}/admin/customers`, { params });
  }

  upload(file: File): Observable<UploadResponse> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<UploadResponse>(`${API_BASE}/admin/uploads`, form);
  }
}
