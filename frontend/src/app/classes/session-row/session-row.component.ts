import { Component, computed, input, output } from '@angular/core';

import { addMinutesToTime, formatTime12 } from '../../core/utils/date.util';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { RowMenuComponent, RowMenuItem } from '../../shared/components/row-menu/row-menu.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { HighlightPipe } from '../../shared/pipes/highlight.pipe';
import { fillPercent, getClassState, isAlmostFull } from '../../shared/utils/class-state.util';
import { ClassSessionItem, SessionAction, SessionActionId } from '../class.model';

/** One session of the class schedule (child: data comes in, the chosen menu action goes out). */
@Component({
  selector: 'app-session-row',
  standalone: true,
  imports: [HighlightPipe, IconComponent, RowMenuComponent, StatusBadgeComponent],
  templateUrl: './session-row.component.html',
  styleUrl: './session-row.component.css'
})
export class SessionRowComponent {
  readonly session = input.required<ClassSessionItem>();
  readonly searchTerm = input<string>('');
  /** True when "All Branches" is selected (the branch name is shown). */
  readonly showBranch = input<boolean>(false);
  /** True in Week view (the end time is always visible). */
  readonly showEndTime = input<boolean>(false);

  readonly actionSelected = output<SessionAction>();

  readonly startLabel = computed(() => formatTime12(this.session().startTime));

  readonly endLabel = computed(() =>
    formatTime12(addMinutesToTime(this.session().startTime, this.session().durationInMinutes))
  );

  readonly state = computed(() =>
    getClassState(this.session().status, this.session().bookedPlaces, this.session().capacityLimit)
  );

  readonly percent = computed(() =>
    fillPercent(this.session().bookedPlaces, this.session().capacityLimit)
  );

  readonly almostFull = computed(() =>
    isAlmostFull(this.session().bookedPlaces, this.session().capacityLimit)
  );

  readonly menuItems = computed<RowMenuItem[]>(() => {
    const state = this.state();
    const closed = state === 'Completed' || state === 'Cancelled';

    return [
      { id: 'view', label: 'View Class' },
      { id: 'edit', label: 'Edit Class' },
      { id: 'book', label: 'Book Session' },
      {
        id: 'cancel',
        label: 'Cancel Class',
        disabled: closed,
        disabledReason: state === 'Cancelled' ? 'Class is already cancelled' : 'Class is already completed'
      }
    ];
  });

  onAction(id: string): void {
    this.actionSelected.emit({ action: id as SessionActionId, session: this.session() });
  }
}
