import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { IconComponent } from '../../shared/icon/icon';

@Component({
  selector: 'app-not-found-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, IconComponent],
  template: `
    <section class="not-found">
      <div class="container not-found__inner">
        <p class="eyebrow">A quiet detour</p>
        <p class="not-found__code">404</p>
        <h1>This page has slipped away.</h1>
        <p class="not-found__copy">
          The link may have changed, or the page may have been retired. The collection,
          however, is right where you left it.
        </p>
        <div class="not-found__actions">
          <a class="btn" routerLink="/shop">
            Explore the collection
            <app-icon name="arrow-right" [size]="14" />
          </a>
          <a class="btn btn--outline" routerLink="/">Return home</a>
        </div>
        <div class="not-found__links">
          <a routerLink="/our-story">Our story</a>
          <span>·</span>
          <a routerLink="/size-guide">Size guide</a>
          <span>·</span>
          <a routerLink="/contact">Client care</a>
        </div>
      </div>
    </section>
  `,
  styleUrl: './not-found-page.scss'
})
export class NotFoundPageComponent {}
