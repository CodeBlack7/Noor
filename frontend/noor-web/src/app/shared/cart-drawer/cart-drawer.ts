import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { OverlayService } from '../../core/services/overlay.service';
import { IconComponent } from '../icon/icon';
import { QuantityStepperComponent } from '../quantity-stepper/quantity-stepper';

@Component({
  selector: 'app-cart-drawer',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [CurrencyPipe, RouterLink, IconComponent, QuantityStepperComponent],
  templateUrl: './cart-drawer.html',
  styleUrl: './cart-drawer.scss'
})
export class CartDrawerComponent {
  readonly cartService = inject(CartService);
  private readonly overlay = inject(OverlayService);
  private readonly router = inject(Router);

  readonly cart = this.cartService.cart;

  readonly progress = computed(() => {
    const cart = this.cart();
    if (!cart || cart.itemCount === 0) return 0;
    const afterDiscount = Math.max(0, cart.subtotal - cart.discountTotal);
    const target = afterDiscount + cart.freeShippingRemaining;
    if (target <= 0) return 100;
    return Math.min(100, (afterDiscount / target) * 100);
  });

  constructor() {
    effect((onCleanup) => {
      if (this.cartService.drawerOpen()) {
        this.overlay.lock('bag');
      } else {
        this.overlay.unlock('bag');
      }
      onCleanup(() => this.overlay.unlock('bag'));
    });
  }

  close(): void {
    this.cartService.close();
  }

  checkout(): void {
    this.cartService.close();
    this.router.navigate(['/checkout']);
  }
}
