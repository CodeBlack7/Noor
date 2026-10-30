import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<svg [attr.width]="size()" [attr.height]="size()" aria-hidden="true"><use [attr.href]="'#i-' + name()"></use></svg>`,
  styles: [
    `
      :host {
        display: inline-flex;
        flex: none;
      }
      svg {
        display: block;
      }
    `
  ]
})
export class IconComponent {
  readonly name = input.required<string>();
  readonly size = input(20);
}
