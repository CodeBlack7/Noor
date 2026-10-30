import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastService } from '../../core/services/toast.service';
import { IconComponent } from '../../shared/icon/icon';

type InfoPageKind = 'story' | 'contact' | 'shipping' | 'sizes';

@Component({
  selector: 'app-info-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, FormsModule, IconComponent],
  templateUrl: './info-page.html',
  styleUrl: './info-page.scss'
})
export class InfoPageComponent {
  readonly page = input<InfoPageKind>('story');

  private readonly toast = inject(ToastService);

  readonly isContact = computed(() => this.page() === 'contact');
  readonly sent = signal(false);

  contact = {
    name: '',
    email: '',
    subject: 'A question about a piece',
    message: ''
  };

  submitContact(): void {
    if (!this.contact.name.trim() || !this.contact.email.trim() || !this.contact.message.trim()) {
      this.toast.show('Please complete your name, email, and message.', 'info');
      return;
    }

    this.sent.set(true);
    this.toast.show('Thank you — client care will reply within one business day.');
    this.contact = {
      name: '',
      email: '',
      subject: 'A question about a piece',
      message: ''
    };
  }
}
