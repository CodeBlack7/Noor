import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class OverlayService {
  private readonly locks = new Set<string>();

  lock(key: string): void {
    this.locks.add(key);
    this.update();
  }

  unlock(key: string): void {
    this.locks.delete(key);
    this.update();
  }

  private update(): void {
    document.body.classList.toggle('no-scroll', this.locks.size > 0);
  }
}
