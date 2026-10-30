import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CatalogService } from '../../core/services/catalog.service';
import { ToastService } from '../../core/services/toast.service';
import { IconComponent } from '../icon/icon';

@Component({
  selector: 'app-footer',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, IconComponent],
  templateUrl: './footer.html',
  styleUrl: './footer.scss'
})
export class FooterComponent {
  private readonly catalog = inject(CatalogService);
  private readonly toast = inject(ToastService);

  readonly categories = this.catalog.categories;
  readonly submitting = signal(false);
  readonly year = new Date().getFullYear();

  newsletterEmail = '';

  constructor() {
    this.catalog.loadCategories();
  }

  subscribe(event: Event): void {
    event.preventDefault();
    const email = this.newsletterEmail.trim();
    if (!email || this.submitting()) return;

    this.submitting.set(true);
    this.catalog.subscribeNewsletter(email).subscribe({
      next: (response) => {
        this.toast.show(response.message || 'Welcome to the Noor circle.');
        this.newsletterEmail = '';
        this.submitting.set(false);
      },
      error: () => this.submitting.set(false)
    });
  }
}
