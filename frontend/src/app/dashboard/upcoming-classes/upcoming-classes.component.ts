import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { StateMessageComponent } from '../../shared/components/state-message/state-message.component';
import { HighlightPipe } from '../../shared/pipes/highlight.pipe';
import { formatTime12 } from '../../core/utils/date.util';
import { ClassState, getClassState } from '../../shared/utils/class-state.util';
import { DashboardSession } from '../dashboard.model';

/** "Upcoming Classes" card of the dashboard (already filtered by the parent). */
@Component({
  selector: 'app-upcoming-classes',
  standalone: true,
  imports: [RouterLink, HighlightPipe, StatusBadgeComponent, StateMessageComponent],
  templateUrl: './upcoming-classes.component.html',
  styleUrl: './upcoming-classes.component.css'
})
export class UpcomingClassesComponent {
  readonly sessions = input<DashboardSession[]>([]);
  readonly searchTerm = input<string>('');

  time(session: DashboardSession): string {
    return formatTime12(session.startTime);
  }

  state(session: DashboardSession): ClassState {
    return getClassState(session.status, session.bookedPlaces, session.capacityLimit);
  }

  /** A class in progress is still "Active" (green) on the dashboard. */
  badgeStatus(session: DashboardSession): string {
    const state = this.state(session);

    return state === 'InProgress' ? 'Active' : state;
  }

  /** "Studio A • Sarah J." (missing parts are left out). */
  subtitle(session: DashboardSession): string {
    return [session.studioName, this.shortName(session.trainerName)]
      .filter((part): part is string => !!part)
      .join(' • ');
  }

  private shortName(name: string | null): string | null {
    const parts = (name ?? '').trim().split(/\s+/).filter(Boolean);

    if (parts.length === 0) {
      return null;
    }

    return parts.length === 1 ? parts[0] : `${parts[0]} ${parts[parts.length - 1][0]}.`;
  }
}
