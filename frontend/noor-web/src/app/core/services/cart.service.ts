import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { API_BASE } from '../api.config';
import { Cart } from '../models/api.models';
import { SessionService } from './session.service';
import { ToastService } from './toast.service';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly session = inject(SessionService);
  private readonly toast = inject(ToastService);

  readonly cart = signal<Cart | null>(null);
  readonly drawerOpen = signal(false);
  readonly busy = signal(false);

  readonly itemCount = computed(() => this.cart()?.itemCount ?? 0);
  readonly subtotal = computed(() => this.cart()?.subtotal ?? 0);
  readonly isEmpty = computed(() => this.itemCount() === 0);

  load(): void {
    this.http.get<Cart>(`${API_BASE}/cart`).subscribe({
      next: (cart) => this.cart.set(cart),
      error: () => undefined
    });
  }

  add(variantId: number, quantity = 1, openDrawer = true): void {
    this.busy.set(true);
    this.http
      .post<Cart>(`${API_BASE}/cart/items`, { variantId, quantity })
      .subscribe({
        next: (cart) => {
          this.cart.set(cart);
          this.busy.set(false);
          if (openDrawer) this.drawerOpen.set(true);
          this.toast.show('Added to your bag.');
        },
        error: () => this.busy.set(false)
      });
  }

  update(itemId: number, quantity: number): void {
    if (quantity <= 0) {
      this.remove(itemId);
      return;
    }
    this.http
      .put<Cart>(`${API_BASE}/cart/items/${itemId}`, { quantity })
      .subscribe({ next: (cart) => this.cart.set(cart) });
  }

  remove(itemId: number): void {
    this.http
      .delete<Cart>(`${API_BASE}/cart/items/${itemId}`)
      .subscribe({
        next: (cart) => {
          this.cart.set(cart);
          this.toast.show('Removed from your bag.', 'info');
        }
      });
  }

  applyCoupon(code: string): Observable<Cart> {
    return this.http
      .post<Cart>(`${API_BASE}/cart/coupon`, { code })
      .pipe(tap((cart) => this.cart.set(cart)));
  }

  removeCoupon(): Observable<Cart> {
    return this.http
      .delete<Cart>(`${API_BASE}/cart/coupon`)
      .pipe(tap((cart) => this.cart.set(cart)));
  }

  mergeGuestCart(): Observable<Cart> {
    return this.http
      .post<Cart>(`${API_BASE}/cart/merge`, { guestKey: this.session.cartKey() })
      .pipe(tap((cart) => this.cart.set(cart)));
  }

  reset(): void {
    this.cart.set(null);
    this.drawerOpen.set(false);
  }

  open(): void {
    this.drawerOpen.set(true);
  }

  close(): void {
    this.drawerOpen.set(false);
  }
}
