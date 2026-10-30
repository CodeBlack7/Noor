import { computed, Injectable, signal } from '@angular/core';
import { AuthResponse, User } from '../models/api.models';

const ACCESS_KEY = 'noor.access-token';
const REFRESH_KEY = 'noor.refresh-token';
const USER_KEY = 'noor.user';
const CART_KEY = 'noor.cart-key';

@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly userState = signal<User | null>(this.readUser());
  readonly user = this.userState.asReadonly();
  readonly isAuthenticated = computed(() => this.userState() !== null);
  readonly isAdmin = computed(() => this.userState()?.role === 'Admin');
  readonly cartKey = signal(this.readCartKey());

  constructor() {
    if (this.userState() && !this.accessToken()) {
      this.clear();
    }
  }

  accessToken(): string | null {
    return localStorage.getItem(ACCESS_KEY);
  }

  refreshToken(): string | null {
    return localStorage.getItem(REFRESH_KEY);
  }

  setSession(response: AuthResponse): void {
    localStorage.setItem(ACCESS_KEY, response.accessToken);
    localStorage.setItem(REFRESH_KEY, response.refreshToken);
    this.persistUser(response.user);
  }

  updateTokens(accessToken: string, refreshToken: string, user?: User): void {
    localStorage.setItem(ACCESS_KEY, accessToken);
    localStorage.setItem(REFRESH_KEY, refreshToken);
    if (user) this.persistUser(user);
  }

  updateUser(user: User): void {
    this.persistUser(user);
  }

  clear(): void {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(USER_KEY);
    this.userState.set(null);
  }

  firstName(): string {
    return this.userState()?.fullName.split(' ')[0] ?? 'there';
  }

  private persistUser(user: User): void {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this.userState.set(user);
  }

  private readUser(): User | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as User;
    } catch {
      localStorage.removeItem(USER_KEY);
      return null;
    }
  }

  private readCartKey(): string {
    const existing = localStorage.getItem(CART_KEY);
    if (existing) return existing;

    const key =
      typeof crypto !== 'undefined' && 'randomUUID' in crypto
        ? crypto.randomUUID()
        : `guest-${Date.now()}-${Math.random().toString(16).slice(2)}`;
    localStorage.setItem(CART_KEY, key);
    return key;
  }
}
