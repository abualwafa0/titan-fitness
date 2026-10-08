import { Component, Inject, OnInit, computed, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { ToastService } from '../../core/services/toast.service';
import { addMinutesToTime, relativeDayLabel } from '../../core/utils/date.util';
import { getApiMessage } from '../../core/utils/form-errors.util';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { MemberAutocompleteComponent } from '../../shared/components/member-autocomplete/member-autocomplete.component';
import { StateMessageComponent } from '../../shared/components/state-message/state-message.component';
import { AutofocusDirective } from '../../shared/directives/autofocus.directive';
import { MemberLookup } from '../../members/member.model';
import { BookingContext } from '../class.model';
import { ClassService } from '../class.service';

export interface BookSessionDialogData {
  sessionId: number;
  /** Member to pre-select (when booking from "Book Class" in the member directory). */
  member?: MemberLookup | null;
}

const MAX_NOTES = 500;

/** Book Session dialog: pick a member, add notes, confirm. Closes with `true` after a booking. */
@Component({
  selector: 'app-book-session-dialog',
  standalone: true,
  imports: [
    AutofocusDirective,
    IconComponent,
    MemberAutocompleteComponent,
    StateMessageComponent
  ],
  templateUrl: './book-session-dialog.component.html',
  styleUrl: './book-session-dialog.component.css'
})
export class BookSessionDialogComponent implements OnInit {
  readonly context = signal<BookingContext | null>(null);
  readonly loading = signal(true);
  readonly failed = signal(false);

  readonly member = signal<MemberLookup | null>(null);
  readonly notes = signal('');
  readonly submitted = signal(false);
  readonly saving = signal(false);
  readonly message = signal<string | null>(null);

  readonly maxNotes = MAX_NOTES;

  readonly whenLabel = computed(() => {
    const context = this.context();

    if (!context) {
      return '';
    }

    const start = context.startTime.slice(0, 5);
    const end = addMinutesToTime(context.startTime, context.durationInMinutes).slice(0, 5);

    return `${relativeDayLabel(context.sessionDate)}, ${start} - ${end}`;
  });

  readonly memberError = computed(() =>
    this.submitted() && !this.member() ? 'Select a member.' : null
  );

  constructor(
    private readonly dialogRef: MatDialogRef<BookSessionDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) private readonly data: BookSessionDialogData,
    private readonly classService: ClassService,
    private readonly toast: ToastService
  ) {
    this.member.set(data.member ?? null);
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.failed.set(false);

    this.classService.getBookingContext(this.data.sessionId).subscribe({
      next: (context) => {
        this.context.set(context);
        this.loading.set(false);
      },
      error: () => {
        this.failed.set(true);
        this.loading.set(false);
      }
    });
  }

  onNotes(event: Event): void {
    this.notes.set((event.target as HTMLTextAreaElement).value);
  }

  confirm(): void {
    if (this.saving()) {
      return;
    }

    this.submitted.set(true);
    this.message.set(null);

    const member = this.member();

    if (!member) {
      return;
    }

    this.saving.set(true);

    this.classService
      .book(this.data.sessionId, member.memberId, this.notes().trim() || undefined)
      .subscribe({
        next: (booking) => {
          this.saving.set(false);

          if (booking.status === 'Waitlisted') {
            this.toast.info(
              booking.waitlistPosition
                ? `Class is full. ${member.fullName} is number ${booking.waitlistPosition} on the waitlist.`
                : `Class is full. ${member.fullName} was added to the waitlist.`
            );
          } else {
            this.toast.success('Booked');
          }

          this.dialogRef.close(true);
        },
        error: (error: unknown) => {
          this.saving.set(false);
          this.message.set(getApiMessage(error) ?? 'The booking could not be completed.');
        }
      });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
