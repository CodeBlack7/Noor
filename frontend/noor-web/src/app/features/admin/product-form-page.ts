import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  computed,
  inject,
  signal
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  AdminCategory,
  ImageInput,
  ProductUpsertRequest,
  VariantInput
} from '../../core/models/api.models';
import { AdminService } from '../../core/services/admin.service';
import { ToastService } from '../../core/services/toast.service';
import { IconComponent } from '../../shared/icon/icon';

interface FormImage {
  id?: number;
  url: string;
  altText: string;
}

interface FormVariant {
  id: number | null;
  size: string;
  colorName: string;
  colorHex: string;
  sku: string;
  stockQuantity: number;
  priceAdjustment: number;
}

interface ProductFormState {
  name: string;
  slug: string;
  categoryId: number | null;
  shortDescription: string;
  description: string;
  price: number | null;
  compareAtPrice: number | null;
  fabric: string;
  careInstructions: string;
  isActive: boolean;
  isFeatured: boolean;
  images: FormImage[];
  variants: FormVariant[];
}

const DEFAULT_HEX = '#1c1b19';
const MAX_UPLOAD_BYTES = 5 * 1024 * 1024;

function emptyForm(): ProductFormState {
  return {
    name: '',
    slug: '',
    categoryId: null,
    shortDescription: '',
    description: '',
    price: null,
    compareAtPrice: null,
    fabric: '',
    careInstructions: '',
    isActive: true,
    isFeatured: false,
    images: [],
    variants: []
  };
}

@Component({
  selector: 'app-admin-product-form-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, IconComponent],
  templateUrl: './product-form-page.html',
  styleUrl: './product-form-page.scss'
})
export class AdminProductFormPageComponent {
  private readonly admin = inject(AdminService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly productId: number | null = this.resolveProductId();

  readonly categories = signal<AdminCategory[]>([]);
  readonly loading = signal(true);
  readonly failed = signal(false);
  readonly saving = signal(false);
  readonly uploading = signal(false);
  readonly isEditing = computed(() => this.productId !== null);

  form: ProductFormState = emptyForm();

  newImageUrl = '';
  quickColorName = '';
  quickColorHex = DEFAULT_HEX;
  quickSizes = 'S, M, L, XL';

  constructor() {
    this.admin.getCategories().subscribe({
      next: (categories) => {
        this.categories.set(categories);
        if (!this.productId && this.form.categoryId === null && categories.length > 0) {
          this.form.categoryId = categories[0].id;
        }
        this.cdr.markForCheck();
      },
      error: () => undefined
    });

    if (this.productId) {
      this.loadProduct(this.productId);
    } else {
      this.loading.set(false);
    }
  }

  addImageUrl(): void {
    const url = this.newImageUrl.trim();
    if (!url) {
      this.toast.show('Paste an image URL first.', 'info');
      return;
    }
    this.form.images.push({ url, altText: this.form.name });
    this.newImageUrl = '';
  }

  uploadImages(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';
    if (files.length === 0) return;

    const oversized = files.find((file) => file.size > MAX_UPLOAD_BYTES);
    if (oversized) {
      this.toast.show(`${oversized.name} is larger than 5 MB.`, 'info');
      return;
    }

    this.uploading.set(true);
    let remaining = files.length;

    for (const file of files) {
      this.admin.upload(file).subscribe({
        next: (response) => {
          this.form.images.push({ url: response.url, altText: this.form.name });
          this.finishUpload(() => --remaining === 0);
        },
        error: () => this.finishUpload(() => --remaining === 0)
      });
    }
  }

  moveImage(index: number, direction: -1 | 1): void {
    const target = index + direction;
    if (target < 0 || target >= this.form.images.length) return;
    const images = this.form.images;
    [images[index], images[target]] = [images[target], images[index]];
  }

  removeImage(index: number): void {
    this.form.images.splice(index, 1);
  }

  addVariant(): void {
    this.form.variants.push({
      id: null,
      size: '',
      colorName: '',
      colorHex: DEFAULT_HEX,
      sku: '',
      stockQuantity: 10,
      priceAdjustment: 0
    });
  }

  removeVariant(index: number): void {
    this.form.variants.splice(index, 1);
  }

  addSizeRun(): void {
    const colorName = this.quickColorName.trim();
    const sizes = this.quickSizes
      .split(',')
      .map((size) => size.trim())
      .filter(Boolean);

    if (!colorName || sizes.length === 0) {
      this.toast.show('Add a colour name and at least one size.', 'info');
      return;
    }

    let added = 0;
    for (const size of sizes) {
      const exists = this.form.variants.some(
        (variant) =>
          variant.size.trim().toLowerCase() === size.toLowerCase() &&
          variant.colorName.trim().toLowerCase() === colorName.toLowerCase()
      );
      if (exists) continue;

      const variant: FormVariant = {
        id: null,
        size,
        colorName,
        colorHex: this.validHex(this.quickColorHex),
        sku: '',
        stockQuantity: 10,
        priceAdjustment: 0
      };
      variant.sku = this.buildSku(variant);
      this.form.variants.push(variant);
      added += 1;
    }

    if (added === 0) {
      this.toast.show('Those size and colour combinations already exist.', 'info');
    } else {
      this.toast.show(`${added} ${added === 1 ? 'variant' : 'variants'} added.`);
    }
  }

  generateSku(variant: FormVariant): void {
    variant.sku = this.buildSku(variant);
  }

  save(): void {
    if (this.saving()) return;

    const error = this.validate();
    if (error) {
      this.toast.show(error, 'info');
      return;
    }

    this.saving.set(true);
    const request = this.toRequest();
    const operation = this.productId
      ? this.admin.updateProduct(this.productId, request)
      : this.admin.createProduct(request);

    operation.subscribe({
      next: (product) => {
        this.toast.show(
          this.productId
            ? `${product.name} was updated.`
            : `${product.name} was added to the catalogue.`
        );
        this.router.navigate(['/admin/products']);
      },
      error: () => this.saving.set(false)
    });
  }

  cancel(): void {
    this.router.navigate(['/admin/products']);
  }

  private loadProduct(id: number): void {
    this.loading.set(true);
    this.failed.set(false);

    this.admin.getProduct(id).subscribe({
      next: (product) => {
        this.form = {
          name: product.name,
          slug: product.slug,
          categoryId: product.categoryId,
          shortDescription: product.shortDescription,
          description: product.description,
          price: product.price,
          compareAtPrice: product.compareAtPrice,
          fabric: product.fabric,
          careInstructions: product.careInstructions ?? '',
          isActive: product.isActive,
          isFeatured: product.isFeatured,
          images: product.images.map((image) => ({
            id: image.id,
            url: image.url,
            altText: image.altText ?? ''
          })),
          variants: product.variants.map((variant) => ({
            id: variant.id,
            size: variant.size,
            colorName: variant.colorName,
            colorHex: variant.colorHex || DEFAULT_HEX,
            sku: variant.sku,
            stockQuantity: variant.stockQuantity,
            priceAdjustment: variant.priceAdjustment ?? 0
          }))
        };
        this.quickColorName = product.colors[0]?.name ?? '';
        this.loading.set(false);
        this.cdr.markForCheck();
      },
      error: () => {
        this.loading.set(false);
        this.failed.set(true);
      }
    });
  }

  private validate(): string | null {
    if (!this.form.name.trim()) return 'Give the product a name.';
    if (!this.form.categoryId) return 'Choose a collection.';
    if (!this.form.shortDescription.trim()) return 'Add a short description.';
    if (!this.form.description.trim()) return 'Add the full description.';
    if (!this.form.price || this.form.price <= 0) return 'Set a selling price above zero.';
    if (!this.form.fabric.trim()) return 'Describe the fabric.';
    if (
      this.form.compareAtPrice !== null &&
      this.form.compareAtPrice > 0 &&
      this.form.compareAtPrice <= this.form.price
    ) {
      return 'The compare-at price must be higher than the selling price.';
    }
    if (this.form.images.length === 0) return 'Add at least one product image.';
    if (this.form.images.some((image) => !image.url.trim())) {
      return 'Every image needs a URL.';
    }
    if (this.form.variants.length === 0) return 'Add at least one variant.';

    for (const variant of this.form.variants) {
      if (!variant.size.trim() || !variant.colorName.trim() || !variant.sku.trim()) {
        return 'Every variant needs a size, colour, and SKU.';
      }
      if (variant.stockQuantity < 0) return 'Stock cannot be negative.';
    }

    const skus = this.form.variants.map((variant) => variant.sku.trim().toLowerCase());
    if (new Set(skus).size !== skus.length) return 'Each SKU must be unique.';

    const combinations = this.form.variants.map(
      (variant) => `${variant.size.trim().toLowerCase()}|${variant.colorName.trim().toLowerCase()}`
    );
    if (new Set(combinations).size !== combinations.length) {
      return 'Each size and colour combination must be unique.';
    }

    return null;
  }

  private toRequest(): ProductUpsertRequest {
    return {
      name: this.form.name.trim(),
      slug: this.form.slug.trim() || null,
      categoryId: this.form.categoryId!,
      shortDescription: this.form.shortDescription.trim(),
      description: this.form.description.trim(),
      price: this.form.price!,
      compareAtPrice:
        this.form.compareAtPrice && this.form.compareAtPrice > 0
          ? this.form.compareAtPrice
          : null,
      fabric: this.form.fabric.trim(),
      careInstructions: this.form.careInstructions.trim() || null,
      isActive: this.form.isActive,
      isFeatured: this.form.isFeatured,
      images: this.form.images.map<ImageInput>((image) => ({
        url: image.url.trim(),
        altText: image.altText.trim() || null
      })),
      variants: this.form.variants.map<VariantInput>((variant) => ({
        id: variant.id,
        size: variant.size.trim(),
        colorName: variant.colorName.trim(),
        colorHex: this.validHex(variant.colorHex),
        sku: variant.sku.trim().toUpperCase(),
        stockQuantity: Math.round(variant.stockQuantity),
        priceAdjustment: Number(variant.priceAdjustment) || 0
      }))
    };
  }

  private buildSku(variant: FormVariant): string {
    const base = (this.form.name || 'NOOR')
      .toUpperCase()
      .replace(/[^A-Z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '')
      .slice(0, 12);
    const color = (variant.colorName || 'COL')
      .toUpperCase()
      .replace(/[^A-Z0-9]+/g, '')
      .slice(0, 6);
    const size = (variant.size || 'SZ')
      .toUpperCase()
      .replace(/[^A-Z0-9]+/g, '')
      .slice(0, 4);
    return `${base || 'NOOR'}-${color || 'COL'}-${size || 'SZ'}`;
  }

  private validHex(value: string): string {
    return /^#[0-9a-fA-F]{6}$/.test(value) ? value : DEFAULT_HEX;
  }

  private finishUpload(complete: () => boolean): void {
    if (complete()) {
      this.uploading.set(false);
      this.cdr.markForCheck();
    }
  }

  private resolveProductId(): number | null {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return null;
    const parsed = Number(id);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : null;
  }
}
