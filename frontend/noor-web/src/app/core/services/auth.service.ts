import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { API_BASE, NO_TOAST } from '../api.config';
import {
  AuthResponse,
  ChangePasswordRequest,
  LoginRequest,
  RegisterRequest,
  UpdateProfileRequest,
  User
} from '../models/api.models';
import { CartService } from './cart.service';
import { SessionService } from './session.service';
import { ToastService } from './toast.service';
import { WishlistService } from './wishlist.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly session = inject(SessionService);
  private readonly cart = inject(CartService);
  private readonly wishlist = inject(WishlistService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${API_BASE}/auth/login`, request, { headers: NO_TOAST })
      .pipe(tap((response) => this.completeSignIn(response)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${API_BASE}/auth/register`, request, { headers: NO_TOAST })
      .pipe(tap((response) => this.completeSignIn(response)));
  }

  me(): Observable<User> {
    return this.http.get<User>(`${API_BASE}/auth/me`).pipe(
      tap((user) => this.session.updateUser(user))
    );
  }

  updateProfile(request: UpdateProfileRequest): Observable<User> {
    return this.http
      .put<User>(`${API_BASE}/auth/profile`, request)
      .pipe(tap((user) => this.session.updateUser(user)));
  }

  changePassword(request: ChangePasswordRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${API_BASE}/auth/change-password`, request)
      .pipe(tap((response) => this.session.setSession(response)));
  }

  logout(): void {
    const refreshToken = this.session.refreshToken();
    if (refreshToken) {
      this.http
        .post(`${API_BASE}/auth/logout`, { refreshToken }, { headers: NO_TOAST })
        .subscribe({ error: () => undefined });
    }

    this.session.clear();
    this.cart.reset();
    this.wishlist.reset();
    this.toast.show('You have been signed out.', 'info');
    this.router.navigate(['/']);
  }

  private completeSignIn(response: AuthResponse): void {
    this.session.setSession(response);
    this.wishlist.load();
    this.cart.mergeGuestCart().subscribe({
      next: (cart) => {
        if (cart.itemCount > 0) this.cart.drawerOpen.set(true);
      },
      error: () => this.cart.load()
    });
  }
}
