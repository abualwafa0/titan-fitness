import { Component, OnDestroy, OnInit, computed, effect, signal, untracked } from '@angular/core';
import { Subscription } from 'rxjs';

import { CheckInService } from '../check-ins/check-in.service';
import { BranchService } from '../core/services/branch.service';
import { HeaderSearchService } from '../core/services/header-search.service';
import { RetryService } from '../core/services/retry.service';
import { PageHeaderComponent } from '../shared/components/page-header/page-header.component';
import { StateMessageComponent } from '../shared/components/state-message/state-message.component';
import { DashboardData, DashboardSession } from './dashboard.model';
import { DashboardService } from './dashboard.service';
import { KpiCardComponent, KpiTone } from './kpi-card/kpi-card.component';
import { QuickActionsComponent } from './quick-actions/quick-actions.component';
import { UpcomingClassesComponent } from './upcoming-classes/upcoming-classes.component';

/** Dashboard: live floor statistics, upcoming classes and quick actions. */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    PageHeaderComponent,
    StateMessageComponent,
    KpiCardComponent,
    UpcomingClassesComponent,
    QuickActionsComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit, OnDestroy {
  readonly data = signal<DashboardData | null>(null);
  readonly loading = signal(true);
  readonly failed = signal(false);

  readonly searchTerm = computed(() => this.headerSearch.term());

  /** The running session (if any) followed by the upcoming ones, filtered by the header search. */
  readonly sessions = computed<DashboardSession[]>(() => {
    const data = this.data();

    if (!data) {
      return [];
    }

    const all = data.currentRunningSession
      ? [data.currentRunningSession, ...data.upcomingSessions.filter(
          (session) => session.sessionId !== data.currentRunningSession?.sessionId
        )]
      : data.upcomingSessions;

    const term = this.searchTerm().toLowerCase();

    if (!term) {
      return all;
    }

    return all.filter((session) =>
      [session.className, session.trainerName, session.studioName]
        .filter((part): part is string => !!part)
        .some((part) => part.toLowerCase().includes(term))
    );
  });

  readonly checkInNote = computed(() => {
    const data = this.data();

    if (!data || data.checkInsSameWeekdayLastWeek <= 0) {
      return 'No data last week';
    }

    const percent = Math.round(
      ((data.checkInsToday - data.checkInsSameWeekdayLastWeek) /
        data.checkInsSameWeekdayLastWeek) * 100
    );

    return `${percent >= 0 ? '+' : ''}${percent}% vs last week`;
  });

  readonly checkInTone = computed<KpiTone>(() => {
    const data = this.data();

    if (!data || data.checkInsSameWeekdayLastWeek <= 0) {
      return 'neutral';
    }

    return data.checkInsToday >= data.checkInsSameWeekdayLastWeek ? 'positive' : 'negative';
  });

  private loadSubscription: Subscription | null = null;

  constructor(
    private readonly dashboardService: DashboardService,
    private readonly branchService: BranchService,
    private readonly checkInService: CheckInService,
    private readonly headerSearch: HeaderSearchService,
    private readonly retryService: RetryService
  ) {
    // Reload when the current branch or the check-in version changes.
    effect(() => {
      const branchId = this.branchService.currentBranchId();

      this.checkInService.checkInVersion();
      this.retryService.tick();

      if (branchId !== null) {
        untracked(() => this.load(branchId));
      }
    });
  }

  ngOnInit(): void {
    this.headerSearch.reset();
    this.headerSearch.setPlaceholder('Search classes...');
  }

  ngOnDestroy(): void {
    this.loadSubscription?.unsubscribe();
    this.headerSearch.reset();
  }

  reload(): void {
    const branchId = this.branchService.currentBranchId();

    if (branchId !== null) {
      this.load(branchId);
    }
  }

  private load(branchId: number): void {
    this.loadSubscription?.unsubscribe();
    this.failed.set(false);

    // Keep the old numbers on screen while a refresh is running.
    if (this.data() === null) {
      this.loading.set(true);
    }

    this.loadSubscription = this.dashboardService.get(branchId).subscribe({
      next: (data) => {
        this.data.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.failed.set(true);
        this.loading.set(false);
      }
    });
  }
}
