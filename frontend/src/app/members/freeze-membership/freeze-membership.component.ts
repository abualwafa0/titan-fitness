import { DatePipe } from '@angular/common';
import { Component, computed, effect, signal, untracked } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { ToastService } from '../../core/services/toast.service';
import { addDays, addMonths, diffDays, todayIso } from '../../core/utils/date.util';
import {
  applyServerErrors,
  clearServerErrors,
  getApiMessage,
  getHttpStatus
} from '../../core/utils/form-errors.util';
import { AvatarComponent } from '../../shared/components/avatar/avatar.component';
import { FieldErrorComponent } from '../../shared/components/field-error/field-error.component';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { OptionGroupComponent, OptionItem } from '../../shared/components/option-group/option-group.component';
import { StateMessageComponent } from '../../shared/components/state-message/state-message.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { ProjectedImpactComponent } from '../projected-impact/projected-impact.component';
import {
  FreezeContext,
  FreezeReason,
  FreezeRequest,
  MemberProfile,
  formatMemberNumber
} from '../member.model';
import { MemberService } from '../member.service';

type LoadState = 'loading' | 'ready' | 'notFound' | 'noMembership' | 'error';

const ISO_DATE = /^\d{4}-\d{2}-\d{2}$/;

const REASONS: { value: FreezeReason; label: string }[] = [
  { value: 'ExtendedTravel', label: 'Extended Travel' },
  { value: 'Medical', label: 'Medical' },
  { value: 'Injury', label: 'Injury' },
  { value: 'Financial', label: 'Financial' },
  { value: 'Other', label: 'Other' }
];

function isIsoDate(value: string): boolean {
  return ISO_DATE.test(value) && !Number.isNaN(new Date(`${value}T00:00:00`).getTime());
}

/** Freeze Membership screen (/members/:id/freeze). */
@Component({
  selector: 'app-freeze-membership',
  standalone: true,
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    AvatarComponent,
    FieldErrorComponent,
    IconComponent,
    OptionGroupComponent,
    StateMessageComponent,
    StatusBadgeComponent,
    ProjectedImpactComponent
  ],
  templateUrl: './freeze-membership.component.html',
  styleUrl: './freeze-membership.component.css'
})
export class FreezeMembershipComponent {
  readonly loadState = signal<LoadState>('loading');
  readonly profile = signal<MemberProfile | null>(null);
  readonly context = signal<FreezeContext | null>(null);
  readonly saving = signal(false);
  readonly banner = signal<string[]>([]);

  readonly reasons = REASONS;
  readonly today = todayIso();

  readonly duration = signal<number | null>(1);
  private readonly startDateValue = signal(addDays(todayIso(), 1));
  readonly notesLength = signal(0);
  private readonly formValid = signal(false);

  /** Start date: not in the past and before the membership end date. */
  private readonly startValidator: ValidatorFn = (
    control: AbstractControl
  ): ValidationErrors | null => {
    const value = String(control.value ?? '');

    if (!isIsoDate(value)) {
      return { required: true };
    }

    if (value < todayIso()) {
      return { past: 'Start date cannot be in the past.' };
    }

    const endDate = this.context()?.endDate;

    if (endDate && value >= endDate) {
      return { afterEnd: 'Start date must be before the membership end date.' };
    }

    return null;
  };

  readonly form = new FormGroup({
    startDate: new FormControl(addDays(todayIso(), 1), {
      nonNullable: true,
      validators: [this.startValidator]
    }),
    durationInMonths: new FormControl<number | null>(1, [Validators.required]),
    reason: new FormControl<FreezeReason | ''>('', {
      nonNullable: true,
      validators: [Validators.required]
    }),
    additionalNotes: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(500)]
    })
  });

  readonly startIsValid = computed(() => {
    const start = this.startDateValue();
    const endDate = this.context()?.endDate;

    return isIsoDate(start) && start >= this.today && !!endDate && start < endDate;
  });

  readonly durationOptions = computed<OptionItem[]>(() => {
    const context = this.context();
    const start = this.startDateValue();

    return [1, 2, 3].map((months) => {
      let disabled = false;
      let tooltip: string | undefined;

      if (context) {
        if (
          context.allowedDurationsInMonths.length > 0 &&
          !context.allowedDurationsInMonths.includes(months)
        ) {
          disabled = true;
          tooltip = 'Not available for this membership';
        } else if (
          isIsoDate(start) &&
          diffDays(start, addMonths(start, months)) > context.remainingFreezeDays
        ) {
          disabled = true;
          tooltip = `Only ${context.remainingFreezeDays} freeze days remaining`;
        }
      }

      return {
        value: months,
        label: months === 1 ? '1 Month' : `${months} Months`,
        disabled,
        tooltip
      };
    });
  });

  /** New end date = original end date + the days between the start and start + N months. */
  readonly newEndDate = computed<string | null>(() => {
    const context = this.context();
    const months = this.duration();

    if (!context || !months || !this.startIsValid()) {
      return null;
    }

    const start = this.startDateValue();

    return addDays(context.endDate, diffDays(start, addMonths(start, months)));
  });

  /** Why the form is read-only (null = the freeze can be requested). */
  readonly readOnlyReason = computed<string | null>(() => {
    const context = this.context();

    if (!context) {
      return null;
    }

    if (context.status === 'Frozen') {
      return 'This membership is already frozen.';
    }

    if (context.status === 'Expired') {
      return 'This membership has expired.';
    }

    if (context.status !== 'Active') {
      return `This membership is ${context.status}, so it cannot be frozen.`;
    }

    if (context.remainingNumberOfFreezes <= 0) {
      return 'No freezes remaining on this plan.';
    }

    return null;
  });

  readonly canConfirm = computed(
    () => this.formValid() && this.readOnlyReason() === null && this.duration() !== null
  );

  readonly idLine = computed(() => {
    const context = this.context();

    return context ? `ID: ${formatMemberNumber(context.membershipNumber).replace(/^#/, '')} • ${context.planName}` : '';
  });

  private readonly memberId: number | null;

  constructor(
    route: ActivatedRoute,
    private readonly router: Router,
    private readonly memberService: MemberService,
    private readonly toast: ToastService
  ) {
    const id = Number(route.snapshot.paramMap.get('id'));

    this.memberId = Number.isInteger(id) && id > 0 ? id : null;

    this.form.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => this.syncState());

    // When the start date changes, a duration that is no longer allowed is replaced.
    effect(() => {
      const options = this.durationOptions();
      const selected = untracked(() => this.duration());
      const current = options.find((option) => option.value === selected);

      if (selected !== null && (!current || current.disabled)) {
        const firstFree = options.find((option) => !option.disabled);

        untracked(() => this.setDuration(firstFree ? Number(firstFree.value) : null));
      }
    });

    if (this.memberId === null) {
      this.loadState.set('notFound');
    } else {
      this.load();
    }
  }

  load(): void {
    if (this.memberId === null) {
      return;
    }

    this.loadState.set('loading');

    this.memberService.getProfile(this.memberId).subscribe({
      next: (profile) => {
        this.profile.set(profile);

        if (!profile.currentMembership) {
          this.loadState.set('noMembership');
          return;
        }

        this.loadContext(profile.currentMembership.membershipId);
      },
      error: (error: unknown) => {
        this.loadState.set(getHttpStatus(error) === 404 ? 'notFound' : 'error');
      }
    });
  }

  onDuration(value: string | number | null): void {
    this.setDuration(value === null ? null : Number(value));
    this.form.controls.durationInMonths.markAsDirty();
  }

  confirm(): void {
    const context = this.context();

    if (!context || !this.canConfirm() || this.saving()) {
      return;
    }

    clearServerErrors(this.form);
    this.banner.set([]);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const notes = value.additionalNotes.trim();

    const request: FreezeRequest = {
      startDate: value.startDate,
      durationInMonths: value.durationInMonths as number,
      reason: value.reason as FreezeReason,
      ...(notes ? { additionalNotes: notes } : {})
    };

    this.saving.set(true);

    this.memberService.freeze(context.membershipId, request).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Membership frozen');
        this.router.navigate(['/members', context.memberId]);
      },
      error: (error: unknown) => {
        this.saving.set(false);

        const messages = applyServerErrors(this.form, error);
        const message = getApiMessage(error);

        if (message && (getHttpStatus(error) === 409 || messages.length === 0)) {
          messages.push(message);
        }

        this.banner.set(messages);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/members', this.memberId]);
  }

  private loadContext(membershipId: number): void {
    this.memberService.getFreezeContext(membershipId).subscribe({
      next: (context) => {
        this.context.set(context);

        if (this.readOnlyReason() !== null) {
          this.form.disable({ emitEvent: false });
        }

        this.form.controls.startDate.updateValueAndValidity({ emitEvent: false });
        this.syncState();
        this.loadState.set('ready');
      },
      error: (error: unknown) => {
        this.loadState.set(getHttpStatus(error) === 404 ? 'notFound' : 'error');
      }
    });
  }

  private setDuration(value: number | null): void {
    this.duration.set(value);
    this.form.controls.durationInMonths.setValue(value, { emitEvent: false });
    this.syncState();
  }

  private syncState(): void {
    const raw = this.form.getRawValue();

    this.startDateValue.set(raw.startDate);
    this.notesLength.set(raw.additionalNotes.length);
    this.formValid.set(this.form.valid);
  }
}
