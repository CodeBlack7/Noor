import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductSummary } from '../../core/models/api.models';
import { WishlistService } from '../../core/services/wishlist.service';
import { IconComponent } from '../icon/icon';
import { RatingStarsComponent } from '../rating-stars/rating-stars';

@Component({
  selector: 'app-product-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, CurrencyPipe, IconComponent, RatingStarsComponent],
  templateUrl: './product-card.html',
  styleUrl: './product-card.scss'
})
export class ProductCardComponent {
  readonly product = input.required<ProductSummary>();

  private readonly wishlist = inject(WishlistService);

  readonly saved = computed(() => this.wishlist.ids().has(this.product().id));
  readonly discount = computed(() => {
    const product = this.product();
    if (!product.compareAtPrice || product.compareAtPrice <= product.price) return 0;
    return Math.round(((product.compareAtPrice - product.price) / product.compareAtPrice) * 100);
  });
  readonly visibleColors = computed(() => this.product().colors.slice(0, 4));

  toggleWishlist(event: MouseEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.wishlist.toggle(this.product().id);
  }
}
