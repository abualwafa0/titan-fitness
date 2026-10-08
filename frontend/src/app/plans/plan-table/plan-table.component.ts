import { CurrencyPipe } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { RowMenuComponent, RowMenuItem } from '../../shared/components/row-menu/row-menu.component';
import { SortChange, SortDirection, SortHeaderComponent } from '../../shared/components/sort-header/sort-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { DurationLabelPipe } from '../../shared/pipes/duration-label.pipe';
import { FreezeAllowancePipe } from '../../shared/pipes/freeze-allowance.pipe';
import { HighlightPipe } from '../../shared/pipes/highlight.pipe';
import { PlanListItem, accessLabel } from '../plan.model';

/** Table of the plan catalogue (child component: data comes in, sort events go out). */
@Component({
  selector: 'app-plan-table',
  standalone: true,
  imports: [
    CurrencyPipe,
    RouterLink,
    DurationLabelPipe,
    FreezeAllowancePipe,
    HighlightPipe,
    RowMenuComponent,
    SortHeaderComponent,
    StatusBadgeComponent
  ],
  templateUrl: './plan-table.component.html',
  styleUrl: './plan-table.component.css'
})
export class PlanTableComponent {
  readonly plans = input<PlanListItem[]>([]);
  readonly searchTerm = input<string>('');
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection>('asc');

  readonly sortChange = output<SortChange>();

  readonly menuItems: RowMenuItem[] = [
    { id: 'view', label: 'View Plan' },
    { id: 'edit', label: 'Update Plan' }
  ];

  readonly accessLabel = accessLabel;

  constructor(private readonly router: Router) {}

  onAction(plan: PlanListItem, action: string): void {
    const commands =
      action === 'edit'
        ? ['/plans', plan.planId, 'edit']
        : ['/plans', plan.planId];

    this.router.navigate(commands);
  }
}
