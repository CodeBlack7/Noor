import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  AdminCategory,
  CategoryUpsertRequest
} from '../../core/models/api.models';
import { AdminService } from '../../core/services/admin.service';
import { ToastService } from '../../core/services/toast.service';
import { IconComponent } from '../../shared/icon/icon';

interface CategoryFormState {
  name: string;
  slug: string;
  description: string;
  imageUrl: string;
  isActive: boolean;
  sortOrder: number;
}

function emptyCategory(sortOrder: number): CategoryFormState {
  return {
    name: '',
    slug: '',
    description: '',
    imageUrl: '',
    isActive: true,
    sortOrder
  };
}

@Component({
  selector: 'app-admin-categories-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, IconComponent],
  templateUrl: './categories-page.html',
  styleUrl: './categories-page.scss'
})
export class AdminCategoriesPageComponent {
  private readonly admin = inject(AdminService);
  private readonly toast = inject(ToastService);

  readonly categories = signal<AdminCategory[]>([]);
  readonly loading = signal(true);
  readonly failed = signal(false);
  readonly editorOpen = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly saving = signal(false);
  readonly deletingId = signal<number | null>(null);

  form: CategoryFormState = emptyCategory(1);

  constructor() {
    this.load();
  }

  startCreate(): void {
    const nextSort =
      this.categories().reduce((max, category) => Math.max(max, category.sortOrder), 0) + 1;
    this.editingId.set(null);
    this.form = emptyCategory(nextSort);
    this.editorOpen.set(true);
  }

  startEdit(category: AdminCategory): void {
    this.editingId.set(category.id);
    this.form = {
      name: category.name,
      slug: category.slug,
      description: category.description ?? '',
      imageUrl: category.imageUrl ?? '',
      isActive: category.isActive,
      sortOrder: category.sortOrder
    };
    this.editorOpen.set(true);
  }

  cancelEditor(): void {
    this.editorOpen.set(false);
    this.editingId.set(null);
  }

  save(): void {
    if (this.saving()) return;

    if (!this.form.name.trim()) {
      this.toast.show('Give the collection a name.', 'info');
      return;
    }

    const request: CategoryUpsertRequest = {
      name: this.form.name.trim(),
      slug: this.form.slug.trim() || null,
      description: this.form.description.trim() || null,
      imageUrl: this.form.imageUrl.trim() || null,
      isActive: this.form.isActive,
      sortOrder: Number(this.form.sortOrder) || 0
    };

    this.saving.set(true);
    const id = this.editingId();
    const operation = id
      ? this.admin.updateCategory(id, request)
      : this.admin.createCategory(request);

    operation.subscribe({
      next: (category) => {
        this.saving.set(false);
        this.editorOpen.set(false);
        this.editingId.set(null);
        this.toast.show(
          id ? `${category.name} was updated.` : `${category.name} was added.`
        );
        this.load();
      },
      error: () => this.saving.set(false)
    });
  }

  remove(category: AdminCategory): void {
    if (this.deletingId()) return;
    const warning =
      category.productCount > 0
        ? `“${category.name}” still has ${category.productCount} products. Collections can only be deleted when empty. Continue?`
        : `Delete the “${category.name}” collection?`;
    if (!window.confirm(warning)) return;

    this.deletingId.set(category.id);
    this.admin.deleteCategory(category.id).subscribe({
      next: (result) => {
        this.deletingId.set(null);
        this.toast.show(result.message);
        this.load();
      },
      error: () => this.deletingId.set(null)
    });
  }

  load(): void {
    this.loading.set(true);
    this.failed.set(false);
    this.admin.getCategories().subscribe({
      next: (categories) => {
        this.categories.set(categories);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.failed.set(true);
      }
    });
  }
}
