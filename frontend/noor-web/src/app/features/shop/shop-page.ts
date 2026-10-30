import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, ParamMap, Router, RouterLink } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  Category,
  PagedResult,
  ProductFacets,
  ProductQuery,
  ProductSummary
} from '../../core/models/api.models';
import { CatalogService } from '../../core/services/catalog.service';
import { OverlayService } from '../../core/services/overlay.service';
import { IconComponent } from '../../shared/icon/icon';
import { ProductCardComponent } from '../../shared/product-card/product-card';

interface ShopFilters {
  search: string;
  category: string;
  minPrice: number | null;
  maxPrice: number | null;
  size: string;
  color: string;
  featured: boolean;
  sort: string;
  page: number;
}

const DEFAULTS: ShopFilters = {
  search: '',
  category: '',
  minPrice: null,
  maxPrice: null,
  size: '',
  color: '',
  featured: false,
  sort: 'featured',
  page: 1
};

const PAGE_SIZE = 12;

@Component({
  selector: 'app-shop-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, CurrencyPipe, IconComponent, ProductCardComponent],
  templateUrl: './shop-page.html',
  styleUrl: './shop-page.scss'
})
export class ShopPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly catalog = inject(CatalogService);
  private readonly overlay = inject(OverlayService);

  readonly categories = this.catalog.categories;
  readonly facets = signal<ProductFacets | null>(null);
  readonly result = signal<PagedResult<ProductSummary> | null>(null);
  readonly loading = signal(true);
  readonly filtersOpen = signal(false);

  readonly filters = signal<ShopFilters>({ ...DEFAULTS });

  minInput: number | null = null;
  maxInput: number | null = null;
  searchInput = '';

  readonly activeChips = computed(() => {
    const filters = this.filters();
    const chips: { key: keyof ShopFilters; label: string }[] = [];

    if (filters.search) chips.push({ key: 'search', label: `“${filters.search}”` });
    if (filters.category) {
      const category = this.categories().find((item) => item.slug === filters.category);
      chips.push({ key: 'category', label: category?.name ?? filters.category });
    }
    if (filters.size) chips.push({ key: 'size', label: `Size ${filters.size}` });
    if (filters.color) chips.push({ key: 'color', label: filters.color });
    if (filters.minPrice !== null || filters.maxPrice !== null) {
      chips.push({
        key: 'minPrice',
        label: this.priceLabel(filters.minPrice, filters.maxPrice)
      });
    }
    if (filters.featured) chips.push({ key: 'featured', label: 'Atelier favourites' });
    return chips;
  });

  readonly hasFilters = computed(() => this.activeChips().length > 0);

  readonly categoryName = computed(() => {
    const slug = this.filters().category;
    if (!slug) return this.filters().search ? 'Search results' : 'All pieces';
    return this.categories().find((item) => item.slug === slug)?.name ?? 'All pieces';
  });

  readonly pages = computed(() => {
    const result = this.result();
    if (!result || result.totalPages <= 1) return [];
    const total = result.totalPages;
    const current = result.page;
    const start = Math.max(1, Math.min(current - 2, total - 4));
    const end = Math.min(total, start + 4);
    return Array.from({ length: end - start + 1 }, (_, index) => start + index);
  });

  constructor() {
    this.catalog.loadCategories();
    this.catalog.getFacets().subscribe({
      next: (facets) => this.facets.set(facets),
      error: () => undefined
    });

    this.route.queryParamMap
      .pipe(takeUntilDestroyed())
      .subscribe((params) => this.applyRoute(params));

    effect((onCleanup) => {
      if (this.filtersOpen()) this.overlay.lock('shop-filters');
      else this.overlay.unlock('shop-filters');
      onCleanup(() => this.overlay.unlock('shop-filters'));
    });
  }

  toggleCategory(slug: string): void {
    this.update({ category: this.filters().category === slug ? '' : slug });
  }

  toggleSize(size: string): void {
    this.update({ size: this.filters().size === size ? '' : size });
  }

  toggleColor(color: string): void {
    this.update({ color: this.filters().color === color ? '' : color });
  }

  toggleFeatured(): void {
    this.update({ featured: !this.filters().featured });
  }

  applySearch(): void {
    this.update({ search: this.searchInput.trim() });
  }

  applyPrice(): void {
    const min = this.minInput !== null && this.minInput > 0 ? this.minInput : null;
    const max = this.maxInput !== null && this.maxInput > 0 ? this.maxInput : null;
    if (min !== null && max !== null && min > max) {
      this.update({ minPrice: max, maxPrice: min });
      return;
    }
    this.update({ minPrice: min, maxPrice: max });
  }

  applyPreset(min: number | null, max: number | null): void {
    this.minInput = min;
    this.maxInput = max;
    this.update({ minPrice: min, maxPrice: max });
  }

  setSort(value: string): void {
    this.update({ sort: value });
  }

  goToPage(page: number): void {
    const result = this.result();
    if (!result || page < 1 || page > result.totalPages || page === result.page) return;
    this.update({ page }, false);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  clearAll(): void {
    this.minInput = null;
    this.maxInput = null;
    this.searchInput = '';
    this.router.navigate(['/shop']);
  }

  clearChip(key: keyof ShopFilters): void {
    if (key === 'minPrice') {
      this.minInput = null;
      this.maxInput = null;
      this.update({ minPrice: null, maxPrice: null });
      return;
    }
    if (key === 'featured') {
      this.update({ featured: false });
      return;
    }
    if (key === 'search') this.searchInput = '';
    this.update({ [key]: DEFAULTS[key] });
  }

  isPresetActive(min: number | null, max: number | null): boolean {
    const filters = this.filters();
    return filters.minPrice === min && filters.maxPrice === max;
  }

  priceLabel(min: number | null, max: number | null): string {
    const format = (value: number) => `₹${value.toLocaleString('en-IN')}`;
    if (min !== null && max !== null) return `${format(min)} – ${format(max)}`;
    if (min !== null) return `${format(min)}+`;
    if (max !== null) return `Under ${format(max)}`;
    return 'Any price';
  }

  private applyRoute(params: ParamMap): void {
    const filters: ShopFilters = {
      search: params.get('search')?.trim() ?? '',
      category: params.get('category') ?? '',
      minPrice: this.numberOrNull(params.get('minPrice')),
      maxPrice: this.numberOrNull(params.get('maxPrice')),
      size: params.get('size') ?? '',
      color: params.get('color') ?? '',
      featured: params.get('featured') === 'true',
      sort: params.get('sort') || DEFAULTS.sort,
      page: Math.max(1, Number(params.get('page')) || 1)
    };

    this.filters.set(filters);
    this.searchInput = filters.search;
    this.minInput = filters.minPrice;
    this.maxInput = filters.maxPrice;

    this.load(filters);
  }

  update(patch: Partial<ShopFilters>, resetPage = true): void {
    const next = {
      ...this.filters(),
      ...patch,
      page: resetPage ? 1 : (patch.page ?? this.filters().page)
    };

    const query: Record<string, string | number | boolean> = {};
    if (next.search) query['search'] = next.search;
    if (next.category) query['category'] = next.category;
    if (next.minPrice !== null) query['minPrice'] = next.minPrice;
    if (next.maxPrice !== null) query['maxPrice'] = next.maxPrice;
    if (next.size) query['size'] = next.size;
    if (next.color) query['color'] = next.color;
    if (next.featured) query['featured'] = true;
    if (next.sort !== DEFAULTS.sort) query['sort'] = next.sort;
    if (next.page > 1) query['page'] = next.page;

    this.router.navigate(['/shop'], {
      queryParams: query,
      replaceUrl: this.filtersOpen()
    });
  }

  private load(filters: ShopFilters): void {
    this.loading.set(true);

    const query: ProductQuery = {
      search: filters.search || undefined,
      category: filters.category || undefined,
      minPrice: filters.minPrice ?? undefined,
      maxPrice: filters.maxPrice ?? undefined,
      size: filters.size || undefined,
      color: filters.color || undefined,
      featured: filters.featured || undefined,
      sort: filters.sort,
      page: filters.page,
      pageSize: PAGE_SIZE
    };

    this.catalog.getProducts(query).subscribe({
      next: (result) => {
        this.result.set(result);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private numberOrNull(value: string | null): number | null {
    if (!value) return null;
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }
}
