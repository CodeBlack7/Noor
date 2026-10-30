import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  Address,
  SaveAddressRequest,
  ShippingMethod
} from '../../core/models/api.models';
import { AccountService } from '../../core/services/account.service';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { SessionService } from '../../core/services/session.service';
import { ToastService } from '../../core/services/toast.service';
import { IconComponent } from '../../shared/icon/icon';

@Component({
  selector: 'app-checkout-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, CurrencyPipe, IconComponent],
  templateUrl: './checkout-page.html',
  styleUrl: './checkout-page.scss'
})
export class CheckoutPageComponent {
  private readonly account = inject(AccountService);
  private readonly orders = inject(OrderService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly cartService = inject(CartService);
  readonly session = inject(SessionService);
  readonly cart = this.cartService.cart;

  readonly addresses = signal<Address[]>([]);
  readonly addressesLoading = signal(true);
  readonly selectedAddressId = signal<number | null>(null);
  readonly showAddressForm = signal(false);
  readonly savingAddress = signal(false);
  readonly placing = signal(false);

  readonly shippingMethod = signal<ShippingMethod>('Standard');

  readonly shippingCost = computed(() => {
    const cart = this.cart();
    if (!cart) return 0;
    if (this.shippingMethod() === 'Express') return 249;
    const afterDiscount = Math.max(0, cart.subtotal - cart.discountTotal);
    return afterDiscount >= 1999 ? 0 : 99;
  });

  readonly orderTotal = computed(() => {
    const cart = this.cart();
    if (!cart) return 0;
    return Math.max(0, cart.subtotal - cart.discountTotal) + this.shippingCost();
  });

  address: SaveAddressRequest = {
    label: 'Home',
    fullName: '',
    phone: '',
    line1: '',
    line2: '',
    city: '',
    state: '',
    postalCode: '',
    country: 'India',
    isDefault: false
  };

  notes = '';

  constructor() {
    const user = this.session.user();
    this.address.fullName = user?.fullName ?? '';
    this.address.phone = user?.phone ?? '';
    this.loadAddresses();
  }

  selectAddress(id: number): void {
    this.selectedAddressId.set(id);
    this.showAddressForm.set(false);
  }

  openAddressForm(): void {
    this.showAddressForm.set(true);
  }

  cancelAddressForm(): void {
    if (this.addresses().length > 0) this.showAddressForm.set(false);
  }

  saveAddress(): void {
    if (this.savingAddress()) return;

    const required: (keyof SaveAddressRequest)[] = [
      'label',
      'fullName',
      'phone',
      'line1',
      'city',
      'state',
      'postalCode',
      'country'
    ];
    const missing = required.some((key) => !String(this.address[key] ?? '').trim());
    if (missing) {
      this.toast.show('Please complete the address fields.', 'info');
      return;
    }

    this.savingAddress.set(true);
    this.account.createAddress({ ...this.address, line2: this.address.line2 || null }).subscribe({
      next: (address) => {
        this.addresses.update((items) => [...items, address]);
        this.selectedAddressId.set(address.id);
        this.showAddressForm.set(false);
        this.savingAddress.set(false);
        this.address = {
          ...this.address,
          line1: '',
          line2: '',
          city: '',
          state: '',
          postalCode: '',
          isDefault: false
        };
        this.toast.show('Address saved.');
      },
      error: () => this.savingAddress.set(false)
    });
  }

  placeOrder(formValid: boolean | null): void {
    if (!formValid || this.placing()) return;

    const addressId = this.selectedAddressId();
    if (!addressId) {
      this.toast.show('Please choose a delivery address.', 'info');
      return;
    }

    this.placing.set(true);
    this.orders
      .placeOrder({
        addressId,
        shippingMethod: this.shippingMethod(),
        paymentMethod: 'CashOnDelivery',
        notes: this.notes.trim() || null
      })
      .subscribe({
        next: (order) => {
          this.cartService.reset();
          this.cartService.load();
          this.toast.show(`Order ${order.orderNumber} confirmed.`);
          this.router.navigate(['/account/orders', order.id], {
            queryParams: { placed: '1' }
          });
        },
        error: () => this.placing.set(false)
      });
  }

  setShipping(method: ShippingMethod): void {
    this.shippingMethod.set(method);
  }

  private loadAddresses(): void {
    this.account.getAddresses().subscribe({
      next: (addresses) => {
        this.addresses.set(addresses);
        this.addressesLoading.set(false);
        const preferred = addresses.find((item) => item.isDefault) ?? addresses[0];
        if (preferred) this.selectedAddressId.set(preferred.id);
        else this.showAddressForm.set(true);
      },
      error: () => {
        this.addressesLoading.set(false);
        this.showAddressForm.set(true);
      }
    });
  }
}
