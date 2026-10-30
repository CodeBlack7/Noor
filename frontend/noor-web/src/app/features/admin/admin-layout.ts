import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { OverlayService } from '../../core/services/overlay.service';
import { SessionService } from '../../core/services/session.service';
import { IconComponent } from '../../shared/icon/icon';

interface AdminNavItem {
  label: string;
  path: string;
  icon: string;
  exact?: boolean;
}

interface AdminNavGroup {
  heading: string;
  items: AdminNavItem[];
}

@Component({
  selector: 'app-admin-layout',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, IconComponent],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.scss'
})
export class AdminLayoutComponent {
  private readonly router = inject(Router);
  private readonly overlay = inject(OverlayService);
  private readonly auth = inject(AuthService);

  readonly session = inject(SessionService);
  readonly menuOpen = signal(false);

  readonly groups: AdminNavGroup[] = [
    {
      heading: 'Atelier',
      items: [
        { label: 'Dashboard', path: '/admin/dashboard', icon: 'grid' },
        { label: 'Orders', path: '/admin/orders', icon: 'package' },
        { label: 'Clients', path: '/admin/customers', icon: 'users' }
      ]
    },
    {
      heading: 'Catalogue',
      items: [
        { label: 'Products', path: '/admin/products', icon: 'tag' },
        { label: 'Collections', path: '/admin/categories', icon: 'book' },
        { label: 'Coupons', path: '/admin/coupons', icon: 'percent' }
      ]
    }
  ];

  private readonly url = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects)
    ),
    { initialValue: this.router.url }
  );

  readonly pageTitle = computed(() => {
    const url = this.url();
    if (url.startsWith('/admin/products/new')) return 'New product';
    if (url.match(/^\/admin\/products\/\d+\/edit/)) return 'Edit product';
    if (url.startsWith('/admin/products')) return 'Products';
    if (url.startsWith('/admin/categories')) return 'Collections';
    if (url.match(/^\/admin\/orders\//)) return 'Order details';
    if (url.startsWith('/admin/orders')) return 'Orders';
    if (url.startsWith('/admin/customers')) return 'Clients';
    if (url.startsWith('/admin/coupons')) return 'Coupons';
    return 'Dashboard';
  });

  constructor() {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => this.menuOpen.set(false));

    effect((onCleanup) => {
      if (this.menuOpen()) this.overlay.lock('admin-menu');
      else this.overlay.unlock('admin-menu');
      onCleanup(() => this.overlay.unlock('admin-menu'));
    });
  }

  logout(): void {
    this.menuOpen.set(false);
    this.auth.logout();
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
}
