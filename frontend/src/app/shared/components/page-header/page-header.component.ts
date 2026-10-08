import { Component, input } from '@angular/core';
import { Params, RouterLink } from '@angular/router';

import { IconComponent } from '../icon/icon.component';

/**
 * Title block of a page.
 *
 *   <app-page-header title="Plan Catalogue" subtitle="Manage and view all membership plans.">
 *     <span pageBadge class="mode-badge">View mode</span>   <!-- next to the title -->
 *     <button class="btn btn-primary">+ Add Plan</button>   <!-- right side (actions) -->
 *   </app-page-header>
 */
@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [RouterLink, IconComponent],
  templateUrl: './page-header.component.html',
  styleUrl: './page-header.component.css'
})
export class PageHeaderComponent {
  readonly title = input<string>('');
  readonly subtitle = input<string>('');

  /** Router link of the "back" link (null hides it). */
  readonly backLink = input<string | null>(null);
  readonly backLabel = input<string>('Back');
  readonly backQueryParams = input<Params | null>(null);
}
