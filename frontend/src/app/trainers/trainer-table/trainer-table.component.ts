import { Component, input, output } from '@angular/core';
import { Router, RouterLink } from '@angular/router';

import { HighlightPipe } from '../../shared/pipes/highlight.pipe';
import { AvatarComponent } from '../../shared/components/avatar/avatar.component';
import { RowMenuComponent, RowMenuItem } from '../../shared/components/row-menu/row-menu.component';
import { SortChange, SortDirection, SortHeaderComponent } from '../../shared/components/sort-header/sort-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { TrainerListItem, formatTrainerNumber } from '../trainer.model';

/** Table of the trainer directory (child component: data comes in, sort events go out). */
@Component({
  selector: 'app-trainer-table',
  standalone: true,
  imports: [
    RouterLink,
    HighlightPipe,
    AvatarComponent,
    RowMenuComponent,
    SortHeaderComponent,
    StatusBadgeComponent
  ],
  templateUrl: './trainer-table.component.html',
  styleUrl: './trainer-table.component.css'
})
export class TrainerTableComponent {
  readonly trainers = input<TrainerListItem[]>([]);
  readonly searchTerm = input<string>('');
  readonly sortBy = input<string | null>(null);
  readonly sortDirection = input<SortDirection>('asc');

  readonly sortChange = output<SortChange>();

  readonly menuItems: RowMenuItem[] = [
    { id: 'view', label: 'View Trainer' },
    { id: 'edit', label: 'Update Trainer' }
  ];

  constructor(private readonly router: Router) {}

  idLabel(trainer: TrainerListItem): string {
    return formatTrainerNumber(trainer.trainerNumber);
  }

  onAction(trainer: TrainerListItem, action: string): void {
    const commands =
      action === 'edit'
        ? ['/trainers', trainer.trainerId, 'edit']
        : ['/trainers', trainer.trainerId];

    this.router.navigate(commands);
  }
}
