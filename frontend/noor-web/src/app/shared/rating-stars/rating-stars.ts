import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-rating-stars',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="rating" [attr.aria-label]="rating() + ' out of 5 stars'">
      <span class="rating__track" aria-hidden="true">★★★★★</span>
      <span class="rating__fill" [style.width.%]="percent()" aria-hidden="true">★★★★★</span>
    </span>
    @if (showCount()) {
      <span class="rating__count">({{ count() }})</span>
    }
  `,
  styles: [
    `
      :host {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        line-height: 1;
      }
      .rating {
        position: relative;
        display: inline-block;
        font-size: 11px;
        letter-spacing: 2px;
        line-height: 1;
        white-space: nowrap;
      }
      .rating__track {
        color: #d8cdb9;
      }
      .rating__fill {
        position: absolute;
        top: 0;
        left: 0;
        overflow: hidden;
        color: var(--gold);
        white-space: nowrap;
      }
      .rating__count {
        font-size: 10.5px;
        color: var(--muted);
        letter-spacing: 0.03em;
      }
    `
  ]
})
export class RatingStarsComponent {
  readonly rating = input(0);
  readonly count = input(0);
  readonly showCount = input(false);

  readonly percent = computed(() => Math.max(0, Math.min(100, (this.rating() / 5) * 100)));
}
