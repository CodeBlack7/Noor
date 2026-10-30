import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { IconComponent } from '../icon/icon';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-toasts',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [IconComponent],
  template: `
    <div class="toasts" aria-live="polite" aria-atomic="false">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast" [class]="'toast toast--' + toast.kind" role="status">
          <app-icon [name]="iconFor(toast.kind)" [size]="17" />
          <p>{{ toast.message }}</p>
          <button
            type="button"
            class="toast__close"
            (click)="toastService.dismiss(toast.id)"
            aria-label="Dismiss"
          >
            <app-icon name="close" [size]="14" />
          </button>
        </div>
      }
    </div>
  `,
  styleUrl: './toasts.scss'
})
export class ToastsComponent {
  readonly toastService = inject(ToastService);

  iconFor(kind: string): string {
    if (kind === 'error') return 'alert-circle';
    if (kind === 'info') return 'info';
    return 'check-circle';
  }
}
