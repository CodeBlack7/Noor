import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  Address,
  OrderSummary,
  SaveAddressRequest,
  WishlistItem
} from '../../core/models/api.models';
import { AccountService } from '../../core/services/account.service';
import { AuthService } from '../../core/services/auth.service';
import { OrderService } from '../../core/services/order.service';
import { SessionService } from '../../core/services/session.service';
import { ToastService } from '../../core/services/toast.service';
import { WishlistService } from '../../core/services/wishlist.service';
import { IconComponent } from '../../shared/icon/icon';
import { ProductCardComponent } from '../../shared/product-card/product-card';

type AccountSection = 'overview' | 'orders' | 'addresses' | 'wishlist' | 'profile';

const SECTIONS: { id: AccountSection; label: string; icon: string }[] = [
  { id: 'overview', label: 'Overview', icon: 'home' },
  { id: 'orders', label: 'Orders', icon: 'package' },
  { id: 'addresses', label: 'Addresses', icon: 'map-pin' },
  { id: 'wishlist', label: 'Wishlist', icon: 'heart' },
  { id: 'profile', label: 'Profile & password', icon: 'settings' }
];

@Component({
  selector: 'app-account-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    FormsModule,
    CurrencyPipe,
    DatePipe,
    IconComponent,
    ProductCardComponent
  ],
  templateUrl: './account-page.html',
  styleUrl: './account-page.scss'
})
export class AccountPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly account = inject(AccountService);
  private readonly ordersService = inject(OrderService);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  readonly session = inject(SessionService);
  readonly wishlist = inject(WishlistService);

  readonly sections = SECTIONS;
  readonly section = signal<AccountSection>('overview');
  readonly sectionTitle = computed(
    () => SECTIONS.find((item) => item.id === this.section())?.label ?? 'Overview'
  );

  readonly orders = signal<OrderSummary[]>([]);
  readonly ordersLoading = signal(true);
  readonly addresses = signal<Address[]>([]);
  readonly addressesLoading = signal(true);
  readonly wishlistItems = signal<WishlistItem[]>([]);
  readonly wishlistLoading = signal(true);

  readonly cancelingId = signal<string | null>(null);
  readonly savingAddress = signal(false);
  readonly deletingAddressId = signal<number | null>(null);
  readonly addressFormOpen = signal(false);
  readonly editingAddressId = signal<number | null>(null);

  readonly savingProfile = signal(false);
  readonly savingPassword = signal(false);

  readonly activeOrders = computed(
    () =>
      this.orders().filter(
        (order) => order.status !== 'Cancelled' && order.status !== 'Delivered'
      ).length
  );

  readonly recentOrders = computed(() => this.orders().slice(0, 3));

  readonly defaultAddress = computed(
    () => this.addresses().find((address) => address.isDefault) ?? this.addresses()[0] ?? null
  );

  readonly wishlistVisible = computed(() =>
    this.wishlistItems().filter((item) => this.wishlist.ids().has(item.product.id))
  );

  profile = {
    fullName: '',
    phone: '',
    marketingOptIn: true
  };

  passwords = {
    currentPassword: '',
    newPassword: '',
    confirm: ''
  };

  address: SaveAddressRequest = this.emptyAddress();

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed()).subscribe((params) => {
      const requested = params.get('section') as AccountSection | null;
      this.section.set(
        requested && SECTIONS.some((item) => item.id === requested) ? requested : 'overview'
      );
    });

    this.loadOrders();
    this.loadAddresses();
    this.loadWishlist();

    effect(() => {
      const user = this.session.user();
      if (user) {
        this.profile.fullName = user.fullName;
        this.profile.phone = user.phone ?? '';
        this.profile.marketingOptIn = user.marketingOptIn;
      }
    });
  }

  firstName(): string {
    return this.session.user()?.fullName.split(' ')[0] ?? 'there';
  }

  initials(): string {
    const name = this.session.user()?.fullName ?? '';
    return name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase())
      .join('');
  }

  statusClass(status: string): string {
    return `status--${status.toLowerCase()}`;
  }

  cancelOrder(order: OrderSummary): void {
    if (this.cancelingId()) return;
    if (!window.confirm(`Cancel order ${order.orderNumber}? This cannot be undone.`)) return;

    this.cancelingId.set(order.id);
    this.ordersService.cancel(order.id).subscribe({
      next: (detail) => {
        this.orders.update((items) =>
          items.map((item) =>
            item.id === order.id ? { ...item, status: detail.status } : item
          )
        );
        this.cancelingId.set(null);
        this.toast.show('Order cancelled.', 'info');
      },
      error: () => this.cancelingId.set(null)
    });
  }

  openNewAddress(): void {
    this.editingAddressId.set(null);
    this.address = this.emptyAddress();
    this.addressFormOpen.set(true);
  }

  editAddress(address: Address): void {
    this.editingAddressId.set(address.id);
    this.address = {
      label: address.label,
      fullName: address.fullName,
      phone: address.phone,
      line1: address.line1,
      line2: address.line2,
      city: address.city,
      state: address.state,
      postalCode: address.postalCode,
      country: address.country,
      isDefault: address.isDefault
    };
    this.addressFormOpen.set(true);
  }

  cancelAddressForm(): void {
    this.addressFormOpen.set(false);
    this.editingAddressId.set(null);
    this.address = this.emptyAddress();
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
    if (required.some((key) => !String(this.address[key] ?? '').trim())) {
      this.toast.show('Please complete the address fields.', 'info');
      return;
    }

    const id = this.editingAddressId();
    this.savingAddress.set(true);
    const request = { ...this.address, line2: this.address.line2 || null };
    const operation = id
      ? this.account.updateAddress(id, request)
      : this.account.createAddress(request);

    operation.subscribe({
      next: () => {
        this.savingAddress.set(false);
        this.addressFormOpen.set(false);
        this.editingAddressId.set(null);
        this.address = this.emptyAddress();
        this.loadAddresses();
        this.toast.show(id ? 'Address updated.' : 'Address added.');
      },
      error: () => this.savingAddress.set(false)
    });
  }

  deleteAddress(address: Address): void {
    if (!window.confirm(`Remove the ${address.label} address?`)) return;

    this.deletingAddressId.set(address.id);
    this.account.deleteAddress(address.id).subscribe({
      next: () => {
        this.addresses.update((items) => items.filter((item) => item.id !== address.id));
        this.deletingAddressId.set(null);
        this.toast.show('Address removed.', 'info');
      },
      error: () => this.deletingAddressId.set(null)
    });
  }

  makeDefault(address: Address): void {
    this.account.setDefaultAddress(address.id).subscribe({
      next: () => {
        this.addresses.update((items) =>
          items.map((item) => ({ ...item, isDefault: item.id === address.id }))
        );
        this.toast.show('Default address updated.');
      }
    });
  }

  saveProfile(): void {
    if (!this.profile.fullName.trim() || this.savingProfile()) return;

    this.savingProfile.set(true);
    this.auth
      .updateProfile({
        fullName: this.profile.fullName.trim(),
        phone: this.profile.phone.trim() || null,
        marketingOptIn: this.profile.marketingOptIn
      })
      .subscribe({
        next: () => {
          this.savingProfile.set(false);
          this.toast.show('Profile updated.');
        },
        error: () => this.savingProfile.set(false)
      });
  }

  changePassword(): void {
    if (this.savingPassword()) return;

    if (!this.passwords.currentPassword || !this.passwords.newPassword) {
      this.toast.show('Please complete both password fields.', 'info');
      return;
    }
    if (this.passwords.newPassword !== this.passwords.confirm) {
      this.toast.show('Your new passwords do not match.', 'info');
      return;
    }
    if (!/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/.test(this.passwords.newPassword)) {
      this.toast.show(
        'Use at least 8 characters, with uppercase, lowercase, and a number.',
        'info'
      );
      return;
    }

    this.savingPassword.set(true);
    this.auth
      .changePassword({
        currentPassword: this.passwords.currentPassword,
        newPassword: this.passwords.newPassword
      })
      .subscribe({
        next: () => {
          this.savingPassword.set(false);
          this.passwords = { currentPassword: '', newPassword: '', confirm: '' };
          this.toast.show('Password updated.');
        },
        error: () => this.savingPassword.set(false)
      });
  }

  signOut(): void {
    this.auth.logout();
  }

  goShop(): void {
    this.router.navigate(['/shop']);
  }

  private loadOrders(): void {
    this.ordersLoading.set(true);
    this.ordersService.myOrders().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.ordersLoading.set(false);
      },
      error: () => this.ordersLoading.set(false)
    });
  }

  private loadAddresses(): void {
    this.addressesLoading.set(true);
    this.account.getAddresses().subscribe({
      next: (addresses) => {
        this.addresses.set(addresses);
        this.addressesLoading.set(false);
      },
      error: () => this.addressesLoading.set(false)
    });
  }

  private loadWishlist(): void {
    this.wishlistLoading.set(true);
    this.wishlist.getItems().subscribe({
      next: (items) => {
        this.wishlistItems.set(items);
        this.wishlistLoading.set(false);
      },
      error: () => this.wishlistLoading.set(false)
    });
  }

  private emptyAddress(): SaveAddressRequest {
    const user = this.session.user();
    return {
      label: 'Home',
      fullName: user?.fullName ?? '',
      phone: user?.phone ?? '',
      line1: '',
      line2: '',
      city: '',
      state: '',
      postalCode: '',
      country: 'India',
      isDefault: false
    };
  }
}
