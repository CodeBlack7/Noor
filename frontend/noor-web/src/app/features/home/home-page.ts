import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HomeContent } from '../../core/models/api.models';
import { CatalogService } from '../../core/services/catalog.service';
import { IconComponent } from '../../shared/icon/icon';
import { ProductCardComponent } from '../../shared/product-card/product-card';

@Component({
  selector: 'app-home-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, IconComponent, ProductCardComponent],
  templateUrl: './home-page.html',
  styleUrl: './home-page.scss'
})
export class HomePageComponent {
  private readonly catalog = inject(CatalogService);

  readonly home = signal<HomeContent | null>(null);
  readonly loading = signal(true);
  readonly featured = computed(() => this.home()?.featured.slice(0, 4) ?? []);
  readonly bestsellers = computed(() => this.home()?.bestsellers.slice(0, 4) ?? []);
  readonly collections = computed(() => this.home()?.categories ?? []);

  constructor() {
    this.catalog.getHome().subscribe({
      next: (home) => {
        this.home.set(home);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
