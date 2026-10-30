import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE } from '../api.config';
import { WishlistItem, WishlistToggleResponse } from '../models/api.models';
import { SessionService } from './session.service';
import { ToastService } from './toast.service';

@Injectable({ providedIn: 'root' })
export class WishlistService {
  private readonly http = inject(HttpClient);
  private readonly session = inject(SessionService);
  private readonly toast = inject(ToastService);

  readonly ids = signal<Set<number>>(new Set<number>());
  readonly count = computed(() => this.ids().size);

  load(): void {
    if (!this.session.isAuthenticated()) {
      this.ids.set(new Set<number>());
      return;
    }
    this.getItems().subscribe({
      next: (items) => this.ids.set(new Set(items.map((item) => item.product.id))),
      error: () => undefined
    });
  }

  getItems(): Observable<WishlistItem[]> {
    return this.http.get<WishlistItem[]>(`${API_BASE}/wishlist`);
  }

  toggle(productId: number): void {
    if (!this.session.isAuthenticated()) {
      this.toast.show('Sign in to save pieces to your wishlist.', 'info');
      return;
    }

    this.http
      .post<WishlistToggleResponse>(`${API_BASE}/wishlist/${productId}`, {})
      .subscribe({
        next: (response) => {
          const next = new Set(this.ids());
          if (response.inWishlist) next.add(productId);
          else next.delete(productId);
          this.ids.set(next);
          this.toast.show(response.message, response.inWishlist ? 'success' : 'info');
        }
      });
  }

  isSaved(productId: number): boolean {
    return this.ids().has(productId);
  }

  reset(): void {
    this.ids.set(new Set<number>());
  }
}
