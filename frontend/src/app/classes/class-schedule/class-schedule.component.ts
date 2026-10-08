import { Component, OnDestroy, OnInit, computed, effect, signal, untracked } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Subscription } from 'rxjs';

import { BranchService } from '../../core/services/branch.service';
import { MemberLookup } from '../../members/member.model';
import { MemberService } from '../../members/member.service';
import { HeaderSearchService } from '../../core/services/header-search.service';
import { RetryService } from '../../core/services/retry.service';
import { ToastService } from '../../core/services/toast.service';
import {
  addDays,
  formatDateMedium,
  todayIso
} from '../../core/utils/date.util';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { OptionGroupComponent, OptionItem } from '../../shared/components/option-group/option-group.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StateMessageComponent } from '../../shared/components/state-message/state-message.component';
import { BookSessionDialogComponent, BookSessionDialogData } from '../book-session-dialog/book-session-dialog.component';
import { CapacityOverviewComponent } from '../capacity-overview/capacity-overview.component';
import { ClassDialogComponent, ClassDialogData } from '../class-dialog/class-dialog.component';
import { ClassSchedule, ClassSessionItem, ScheduleView, SessionAction } from '../class.model';
import { ClassService } from '../class.service';
import { SessionRowComponent } from '../session-row/session-row.component';

interface DayGroup {
  date: string;
  label: string;
  sessions: ClassSessionItem[];
}

const ISO_DATE = /^\d{4}-\d{2}-\d{2}$/;

/**
 * Class Schedule page. Date, branch and view (Day/Week) live in the URL query
 * string and in signals; an effect reloads the schedule when they change.
 */
@Component({
  selector: 'app-class-schedule',
  standalone: true,
  imports: [
    IconComponent,
    OptionGroupComponent,
    PageHeaderComponent,
    StateMessageComponent,
    CapacityOverviewComponent,
    SessionRowComponent
  ],
  templateUrl: './class-schedule.component.html',
  styleUrl: './class-schedule.component.css'
})
export class ClassScheduleComponent implements OnInit, OnDestroy {
  private readonly queryParams = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap
  });

  readonly date = computed(() => {
    const value = this.queryParams().get('date') ?? '';

    return ISO_DATE.test(value) ? value : todayIso();
  });

  readonly view = computed<ScheduleView>(() =>
    this.queryParams().get('view') === 'week' ? 'week' : 'day'
  );

  /** Branch select value: 'all' or a branch id (the current header branch when the URL has none). */
  readonly branchValue = computed<string>(() => {
    const param = this.queryParams().get('branchId');

    if (param === 'all') {
      return 'all';
    }

    if (param && Number.isFinite(Number(param))) {
      return param;
    }

    const current = this.branchService.currentBranchId();

    return current === null ? '' : String(current);
  });

  readonly branchId = computed<number | null>(() => {
    const value = this.branchValue();

    return value === '' || value === 'all' ? null : Number(value);
  });

  readonly allBranches = computed(() => this.branchValue() === 'all');

  /** Member id from "Book Class" in the member directory (?bookFor=12), or null. */
  readonly bookFor = computed<number | null>(() => {
    const id = Number(this.queryParams().get('bookFor'));

    return Number.isInteger(id) && id > 0 ? id : null;
  });

  /** The member sessions are being booked for (loaded from the id above). */
  readonly bookingMember = signal<MemberLookup | null>(null);

  readonly branches = this.branchService.branches;

  readonly viewOptions: OptionItem[] = [
    { value: 'day', label: 'Day' },
    { value: 'week', label: 'Week' }
  ];

  readonly schedule = signal<ClassSchedule | null>(null);
  readonly loading = signal(true);
  readonly failed = signal(false);

  readonly searchTerm = computed(() => this.headerSearch.term());

  readonly isToday = computed(() => this.date() === todayIso());

  readonly rangeLabel = computed(() => {
    const start = formatDateMedium(this.date());

    return this.view() === 'week'
      ? `${start} – ${formatDateMedium(addDays(this.date(), 6))}`
      : start;
  });

  readonly subtitle = computed(() =>
    this.isToday() && this.view() === 'day'
      ? "Manage and monitor today's sessions."
      : `Sessions for ${this.rangeLabel()}`
  );

  readonly listTitle = computed(() =>
    this.isToday() && this.view() === 'day' ? "Today's Sessions" : this.rangeLabel()
  );

  /** Sessions narrowed by the header search (class, trainer or studio). */
  readonly filtered = computed<ClassSessionItem[]>(() => {
    const sessions = this.schedule()?.sessions ?? [];
    const term = this.searchTerm().toLowerCase();

    if (!term) {
      return sessions;
    }

    return sessions.filter((session) =>
      [session.className, session.trainerName, session.studioName]
        .filter((part): part is string => !!part)
        .some((part) => part.toLowerCase().includes(term))
    );
  });

  /** Day view: one group. Week view: one group per day, in date order. */
  readonly groups = computed<DayGroup[]>(() => {
    const sorted = [...this.filtered()].sort((a, b) =>
      a.sessionDate === b.sessionDate
        ? a.startTime.localeCompare(b.startTime)
        : a.sessionDate.localeCompare(b.sessionDate)
    );

    if (this.view() === 'day') {
      return sorted.length > 0
        ? [{ date: this.date(), label: '', sessions: sorted }]
        : [];
    }

    const byDate = new Map<string, ClassSessionItem[]>();

    for (const session of sorted) {
      byDate.set(session.sessionDate, [...(byDate.get(session.sessionDate) ?? []), session]);
    }

    return [...byDate.entries()].map(([date, sessions]) => ({
      date,
      label: formatDateMedium(date),
      sessions
    }));
  });

  readonly countLabel = computed(() => {
    const count = this.filtered().length;

    return `${count} ${count === 1 ? 'Class' : 'Classes'}`;
  });

  private loadSubscription: Subscription | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly dialog: MatDialog,
    private readonly classService: ClassService,
    private readonly branchService: BranchService,
    private readonly headerSearch: HeaderSearchService,
    private readonly toast: ToastService,
    private readonly memberService: MemberService,
    private readonly retryService: RetryService
  ) {
    // Load the member we are booking for.
    effect(() => {
      const memberId = this.bookFor();

      untracked(() => this.loadBookingMember(memberId));
    });

    // Reload when the date, branch or view changes.
    effect(() => {
      const date = this.date();
      const view = this.view();
      const branchId = this.branchId();

      this.retryService.tick();

      const waitingForBranch =
        this.branchValue() === '' && this.branchService.branches().length === 0;

      if (!waitingForBranch) {
        untracked(() => this.load(date, view, branchId));
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
    this.load(this.date(), this.view(), this.branchId());
  }

  clearBookFor(): void {
    this.update({ bookFor: null });
  }

  onBranch(event: Event): void {
    this.update({ branchId: (event.target as HTMLSelectElement).value });
  }

  onDate(event: Event): void {
    const value = (event.target as HTMLInputElement).value;

    if (ISO_DATE.test(value)) {
      this.update({ date: value });
    }
  }

  shift(direction: -1 | 1): void {
    const step = this.view() === 'week' ? 7 : 1;

    this.update({ date: addDays(this.date(), direction * step) });
  }

  goToToday(): void {
    this.update({ date: null });
  }

  onView(value: string | number | null): void {
    this.update({ view: value === 'week' ? 'week' : null });
  }

  openAdd(): void {
    this.openClassDialog({ mode: 'add' });
  }

  onAction(event: SessionAction): void {
    switch (event.action) {
      case 'view':
        this.openClassDialog({ mode: 'view', sessionId: event.session.sessionId });
        break;

      case 'edit':
        this.openClassDialog({ mode: 'edit', sessionId: event.session.sessionId });
        break;

      case 'book':
        this.openBook(event.session);
        break;

      case 'cancel':
        this.confirmCancel(event.session);
        break;

      default:
        break;
    }
  }

  private openClassDialog(data: ClassDialogData): void {
    this.dialog
      .open<ClassDialogComponent, ClassDialogData, boolean>(ClassDialogComponent, {
        data,
        panelClass: 'titan-dialog',
        width: '640px',
        autoFocus: false
      })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) {
          this.reload();
        }
      });
  }

  private openBook(session: ClassSessionItem): void {
    this.dialog
      .open<BookSessionDialogComponent, BookSessionDialogData, boolean>(BookSessionDialogComponent, {
        data: { sessionId: session.sessionId, member: this.bookingMember() },
        panelClass: 'titan-dialog',
        width: '520px',
        autoFocus: false
      })
      .afterClosed()
      .subscribe((booked) => {
        if (booked) {
          this.reload();
        }
      });
  }

  private confirmCancel(session: ClassSessionItem): void {
    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
        data: {
          title: 'Cancel this class?',
          message: `"${session.className}" will be cancelled. Members who booked it will lose their place.`,
          confirmLabel: 'Cancel Class',
          cancelLabel: 'Keep Class',
          danger: true
        },
        panelClass: 'titan-dialog',
        width: '420px'
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.classService.cancel(session.sessionId).subscribe({
          next: () => {
            this.toast.success('Class cancelled');
            this.reload();
          },
          error: () => {
            // The error interceptor already shows the message.
          }
        });
      });
  }

  private loadBookingMember(memberId: number | null): void {
    if (memberId === null) {
      this.bookingMember.set(null);
      return;
    }

    this.memberService.getProfile(memberId).subscribe({
      next: (profile) =>
        this.bookingMember.set({
          memberId: profile.memberId,
          fullName: profile.fullName,
          membershipNumber: profile.membershipNumber,
          photo: profile.photo,
          status: profile.status
        }),
      error: () => this.bookingMember.set(null)
    });
  }

  private load(date: string, view: ScheduleView, branchId: number | null): void {
    this.loadSubscription?.unsubscribe();
    this.loading.set(true);
    this.failed.set(false);

    const toDate = view === 'week' ? addDays(date, 6) : null;

    this.loadSubscription = this.classService.getSchedule(date, toDate, branchId).subscribe({
      next: (schedule) => {
        this.schedule.set(schedule);
        this.loading.set(false);
      },
      error: () => {
        this.schedule.set(null);
        this.failed.set(true);
        this.loading.set(false);
      }
    });
  }

  private update(queryParams: Params): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'merge'
    });
  }
}
