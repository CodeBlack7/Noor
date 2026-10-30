import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ProductSummary } from '../../core/models/api.models';
import { CartService } from '../../core/services/cart.service';
import { CatalogService } from '../../core/services/catalog.service';
import { SessionService } from '../../core/services/session.service';
import { ToastService } from '../../core/services/toast.service';
import { IconComponent } from '../../shared/icon/icon';
import { ProductCardComponent } from '../../shared/product-card/product-card';
import { QuantityStepperComponent } from '../../shared/quantity-stepper/quantity-stepper';

@Component({
  selector: 'app-cart-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    FormsModule,
    CurrencyPipe,
    IconComponent,
    ProductCardComponent,
    QuantityStepperComponent
  ],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.scss'
})
export class CartPageComponent {
  readonly cartService = inject(CartService);
  private readonly catalog = inject(CatalogService);
  private readonly session = inject(SessionService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly cart = this.cartService.cart;
  readonly recommended = signal<ProductSummary[]>([]);
  readonly applyingCoupon = signal(false);
  readonly shipping = computed(() => this.cart()?.shippingEstimate ?? 0);

  couponCode = '';

  constructor() {
    this.catalog.getProducts({ pageSize: 4, sort: 'rating' }).subscribe({
      next: (result) => this.recommended.set(result.items),
      error: () => undefined
    });
  }

  applyCoupon(): void {
    const code = this.couponCode.trim();
    if (!code || this.applyingCoupon()) return;

    this.applyingCoupon.set(true);
    this.cartService.applyCoupon(code).subscribe({
      next: (cart) => {
        this.applyingCoupon.set(false);
        this.couponCode = '';
        this.toast.show(`Coupon ${cart.couponCode} applied.`);
      },
      error: () => this.applyingCoupon.set(false)
    });
  }

  removeCoupon(): void {
    this.cartService.removeCoupon().subscribe({
      next: () => this.toast.show('Coupon removed.', 'info')
    });
  }

  checkout(): void {
    if (!this.session.isAuthenticated()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: '/checkout' } });
      return;
    }
    this.router.navigate(['/checkout']);
  }
}
