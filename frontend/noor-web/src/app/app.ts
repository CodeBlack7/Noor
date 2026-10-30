import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map } from 'rxjs';
import { CartService } from './core/services/cart.service';
import { WishlistService } from './core/services/wishlist.service';
import { CartDrawerComponent } from './shared/cart-drawer/cart-drawer';
import { FooterComponent } from './shared/footer/footer';
import { HeaderComponent } from './shared/header/header';
import { ToastsComponent } from './shared/toasts/toasts';

@Component({
  selector: 'app-root',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, HeaderComponent, FooterComponent, CartDrawerComponent, ToastsComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly router = inject(Router);
  private readonly cart = inject(CartService);
  private readonly wishlist = inject(WishlistService);

  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects)
    ),
    { initialValue: this.router.url }
  );

  readonly chromeless = computed(() => {
    const url = this.currentUrl();
    return url.startsWith('/admin') || url.startsWith('/login') || url.startsWith('/register');
  });

  constructor() {
    this.cart.load();
    this.wishlist.load();
  }
}
