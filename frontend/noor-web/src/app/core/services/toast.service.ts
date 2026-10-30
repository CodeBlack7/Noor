import { Injectable, signal } from '@angular/core';

export type ToastKind = 'success' | 'error' | 'info';

export interface ToastItem {
  id: number;
  kind: ToastKind;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly state = signal<ToastItem[]>([]);
  private nextId = 1;

  readonly toasts = this.state.asReadonly();

  show(message: string, kind: ToastKind = 'success', duration = 4200): void {
    const id = this.nextId++;
    this.state.update((items) => [...items.slice(-3), { id, kind, message }]);
    setTimeout(() => this.dismiss(id), duration);
  }

  dismiss(id: number): void {
    this.state.update((items) => items.filter((item) => item.id !== id));
  }
}
