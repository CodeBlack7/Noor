import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE } from '../api.config';
import { CheckoutRequest, OrderDetail, OrderSummary } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);

  placeOrder(request: CheckoutRequest): Observable<OrderDetail> {
    return this.http.post<OrderDetail>(`${API_BASE}/orders`, request);
  }

  myOrders(): Observable<OrderSummary[]> {
    return this.http.get<OrderSummary[]>(`${API_BASE}/orders`);
  }

  getOrder(id: string): Observable<OrderDetail> {
    return this.http.get<OrderDetail>(`${API_BASE}/orders/${id}`);
  }

  cancel(id: string): Observable<OrderDetail> {
    return this.http.post<OrderDetail>(`${API_BASE}/orders/${id}/cancel`, {});
  }
}
