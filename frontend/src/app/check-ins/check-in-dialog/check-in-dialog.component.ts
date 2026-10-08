import { Component, Inject, computed, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { BranchService } from '../../core/services/branch.service';
import { ToastService } from '../../core/services/toast.service';
import {
  addDays,
  combineLocal,
  formatTime12,
  isFutureDateTime,
  parseLocalDateTime,
  roundDownTo5,
  toTimeInput,
  todayIso
} from '../../core/utils/date.util';
import {
  applyServerErrors,
  clearServerErrors,
  getApiMessage
} from '../../core/utils/form-errors.util';
import { FieldErrorComponent } from '../../shared/components/field-error/field-error.component';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { MemberAutocompleteComponent } from '../../shared/components/member-autocomplete/member-autocomplete.component';
import { AutofocusDirective } from '../../shared/directives/autofocus.directive';
import { MemberLookup } from '../../members/member.model';
import { CheckInService } from '../check-in.service';

export interface CheckInDialogData {
  member: MemberLookup | null;
}

const MAX_NOTES = 250;
const MAX_DAYS_BACK = 7;

/** Manual Check-in dialog. A refused check-in keeps the dialog open and shows the reason. */
@Component({
  selector: 'app-check-in-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AutofocusDirective,
    FieldErrorComponent,
    IconComponent,
    MemberAutocompleteComponent
  ],
  templateUrl: './check-in-dialog.component.html',
  styleUrl: './check-in-dialog.component.css'
})
export class CheckInDialogComponent {
  readonly member = signal<MemberLookup | null>(null);
  readonly memberLocked: boolean;
  readonly memberTouched = signal(false);

  readonly saving = signal(false);
  readonly refusal = signal<string | null>(null);
  readonly banner = signal<string[]>([]);
  readonly notesLength = signal(0);

  readonly today = todayIso();
  readonly oldestDate = addDays(todayIso(), -MAX_DAYS_BACK);
  readonly maxNotes = MAX_NOTES;

  readonly branchName = computed(() => this.branchService.currentBranch()?.branchName ?? '');

  readonly memberError = computed(() =>
    this.memberTouched() && !this.member() ? 'Member is required.' : null
  );

  readonly form = new FormGroup({
    checkInDate: new FormControl(todayIso(), {
      nonNullable: true,
      validators: [Validators.required, CheckInDialogComponent.dateRule]
    }),
    checkInTime: new FormControl(toTimeInput(roundDownTo5(new Date())), {
      nonNullable: true,
      validators: [Validators.required, CheckInDialogComponent.timeRule]
    }),
    notes: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(MAX_NOTES)]
    })
  });

  constructor(
    private readonly dialogRef: MatDialogRef<CheckInDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) data: CheckInDialogData,
    private readonly checkInService: CheckInService,
    private readonly branchService: BranchService,
    private readonly toast: ToastService
  ) {
    this.member.set(data.member);
    this.memberLocked = data.member !== null;

    // The time rule depends on the date, so re-check it whenever the date changes.
    this.form.controls.checkInDate.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.checkInTime.updateValueAndValidity());

    this.form.controls.notes.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.notesLength.set(value.length));
  }

  /** Date: not in the future and not older than 7 days. */
  private static dateRule(control: AbstractControl): ValidationErrors | null {
    const value = String(control.value ?? '');

    if (!value) {
      return null;
    }

    if (value > todayIso()) {
      return { future: 'Date cannot be in the future.' };
    }

    if (value < addDays(todayIso(), -MAX_DAYS_BACK)) {
      return { old: `Date cannot be older than ${MAX_DAYS_BACK} days.` };
    }

    return null;
  }

  /** Time: date + time must not be in the future. */
  private static timeRule(control: AbstractControl): ValidationErrors | null {
    const time = String(control.value ?? '');
    const date = String(control.parent?.get('checkInDate')?.value ?? '');

    if (!time || !date) {
      return null;
    }

    return isFutureDateTime(date, time)
      ? { future: 'Check-in time cannot be in the future.' }
      : null;
  }

  save(): void {
    if (this.saving()) {
      return;
    }

    clearServerErrors(this.form);
    this.banner.set([]);
    this.refusal.set(null);
    this.memberTouched.set(true);

    const member = this.member();
    const branchId = this.branchService.currentBranchId();

    if (this.form.invalid || !member) {
      this.form.markAllAsTouched();
      return;
    }

    if (branchId === null) {
      this.banner.set(['Select a branch first.']);
      return;
    }

    const value = this.form.getRawValue();
    const notes = value.notes.trim();

    this.saving.set(true);

    this.checkInService
      .checkIn({
        memberId: member.memberId,
        branchId,
        checkInDateTime: combineLocal(value.checkInDate, value.checkInTime),
        ...(notes ? { notes } : {})
      })
      .subscribe({
        next: (response) => {
          this.saving.set(false);

          if (response.result === 'Refused') {
            this.refusal.set(response.refusalReason ?? 'The check-in was refused.');
            return;
          }

          const time = formatTime12(parseLocalDateTime(response.checkInDateTime));

          this.toast.success(`${member.fullName} checked in at ${time}`);
          this.checkInService.notifyCheckedIn();
          this.dialogRef.close(true);
        },
        error: (error: unknown) => {
          this.saving.set(false);

          const messages = applyServerErrors(this.form, error);
          const message = getApiMessage(error);

          if (message && messages.length === 0) {
            messages.push(message);
          }

          this.banner.set(messages);
        }
      });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
