import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Dashboard, SalesPoint } from '../../core/models/api.models';
import { AdminService } from '../../core/services/admin.service';
import { SessionService } from '../../core/services/session.service';
import { IconComponent } from '../../shared/icon/icon';

interface ChartBar extends SalesPoint {
  x: number;
  y: number;
  width: number;
  height: number;
}

@Component({
  selector: 'app-admin-dashboard-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, CurrencyPipe, DatePipe, DecimalPipe, IconComponent],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss'
})
export class AdminDashboardPageComponent {
  private readonly admin = inject(AdminService);
  readonly session = inject(SessionService);

  readonly data = signal<Dashboard | null>(null);
  readonly loading = signal(true);
  readonly failed = signal(false);

  readonly chartBars = computed<ChartBar[]>(() => {
    const points = this.data()?.salesByDay ?? [];
    if (points.length === 0) return [];

    const width = 720;
    const height = 176;
    const gap = points.length > 1 ? 7 : 0;
    const barWidth = Math.max(4, (width - gap * (points.length - 1)) / points.length);
    const max = Math.max(1, ...points.map((point) => point.revenue));

    return points.map((point, index) => {
      const barHeight = Math.max(2, (point.revenue / max) * (height - 10));
      return {
        ...point,
        x: index * (barWidth + gap),
        y: height - barHeight,
        width: barWidth,
        height: barHeight
      };
    });
  });

  readonly chartMax = computed(() =>
    Math.max(1, ...(this.data()?.salesByDay ?? []).map((point) => point.revenue))
  );

  readonly chartTotal = computed(() =>
    (this.data()?.salesByDay ?? []).reduce((sum, point) => sum + point.revenue, 0)
  );

  readonly topProductMax = computed(() =>
    Math.max(1, ...(this.data()?.topProducts ?? []).map((product) => product.unitsSold))
  );

  readonly today = new Date();

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.failed.set(false);
    this.admin.getDashboard().subscribe({
      next: (data) => {
        this.data.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.failed.set(true);
      }
    });
  }

  greeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  }

  firstName(): string {
    return this.session.user()?.fullName.split(' ')[0] ?? 'there';
  }

  barLabel(index: number, total: number): boolean {
    const step = total > 20 ? 5 : total > 12 ? 3 : 2;
    return index % step === 0 || index === total - 1;
  }

  statusClass(status: string): string {
    return `status--${status.toLowerCase()}`;
  }
}
