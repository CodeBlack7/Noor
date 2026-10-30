import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { CatalogService } from '../../core/services/catalog.service';
import { OverlayService } from '../../core/services/overlay.service';
import { SessionService } from '../../core/services/session.service';
import { WishlistService } from '../../core/services/wishlist.service';
import { IconComponent } from '../icon/icon';

@Component({
  selector: 'app-header',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, IconComponent],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class HeaderComponent {
  private readonly router = inject(Router);
  private readonly overlay = inject(OverlayService);
  private readonly auth = inject(AuthService);

  readonly session = inject(SessionService);
  readonly cart = inject(CartService);
  readonly wishlist = inject(WishlistService);
  readonly catalog = inject(CatalogService);

  readonly menuOpen = signal(false);
  readonly searchOpen = signal(false);
  readonly accountOpen = signal(false);

  searchTerm = '';

  private readonly searchInput = viewChild<ElementRef<HTMLInputElement>>('searchInput');

  constructor() {
    this.catalog.loadCategories();

    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntilDestroyed()
      )
      .subscribe(() => this.closePanels());

    effect((onCleanup) => {
      if (this.menuOpen()) {
        this.overlay.lock('mobile-menu');
      } else {
        this.overlay.unlock('mobile-menu');
      }
      onCleanup(() => this.overlay.unlock('mobile-menu'));
    });
  }

  toggleSearch(): void {
    this.accountOpen.set(false);
    this.searchOpen.update((open) => !open);
    if (this.searchOpen()) {
      setTimeout(() => this.searchInput()?.nativeElement.focus(), 60);
    }
  }

  submitSearch(event: Event): void {
    event.preventDefault();
    const term = this.searchTerm.trim();
    this.router.navigate(['/shop'], {
      queryParams: term ? { search: term } : {}
    });
    this.searchTerm = '';
    this.searchOpen.set(false);
  }

  closePanels(): void {
    this.menuOpen.set(false);
    this.searchOpen.set(false);
    this.accountOpen.set(false);
  }

  logout(): void {
    this.accountOpen.set(false);
    this.menuOpen.set(false);
    this.auth.logout();
  }
}
