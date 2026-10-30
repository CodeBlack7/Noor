import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  OrderDetail,
  OrderStatus,
  PaymentStatus
} from '../../core/models/api.models';
import { AdminService } from '../../core/services/admin.service';
import { ToastService } from '../../core/services/toast.service';
import { IconComponent } from '../../shared/icon/icon';

@Component({
  selector: 'app-admin-order-detail-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, CurrencyPipe, DatePipe, IconComponent],
  templateUrl: './order-detail-page.html',
  styleUrl: './order-detail-page.scss'
})
export class AdminOrderDetailPageComponent {
  private readonly admin = inject(AdminService);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  readonly order = signal<OrderDetail | null>(null);
  readonly loading = signal(true);
  readonly failed = signal(false);
  readonly saving = signal(false);

  readonly statuses: OrderStatus[] = [
    'Pending',
    'Confirmed',
    'Processing',
    'Shipped',
    'Delivered',
    'Cancelled',
    'Refunded'
  ];
  readonly paymentStatuses: PaymentStatus[] = ['Pending', 'Paid', 'Failed', 'Refunded'];

  readonly isClosed = computed(() => {
    const status = this.order()?.status;
    return status === 'Cancelled' || status === 'Refunded';
  });

  readonly statusOptions = computed<OrderStatus[]>(() => {
    if (!this.isClosed()) return this.statuses;
    return ['Cancelled', 'Refunded'];
  });

  readonly canCancel = computed(
    () => this.order()?.status === 'Pending' || this.order()?.status === 'Confirmed'
  );

  status: OrderStatus = 'Pending';
  paymentStatus: PaymentStatus = 'Pending';
  carrier = '';
  trackingNumber = '';

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.load(id);
    else {
      this.loading.set(false);
      this.failed.set(true);
    }
  }

  save(): void {
    const order = this.order();
    if (!order || this.saving()) return;

    this.saving.set(true);
    this.admin
      .updateOrderStatus(order.id, {
        status: this.status,
        paymentStatus: this.paymentStatus,
        carrier: this.carrier.trim() || null,
        trackingNumber: this.trackingNumber.trim() || null
      })
      .subscribe({
        next: (updated) => {
          this.order.set(updated);
          this.syncFields(updated);
          this.saving.set(false);
          this.toast.show(`${updated.orderNumber} was updated.`);
        },
        error: () => this.saving.set(false)
      });
  }

  cancelOrder(): void {
    const order = this.order();
    if (!order || this.saving()) return;
    if (!window.confirm(`Cancel ${order.orderNumber}? Stock will be returned to inventory.`)) {
      return;
    }
    this.status = 'Cancelled';
    this.save();
  }

  print(): void {
    window.print();
  }

  statusClass(status: string): string {
    return `status--${status.toLowerCase()}`;
  }

  paymentClass(status: string): string {
    return `status--${status === 'Paid' ? 'paid' : 'unpaid'}`;
  }

  totalUnits(order: OrderDetail): number {
    return order.items.reduce((sum, item) => sum + item.quantity, 0);
  }

  load(id: string): void {
    this.loading.set(true);
    this.failed.set(false);
    this.admin.getOrder(id).subscribe({
      next: (order) => {
        this.order.set(order);
        this.syncFields(order);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.failed.set(true);
      }
    });
  }

  private syncFields(order: OrderDetail): void {
    this.status = order.status;
    this.paymentStatus = order.paymentStatus;
    this.carrier = order.carrier ?? '';
    this.trackingNumber = order.trackingNumber ?? '';
  }
}
