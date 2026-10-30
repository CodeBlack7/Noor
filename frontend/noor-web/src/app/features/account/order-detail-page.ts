import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { OrderDetail, OrderStatus } from '../../core/models/api.models';
import { OrderService } from '../../core/services/order.service';
import { ToastService } from '../../core/services/toast.service';
import { IconComponent } from '../../shared/icon/icon';

const FLOW: OrderStatus[] = ['Pending', 'Confirmed', 'Processing', 'Shipped', 'Delivered'];

@Component({
  selector: 'app-order-detail-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, CurrencyPipe, DatePipe, IconComponent],
  templateUrl: './order-detail-page.html',
  styleUrl: './order-detail-page.scss'
})
export class OrderDetailPageComponent {
  readonly id = input('');

  private readonly orders = inject(OrderService);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  readonly order = signal<OrderDetail | null>(null);
  readonly loading = signal(true);
  readonly failed = signal(false);
  readonly canceling = signal(false);
  readonly justPlaced = this.route.snapshot.queryParamMap.get('placed') === '1';

  readonly flow = FLOW;
  readonly currentStep = computed(() => {
    const status = this.order()?.status;
    return status ? FLOW.indexOf(status) : -1;
  });
  readonly canCancel = computed(
    () => this.order()?.status === 'Pending' || this.order()?.status === 'Confirmed'
  );

  constructor() {
    effect(() => {
      const id = this.id();
      if (id) this.load(id);
    });
  }

  cancel(): void {
    const order = this.order();
    if (!order || this.canceling()) return;
    if (!window.confirm(`Cancel order ${order.orderNumber}? This cannot be undone.`)) return;

    this.canceling.set(true);
    this.orders.cancel(order.id).subscribe({
      next: (updated) => {
        this.order.set(updated);
        this.canceling.set(false);
        this.toast.show('Order cancelled.', 'info');
      },
      error: () => this.canceling.set(false)
    });
  }

  statusClass(status: string): string {
    return `status--${status.toLowerCase()}`;
  }

  private load(id: string): void {
    this.loading.set(true);
    this.failed.set(false);
    this.orders.getOrder(id).subscribe({
      next: (order) => {
        this.order.set(order);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.failed.set(true);
      }
    });
  }
}
