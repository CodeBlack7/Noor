import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';
import { IconComponent } from '../icon/icon';

@Component({
  selector: 'app-quantity-stepper',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [IconComponent],
  template: `
    <div class="qty">
      <button
        type="button"
        (click)="step(-1)"
        [disabled]="value() <= min()"
        aria-label="Decrease quantity"
      >
        <app-icon name="minus" [size]="13" />
      </button>
      <input
        type="number"
        [value]="value()"
        [min]="min()"
        [max]="max()"
        (change)="onInput($event)"
        aria-label="Quantity"
      />
      <button
        type="button"
        (click)="step(1)"
        [disabled]="value() >= max()"
        aria-label="Increase quantity"
      >
        <app-icon name="plus" [size]="13" />
      </button>
    </div>
  `,
  styles: [
    `
      .qty {
        display: inline-flex;
        align-items: center;
        border: 1px solid var(--line);
        background: var(--white);
        height: 46px;
      }
      button {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 38px;
        height: 100%;
        border: 0;
        background: transparent;
        color: var(--ink);
        transition: background 0.2s ease;
      }
      button:hover:not(:disabled) {
        background: var(--gold-wash);
      }
      button:disabled {
        opacity: 0.3;
      }
      input {
        width: 38px;
        height: 100%;
        border: 0;
        background: transparent;
        text-align: center;
        font-size: 12.5px;
        font-weight: 600;
        -moz-appearance: textfield;
        appearance: textfield;
      }
      input::-webkit-outer-spin-button,
      input::-webkit-inner-spin-button {
        -webkit-appearance: none;
        margin: 0;
      }
      input:focus {
        outline: none;
      }
    `
  ]
})
export class QuantityStepperComponent {
  readonly value = model(1);
  readonly min = input(1);
  readonly max = input(20);

  step(delta: number): void {
    this.value.set(this.clamp(this.value() + delta));
  }

  onInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const next = this.clamp(Number(input.value) || this.min());
    this.value.set(next);
    input.value = String(next);
  }

  private clamp(value: number): number {
    return Math.max(this.min(), Math.min(this.max(), Math.round(value)));
  }
}
