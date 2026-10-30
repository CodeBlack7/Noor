import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE } from '../api.config';
import { Address, SaveAddressRequest } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly http = inject(HttpClient);

  getAddresses(): Observable<Address[]> {
    return this.http.get<Address[]>(`${API_BASE}/addresses`);
  }

  createAddress(request: SaveAddressRequest): Observable<Address> {
    return this.http.post<Address>(`${API_BASE}/addresses`, request);
  }

  updateAddress(id: number, request: SaveAddressRequest): Observable<Address> {
    return this.http.put<Address>(`${API_BASE}/addresses/${id}`, request);
  }

  deleteAddress(id: number): Observable<void> {
    return this.http.delete<void>(`${API_BASE}/addresses/${id}`);
  }

  setDefaultAddress(id: number): Observable<void> {
    return this.http.post<void>(`${API_BASE}/addresses/${id}/default`, {});
  }
}
