import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  OrderStatus,
  OrderSummary,
  PagedResult
} from '../../core/models/api.models';
import { AdminService } from '../../core/services/admin.service';
import { IconComponent } from '../../shared/icon/icon';

type StatusFilter = '' | OrderStatus;

const PAGE_SIZE = 15;

@Component({
  selector: 'app-admin-orders-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, CurrencyPipe, DatePipe, IconComponent],
  templateUrl: './orders-page.html',
  styleUrl: './orders-page.scss'
})
export class AdminOrdersPageComponent {
  private readonly admin = inject(AdminService);
  private readonly route = inject(ActivatedRoute);

  readonly statuses: OrderStatus[] = [
    'Pending',
    'Confirmed',
    'Processing',
    'Shipped',
    'Delivered',
    'Cancelled',
    'Refunded'
  ];

  readonly result = signal<PagedResult<OrderSummary> | null>(null);
  readonly loading = signal(true);
  readonly failed = signal(false);

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

  search = this.route.snapshot.queryParamMap.get('search')?.trim() ?? '';
  statusFilter: StatusFilter = (this.route.snapshot.queryParamMap.get('status') as StatusFilter) ?? '';
  page = Math.max(1, Number(this.route.snapshot.queryParamMap.get('page')) || 1);

  constructor() {
    if (!this.statuses.includes(this.statusFilter as OrderStatus)) {
      this.statusFilter = '';
    }
    this.load();
  }

  applyFilters(): void {
    this.page = 1;
    this.load();
  }

  resetFilters(): void {
    this.search = '';
    this.statusFilter = '';
    this.page = 1;
    this.load();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page) return;
    this.page = page;
    this.load();
  }

  statusClass(status: OrderStatus): string {
    return `status--${status.toLowerCase()}`;
  }

  paymentClass(status: string): string {
    return `status--${status === 'Paid' ? 'paid' : 'unpaid'}`;
  }

  load(): void {
    this.loading.set(true);
    this.failed.set(false);

    this.admin
      .getOrders({
        search: this.search.trim() || undefined,
        status: this.statusFilter || undefined,
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
