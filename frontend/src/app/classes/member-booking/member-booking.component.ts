import { Component, computed, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { environment } from '../../../environments/environment';
import { ToastService } from '../../core/services/toast.service';
import { addMinutesToTime, relativeDayLabel } from '../../core/utils/date.util';
import { getApiMessage, getHttpStatus } from '../../core/utils/form-errors.util';
import { MemberProfile } from '../../members/member.model';
import { MemberService } from '../../members/member.service';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { StateMessageComponent } from '../../shared/components/state-message/state-message.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { BookingContext } from '../class.model';
import { ClassService } from '../class.service';

type LoadState = 'loading' | 'ready' | 'notFound' | 'error';

const MAX_NOTES = 500;

/**
 * Member-facing booking page (/member/classes/:sessionId/book, no sidebar).
 * The member is the demo member from the environment file.
 */
@Component({
  selector: 'app-member-booking',
  standalone: true,
  imports: [RouterLink, IconComponent, StateMessageComponent, StatusBadgeComponent],
  templateUrl: './member-booking.component.html',
  styleUrl: './member-booking.component.css'
})
export class MemberBookingComponent {
  readonly loadState = signal<LoadState>('loading');
  readonly session = signal<BookingContext | null>(null);
  readonly member = signal<MemberProfile | null>(null);

  readonly notes = signal('');
  readonly saving = signal(false);
  readonly message = signal<string | null>(null);
  readonly maxNotes = MAX_NOTES;

  readonly whenLabel = computed(() => {
    const session = this.session();

    if (!session) {
      return '';
    }

    const start = session.startTime.slice(0, 5);
    const end = addMinutesToTime(session.startTime, session.durationInMinutes).slice(0, 5);

    return `${relativeDayLabel(session.sessionDate)}, ${start} - ${end}`;
  });

  readonly percent = computed(() => {
    const session = this.session();

    return session && session.capacityLimit > 0
      ? Math.min(100, Math.round((session.bookedPlaces / session.capacityLimit) * 100))
      : 0;
  });

  readonly eligible = computed(() => this.member()?.status === 'Active');

  readonly memberStatusText = computed(() => this.member()?.status ?? 'No membership');

  readonly canConfirm = computed(() => this.eligible() && !this.saving());

  private readonly sessionId: number | null;

  constructor(
    route: ActivatedRoute,
    private readonly router: Router,
    private readonly classService: ClassService,
    private readonly memberService: MemberService,
    private readonly toast: ToastService
  ) {
    const id = Number(route.snapshot.paramMap.get('sessionId'));

    this.sessionId = Number.isInteger(id) && id > 0 ? id : null;

    if (this.sessionId === null) {
      this.loadState.set('notFound');
    } else {
      this.load();
    }
  }

  load(): void {
    if (this.sessionId === null) {
      return;
    }

    this.loadState.set('loading');

    this.classService.getBookingContext(this.sessionId).subscribe({
      next: (session) => {
        this.session.set(session);
        this.loadMember();
      },
      error: (error: unknown) => {
        this.loadState.set(getHttpStatus(error) === 404 ? 'notFound' : 'error');
      }
    });
  }

  onNotes(event: Event): void {
    this.notes.set((event.target as HTMLTextAreaElement).value);
  }

  confirm(): void {
    if (this.sessionId === null || !this.canConfirm()) {
      return;
    }

    this.saving.set(true);
    this.message.set(null);

    this.classService
      .book(this.sessionId, environment.demoMemberId, this.notes().trim() || undefined)
      .subscribe({
        next: (booking) => {
          this.saving.set(false);

          if (booking.status === 'Waitlisted') {
            this.toast.info(
              booking.waitlistPosition
                ? `The class is full. You are number ${booking.waitlistPosition} on the waitlist.`
                : 'The class is full. You were added to the waitlist.'
            );
          } else {
            this.toast.success('Booking confirmed');
          }

          this.router.navigate(['/classes']);
        },
        error: (error: unknown) => {
          this.saving.set(false);
          this.message.set(getApiMessage(error) ?? 'The booking could not be completed.');
        }
      });
  }

  cancel(): void {
    this.router.navigate(['/classes']);
  }

  private loadMember(): void {
    this.memberService.getProfile(environment.demoMemberId).subscribe({
      next: (profile) => {
        this.member.set(profile);
        this.loadState.set('ready');
      },
      error: () => {
        // Without the profile the member cannot be cleared for booking.
        this.member.set(null);
        this.loadState.set('ready');
      }
    });
  }
}
