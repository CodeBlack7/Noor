import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  Coupon,
  CouponType,
  CouponUpsertRequest
} from '../../core/models/api.models';
import { AdminService } from '../../core/services/admin.service';
import { ToastService } from '../../core/services/toast.service';
import { IconComponent } from '../../shared/icon/icon';

type CouponState = 'live' | 'scheduled' | 'expired' | 'paused' | 'exhausted';

interface CouponFormState {
  code: string;
  description: string;
  discountType: CouponType;
  value: number | null;
  minOrderAmount: number | null;
  maxRedemptions: number | null;
  startsAt: string;
  endsAt: string;
  isActive: boolean;
}

const STATE_LABELS: Record<CouponState, string> = {
  live: 'Live',
  scheduled: 'Scheduled',
  expired: 'Expired',
  paused: 'Paused',
  exhausted: 'Redeemed out'
};

function emptyCoupon(): CouponFormState {
  return {
    code: '',
    description: '',
    discountType: 'Percentage',
    value: 10,
    minOrderAmount: 0,
    maxRedemptions: null,
    startsAt: '',
    endsAt: '',
    isActive: true
  };
}

function toLocalInput(value: string | null): string {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  const pad = (part: number) => String(part).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(
    date.getHours()
  )}:${pad(date.getMinutes())}`;
}

function toIso(value: string): string | null {
  if (!value) return null;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date.toISOString();
}

@Component({
  selector: 'app-admin-coupons-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, CurrencyPipe, DatePipe, IconComponent],
  templateUrl: './coupons-page.html',
  styleUrl: './coupons-page.scss'
})
export class AdminCouponsPageComponent {
  private readonly admin = inject(AdminService);
  private readonly toast = inject(ToastService);

  readonly coupons = signal<Coupon[]>([]);
  readonly loading = signal(true);
  readonly failed = signal(false);
  readonly editorOpen = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly saving = signal(false);
  readonly deletingId = signal<number | null>(null);

  readonly discountTypes: CouponType[] = ['Percentage', 'FixedAmount'];

  form: CouponFormState = emptyCoupon();

  constructor() {
    this.load();
  }

  startCreate(): void {
    this.editingId.set(null);
    this.form = emptyCoupon();
    this.editorOpen.set(true);
  }

  startEdit(coupon: Coupon): void {
    this.editingId.set(coupon.id);
    this.form = {
      code: coupon.code,
      description: coupon.description ?? '',
      discountType: coupon.discountType,
      value: coupon.value,
      minOrderAmount: coupon.minOrderAmount,
      maxRedemptions: coupon.maxRedemptions,
      startsAt: toLocalInput(coupon.startsAt),
      endsAt: toLocalInput(coupon.endsAt),
      isActive: coupon.isActive
    };
    this.editorOpen.set(true);
  }

  cancelEditor(): void {
    this.editorOpen.set(false);
    this.editingId.set(null);
  }

  save(): void {
    if (this.saving()) return;

    const code = this.form.code.trim().toUpperCase();
    if (!code) {
      this.toast.show('Give the coupon a code.', 'info');
      return;
    }
    if (!/^[A-Z0-9_-]{3,32}$/.test(code)) {
      this.toast.show('Codes use 3–32 letters, numbers, hyphens, or underscores.', 'info');
      return;
    }
    if (this.form.value === null || this.form.value <= 0) {
      this.toast.show('Set a discount above zero.', 'info');
      return;
    }
    if (this.form.discountType === 'Percentage' && this.form.value > 100) {
      this.toast.show('A percentage discount cannot exceed 100%.', 'info');
      return;
    }

    const startsAt = toIso(this.form.startsAt);
    const endsAt = toIso(this.form.endsAt);
    if (startsAt && endsAt && new Date(endsAt) <= new Date(startsAt)) {
      this.toast.show('The end date must be after the start date.', 'info');
      return;
    }

    const request: CouponUpsertRequest = {
      code,
      description: this.form.description.trim() || null,
      discountType: this.form.discountType,
      value: Number(this.form.value),
      minOrderAmount: Number(this.form.minOrderAmount) || 0,
      maxRedemptions:
        this.form.maxRedemptions && this.form.maxRedemptions > 0
          ? Math.round(this.form.maxRedemptions)
          : null,
      startsAt,
      endsAt,
      isActive: this.form.isActive
    };

    this.saving.set(true);
    const id = this.editingId();
    const operation = id
      ? this.admin.updateCoupon(id, request)
      : this.admin.createCoupon(request);

    operation.subscribe({
      next: (coupon) => {
        this.saving.set(false);
        this.editorOpen.set(false);
        this.editingId.set(null);
        this.toast.show(id ? `${coupon.code} was updated.` : `${coupon.code} is ready to use.`);
        this.load();
      },
      error: () => this.saving.set(false)
    });
  }

  remove(coupon: Coupon): void {
    if (this.deletingId()) return;
    if (
      !window.confirm(
        `Delete the ${coupon.code} coupon? Past orders keep their discount, but it can no longer be redeemed.`
      )
    ) {
      return;
    }

    this.deletingId.set(coupon.id);
    this.admin.deleteCoupon(coupon.id).subscribe({
      next: () => {
        this.deletingId.set(null);
        this.toast.show(`${coupon.code} was deleted.`);
        this.load();
      },
      error: () => this.deletingId.set(null)
    });
  }

  stateOf(coupon: Coupon): CouponState {
    if (!coupon.isActive) return 'paused';
    if (coupon.maxRedemptions !== null && coupon.timesRedeemed >= coupon.maxRedemptions) {
      return 'exhausted';
    }
    const now = Date.now();
    if (coupon.startsAt && new Date(coupon.startsAt).getTime() > now) return 'scheduled';
    if (coupon.endsAt && new Date(coupon.endsAt).getTime() < now) return 'expired';
    return 'live';
  }

  stateLabel(coupon: Coupon): string {
    return STATE_LABELS[this.stateOf(coupon)];
  }

  discountLabel(coupon: Coupon): string {
    return coupon.discountType === 'Percentage'
      ? `${coupon.value}% off`
      : `₹${coupon.value.toLocaleString('en-IN')} off`;
  }

  load(): void {
    this.loading.set(true);
    this.failed.set(false);
    this.admin.getCoupons().subscribe({
      next: (coupons) => {
        this.coupons.set(coupons);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.failed.set(true);
      }
    });
  }
}
