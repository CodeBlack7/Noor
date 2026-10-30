import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  AdminCategory,
  AdminProductListItem,
  PagedResult
} from '../../core/models/api.models';
import { AdminService } from '../../core/services/admin.service';
import { ToastService } from '../../core/services/toast.service';
import { IconComponent } from '../../shared/icon/icon';

type StatusFilter = '' | 'active' | 'inactive';

const PAGE_SIZE = 15;

@Component({
  selector: 'app-admin-products-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, CurrencyPipe, DatePipe, DecimalPipe, IconComponent],
  templateUrl: './products-page.html',
  styleUrl: './products-page.scss'
})
export class AdminProductsPageComponent {
  private readonly admin = inject(AdminService);
  private readonly toast = inject(ToastService);

  readonly result = signal<PagedResult<AdminProductListItem> | null>(null);
  readonly categories = signal<AdminCategory[]>([]);
  readonly loading = signal(true);
  readonly failed = signal(false);
  readonly deletingId = signal<number | null>(null);

  readonly items = computed(() => this.result()?.items ?? []);
  readonly totalCount = computed(() => this.result()?.totalCount ?? 0);
  readonly totalPages = computed(() => this.result()?.totalPages ?? 0);

  readonly pages = computed(() => {
    const total = this.totalPages();
    if (total <= 1) return [];
    const current = this.result()?.page ?? 1;
    const start = Math.max(1, Math.min(current - 2, total - 4));
    const end = Math.min(total, start + 4);
    return Array.from({ length: end - start + 1 }, (_, index) => start + index);
  });

  search = '';
  categoryId: number | null = null;
  statusFilter: StatusFilter = '';
  page = 1;

  constructor() {
    this.admin.getCategories().subscribe({
      next: (categories) => this.categories.set(categories),
      error: () => undefined
    });
    this.load();
  }

  applyFilters(): void {
    this.page = 1;
    this.load();
  }

  resetFilters(): void {
    this.search = '';
    this.categoryId = null;
    this.statusFilter = '';
    this.page = 1;
    this.load();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page) return;
    this.page = page;
    this.load();
  }

  deleteProduct(product: AdminProductListItem): void {
    if (this.deletingId()) return;
    const confirmed = window.confirm(
      `Delete “${product.name}”? Products with order history are archived instead.`
    );
    if (!confirmed) return;

    this.deletingId.set(product.id);
    this.admin.deleteProduct(product.id).subscribe({
      next: (result) => {
        this.deletingId.set(null);
        this.toast.show(result.message);
        if (this.items().length === 1 && this.page > 1) this.page -= 1;
        this.load();
      },
      error: () => this.deletingId.set(null)
    });
  }

  stockTone(product: AdminProductListItem): string {
    if (product.totalStock <= 0) return 'out';
    if (product.totalStock <= 10) return 'low';
    return 'ok';
  }

  private load(): void {
    this.loading.set(true);
    this.failed.set(false);

    this.admin
      .getProducts({
        search: this.search.trim() || undefined,
        categoryId: this.categoryId ?? undefined,
        isActive:
          this.statusFilter === '' ? undefined : this.statusFilter === 'active',
        page: this.page,
        pageSize: PAGE_SIZE
      })
      .subscribe({
        next: (result) => {
          this.result.set(result);
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
          this.failed.set(true);
        }
      });
  }
}
