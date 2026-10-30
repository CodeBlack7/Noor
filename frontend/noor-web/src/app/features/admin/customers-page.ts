import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminCustomer, PagedResult } from '../../core/models/api.models';
import { AdminService } from '../../core/services/admin.service';
import { IconComponent } from '../../shared/icon/icon';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-admin-customers-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, CurrencyPipe, DatePipe, IconComponent],
  templateUrl: './customers-page.html',
  styleUrl: './customers-page.scss'
})
export class AdminCustomersPageComponent {
  private readonly admin = inject(AdminService);

  readonly result = signal<PagedResult<AdminCustomer> | null>(null);
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

  search = '';
  page = 1;

  constructor() {
    this.load();
  }

  applyFilters(): void {
    this.page = 1;
    this.load();
  }

  resetFilters(): void {
    this.search = '';
    this.page = 1;
    this.load();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page) return;
    this.page = page;
    this.load();
  }

  initials(name: string): string {
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  private load(): void {
    this.loading.set(true);
    this.failed.set(false);

    this.admin
      .getCustomers({
        search: this.search.trim() || undefined,
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
