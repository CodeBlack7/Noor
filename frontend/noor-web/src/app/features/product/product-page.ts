import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Title } from '@angular/platform-browser';
import {
  ProductDetail,
  ProductVariant,
  Review
} from '../../core/models/api.models';
import { CartService } from '../../core/services/cart.service';
import { CatalogService } from '../../core/services/catalog.service';
import { SessionService } from '../../core/services/session.service';
import { ToastService } from '../../core/services/toast.service';
import { WishlistService } from '../../core/services/wishlist.service';
import { IconComponent } from '../../shared/icon/icon';
import { ProductCardComponent } from '../../shared/product-card/product-card';
import { QuantityStepperComponent } from '../../shared/quantity-stepper/quantity-stepper';
import { RatingStarsComponent } from '../../shared/rating-stars/rating-stars';

type ProductTab = 'details' | 'care' | 'shipping';

@Component({
  selector: 'app-product-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    FormsModule,
    CurrencyPipe,
    DatePipe,
    DecimalPipe,
    IconComponent,
    ProductCardComponent,
    QuantityStepperComponent,
    RatingStarsComponent
  ],
  templateUrl: './product-page.html',
  styleUrl: './product-page.scss'
})
export class ProductPageComponent {
  readonly slug = input('');

  private readonly catalog = inject(CatalogService);
  readonly cart = inject(CartService);
  private readonly wishlist = inject(WishlistService);
  private readonly session = inject(SessionService);
  private readonly toast = inject(ToastService);
  private readonly title = inject(Title);

  readonly product = signal<ProductDetail | null>(null);
  readonly loading = signal(true);
  readonly failed = signal(false);
  readonly activeImage = signal(0);
  readonly selectedColor = signal('');
  readonly selectedSize = signal('');
  readonly quantity = signal(1);
  readonly activeTab = signal<ProductTab>('details');

  readonly reviews = signal<Review[]>([]);
  readonly reviewCount = signal(0);
  readonly averageRating = signal(0);
  readonly submittingReview = signal(false);
  reviewRating = 5;
  reviewTitle = '';
  reviewComment = '';

  readonly isAuthenticated = this.session.isAuthenticated;
  readonly saved = computed(() => {
    const product = this.product();
    return product ? this.wishlist.ids().has(product.id) : false;
  });

  readonly colorVariants = computed(() => {
    const product = this.product();
    if (!product) return [] as ProductVariant[];
    return product.variants.filter((variant) => variant.colorName === this.selectedColor());
  });

  readonly sizesForColor = computed(() => {
    const seen = new Set<string>();
    const list: { size: string; inStock: boolean }[] = [];
    for (const variant of this.colorVariants()) {
      if (seen.has(variant.size)) continue;
      seen.add(variant.size);
      list.push({ size: variant.size, inStock: variant.stockQuantity > 0 });
    }
    return list;
  });

  readonly selectedVariant = computed(() => {
    return (
      this.colorVariants().find((variant) => variant.size === this.selectedSize()) ?? null
    );
  });

  readonly activeImageUrl = computed(() => {
    const product = this.product();
    if (!product || product.images.length === 0) return null;
    return product.images[Math.min(this.activeImage(), product.images.length - 1)]?.url ?? null;
  });

  readonly stockMessage = computed(() => {
    const variant = this.selectedVariant();
    if (!variant) return 'Select a size';
    if (variant.stockQuantity <= 0) return 'Sold out in this combination';
    if (variant.stockQuantity <= 4) return `Only ${variant.stockQuantity} left`;
    return 'In stock, ready to send';
  });

  readonly maxQuantity = computed(() => {
    const variant = this.selectedVariant();
    return variant ? Math.max(1, variant.stockQuantity) : 1;
  });

  readonly canAdd = computed(() => {
    const variant = this.selectedVariant();
    return !!variant && variant.stockQuantity > 0;
  });

  constructor() {
    effect(() => {
      const slug = this.slug();
      if (slug) this.load(slug);
    });
  }

  selectColor(color: string): void {
    this.selectedColor.set(color);
    const sizes = this.colorVariants();
    const current = sizes.find((variant) => variant.size === this.selectedSize());
    if (!current || current.stockQuantity <= 0) {
      const available = sizes.find((variant) => variant.stockQuantity > 0) ?? sizes[0];
      this.selectedSize.set(available?.size ?? '');
    }
    this.quantity.set(1);
  }

  selectSize(size: string): void {
    this.selectedSize.set(size);
    this.quantity.set(1);
  }

  addToBag(): void {
    const variant = this.selectedVariant();
    if (!variant || variant.stockQuantity <= 0) return;
    this.cart.add(variant.id, this.quantity());
  }

  toggleWishlist(): void {
    const product = this.product();
    if (product) this.wishlist.toggle(product.id);
  }

  setTab(tab: ProductTab): void {
    this.activeTab.set(tab);
  }

  discountPercent(product: ProductDetail): number {
    if (!product.compareAtPrice || product.compareAtPrice <= product.price) return 0;
    return Math.round(((product.compareAtPrice - product.price) / product.compareAtPrice) * 100);
  }

  submitReview(): void {
    const product = this.product();
    if (!product || this.submittingReview()) return;

    if (!this.reviewComment.trim()) {
      this.toast.show('Please share a few words about the piece.', 'info');
      return;
    }

    this.submittingReview.set(true);
    this.catalog
      .addReview(product.id, {
        rating: Number(this.reviewRating),
        title: this.reviewTitle.trim() || null,
        comment: this.reviewComment.trim()
      })
      .subscribe({
        next: (review) => {
          this.reviews.update((items) => [review, ...items]);
          this.reviewCount.update((count) => count + 1);
          this.averageRating.set(
            this.reviews().reduce((sum, item) => sum + item.rating, 0) /
              this.reviews().length
          );
          this.reviewTitle = '';
          this.reviewComment = '';
          this.reviewRating = 5;
          this.submittingReview.set(false);
          this.toast.show('Thank you — your review is now live.');
        },
        error: () => this.submittingReview.set(false)
      });
  }

  private load(slug: string): void {
    this.loading.set(true);
    this.failed.set(false);

    this.catalog.getProduct(slug).subscribe({
      next: (product) => {
        this.product.set(product);
        this.activeImage.set(0);
        this.reviews.set(product.reviews);
        this.reviewCount.set(product.reviewCount);
        this.averageRating.set(product.averageRating);
        this.title.setTitle(`${product.name} — Noor`);

        const colors = product.colors.map((color) => color.name);
        const firstColor =
          colors.find((color) =>
            product.variants.some(
              (variant) => variant.colorName === color && variant.stockQuantity > 0
            )
          ) ??
          colors[0] ??
          '';
        this.selectedColor.set(firstColor);

        const variants = product.variants.filter(
          (variant) => variant.colorName === firstColor
        );
        const firstSize =
          variants.find((variant) => variant.stockQuantity > 0)?.size ??
          variants[0]?.size ??
          '';
        this.selectedSize.set(firstSize);
        this.quantity.set(1);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.failed.set(true);
      }
    });
  }
}
