import { Component, Inject, Signal, computed, signal } from '@angular/core';
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
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { Branch } from '../../core/models/branch.model';
import { Studio } from '../../core/models/branch.model';
import { BranchService } from '../../core/services/branch.service';
import { ToastService } from '../../core/services/toast.service';
import { isFutureDateTime, normalizeTime, todayIso } from '../../core/utils/date.util';
import {
  applyServerErrors,
  clearServerErrors,
  getApiMessage,
  getHttpStatus
} from '../../core/utils/form-errors.util';
import { FieldErrorComponent } from '../../shared/components/field-error/field-error.component';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { OptionGroupComponent, OptionItem } from '../../shared/components/option-group/option-group.component';
import { StateMessageComponent } from '../../shared/components/state-message/state-message.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { AutofocusDirective } from '../../shared/directives/autofocus.directive';
import { getClassState } from '../../shared/utils/class-state.util';
import { TrainerLookup } from '../../trainers/trainer.model';
import { TrainerService } from '../../trainers/trainer.service';
import { BookingContext, ClassSessionRequest } from '../class.model';
import { ClassService } from '../class.service';

export interface ClassDialogData {
  mode: 'add' | 'view' | 'edit';
  sessionId?: number;
}

type DialogMode = 'add' | 'view' | 'edit';
type LoadState = 'loading' | 'ready' | 'error';

const DEFAULT_CAPACITY = 20;
const MAX_DESCRIPTION = 500;

/**
 * ONE dialog for adding, viewing and editing a class session
 * (data `{ mode, sessionId? }`). Closes with `true` after a successful save.
 */
@Component({
  selector: 'app-class-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AutofocusDirective,
    FieldErrorComponent,
    IconComponent,
    OptionGroupComponent,
    StateMessageComponent,
    StatusBadgeComponent
  ],
  templateUrl: './class-dialog.component.html',
  styleUrl: './class-dialog.component.css'
})
export class ClassDialogComponent {
  readonly mode = signal<DialogMode>('add');
  readonly loadState = signal<LoadState>('ready');
  readonly context = signal<BookingContext | null>(null);
  readonly saving = signal(false);
  readonly banner = signal<string[]>([]);

  readonly trainers = signal<TrainerLookup[]>([]);
  readonly studios = signal<Studio[]>([]);
  readonly branches: Signal<Branch[]>;

  readonly duration = signal<number>(45);
  readonly descriptionLength = signal(0);
  readonly maxDescription = MAX_DESCRIPTION;
  readonly today = todayIso();

  readonly durationOptions: OptionItem[] = [
    { value: 30, label: '30 min' },
    { value: 45, label: '45 min' },
    { value: 60, label: '60 min' }
  ];

  readonly title = computed(() => {
    switch (this.mode()) {
      case 'add':
        return 'Add New Class';
      case 'edit':
        return 'Edit Class';
      default:
        return 'Class Details';
    }
  });

  readonly state = computed(() => {
    const context = this.context();

    return context
      ? getClassState(context.status, context.bookedPlaces, context.capacityLimit)
      : 'Upcoming';
  });

  /** In Edit the branch cannot change once somebody booked or waits for the class. */
  readonly branchLocked = computed(() => {
    const context = this.context();

    return this.mode() === 'edit' && !!context && context.bookedPlaces + context.waitlistCount > 0;
  });

  /** Original date / time of the session (an unchanged value is never rejected as "in the past"). */
  private original: { date: string; time: string } | null = null;

  private readonly dateRule: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
    const value = String(control.value ?? '');

    if (!value) {
      return null;
    }

    if (this.original && value === this.original.date) {
      return null;
    }

    return value < todayIso() ? { past: 'Date must be today or later.' } : null;
  };

  private readonly timeRule: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
    const time = String(control.value ?? '');
    const date = String(control.parent?.get('sessionDate')?.value ?? '');

    if (!time || !date) {
      return null;
    }

    if (this.original && date === this.original.date && time === this.original.time) {
      return null;
    }

    return date === todayIso() && !isFutureDateTime(date, time)
      ? { past: 'Start time must be later than now.' }
      : null;
  };

  private readonly capacityRule: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as number | null;

    if (value === null || value === undefined) {
      return null;
    }

    if (!Number.isInteger(value) || value < 1 || value > 100) {
      return { whole: 'Capacity must be a whole number between 1 and 100.' };
    }

    const studioId = control.parent?.get('studioId')?.value as number | null | undefined;
    const studio = this.studios().find((item) => item.studioId === studioId);

    if (studio && value > studio.capacity) {
      return { studio: `Capacity cannot be above the studio capacity (${studio.capacity}).` };
    }

    const booked = this.context()?.bookedPlaces ?? 0;

    if (this.mode() === 'edit' && value < booked) {
      return { enrolled: `Capacity cannot be below the current enrolment (${booked}).` };
    }

    return null;
  };

  readonly form = new FormGroup({
    className: new FormControl('', {
      nonNullable: true,
      validators: [ClassDialogComponent.classNameRule]
    }),
    branchId: new FormControl<number | null>(null, [Validators.required]),
    trainerId: new FormControl<number | null>(null),
    studioId: new FormControl<number | null>(null),
    sessionDate: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, this.dateRule]
    }),
    startTime: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, this.timeRule]
    }),
    capacityLimit: new FormControl<number | null>(null, [this.capacityRule]),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(MAX_DESCRIPTION)]
    })
  });

  private readonly sessionId: number | null;

  constructor(
    private readonly dialogRef: MatDialogRef<ClassDialogComponent, boolean>,
    @Inject(MAT_DIALOG_DATA) data: ClassDialogData,
    private readonly classService: ClassService,
    private readonly trainerService: TrainerService,
    private readonly branchService: BranchService,
    private readonly toast: ToastService
  ) {
    this.branches = branchService.branches;
    this.sessionId = data.sessionId ?? null;

    this.form.controls.sessionDate.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.startTime.updateValueAndValidity());

    this.form.controls.studioId.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.capacityLimit.updateValueAndValidity());

    this.form.controls.description.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.descriptionLength.set(value.length));

    this.setMode(data.mode);

    if (data.mode === 'add') {
      const branchId = branchService.currentBranchId();

      this.form.controls.branchId.setValue(branchId);

      if (branchId !== null) {
        this.loadBranchLists(branchId);
      }
    }

    // Only a change made by the user reaches this (patchForm resets without events).
    this.form.controls.branchId.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.onBranchChange());

    if (data.mode !== 'add') {
      if (this.sessionId !== null) {
        this.load(this.sessionId);
      } else {
        this.loadState.set('error');
      }
    }
  }

  private static classNameRule(control: AbstractControl): ValidationErrors | null {
    const length = String(control.value ?? '').trim().length;

    if (length === 0) {
      return { required: true };
    }

    return length < 3 || length > 80
      ? { range: 'Class name must be between 3 and 80 characters.' }
      : null;
  }

  load(sessionId: number = this.sessionId ?? 0): void {
    this.loadState.set('loading');

    this.classService.getBookingContext(sessionId).subscribe({
      next: (context) => {
        this.context.set(context);
        this.patchForm(context);
        this.loadBranchLists(context.branchId);
        this.loadState.set('ready');
      },
      error: () => this.loadState.set('error')
    });
  }

  private onBranchChange(): void {
    const branchId = this.form.controls.branchId.value;

    this.form.controls.trainerId.setValue(null);
    this.form.controls.studioId.setValue(null);
    this.trainers.set([]);
    this.studios.set([]);

    if (branchId !== null) {
      this.loadBranchLists(branchId);
    }
  }

  onDuration(value: string | number | null): void {
    if (value !== null) {
      this.duration.set(Number(value));
      this.form.markAsDirty();
    }
  }

  startEdit(): void {
    this.banner.set([]);
    this.setMode('edit');
  }

  cancel(): void {
    this.dialogRef.close(false);
  }

  save(): void {
    if (this.saving()) {
      return;
    }

    clearServerErrors(this.form);
    this.banner.set([]);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request = this.buildRequest();

    this.saving.set(true);

    if (this.mode() === 'edit' && this.sessionId !== null) {
      this.classService.update(this.sessionId, request).subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.success('Class updated');
          this.dialogRef.close(true);
        },
        error: (error: unknown) => this.onSaveError(error)
      });

      return;
    }

    this.classService.schedule(request).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success('Class scheduled');
        this.dialogRef.close(true);
      },
      error: (error: unknown) => this.onSaveError(error)
    });
  }

  private onSaveError(error: unknown): void {
    this.saving.set(false);

    const messages = applyServerErrors(this.form, error);
    const message = getApiMessage(error);

    if (message && (getHttpStatus(error) === 409 || messages.length === 0)) {
      messages.push(message);
    }

    this.banner.set(messages);
  }

  private buildRequest(): ClassSessionRequest {
    const value = this.form.getRawValue();
    const description = value.description.trim();

    return {
      className: value.className.trim(),
      branchId: value.branchId as number,
      studioId: value.studioId,
      trainerId: value.trainerId,
      sessionDate: value.sessionDate,
      startTime: normalizeTime(value.startTime),
      durationInMinutes: this.duration(),
      capacityLimit: value.capacityLimit ?? DEFAULT_CAPACITY,
      description: description === '' ? null : description
    };
  }

  private patchForm(context: BookingContext): void {
    const time = context.startTime.slice(0, 5);

    this.original = { date: context.sessionDate, time };

    this.form.reset({
      className: context.className,
      branchId: context.branchId,
      trainerId: context.trainerId,
      studioId: context.studioId,
      sessionDate: context.sessionDate,
      startTime: time,
      capacityLimit: context.capacityLimit,
      description: context.description ?? ''
    }, { emitEvent: false });

    this.duration.set(context.durationInMinutes);
    this.descriptionLength.set((context.description ?? '').length);
    this.applyMode();
  }

  private loadBranchLists(branchId: number): void {
    this.trainerService.lookup(branchId).subscribe({
      next: (trainers) => this.trainers.set(trainers),
      error: () => this.trainers.set([])
    });

    this.branchService.getStudios(branchId).subscribe({
      next: (studios) => {
        this.studios.set(studios);
        this.form.controls.capacityLimit.updateValueAndValidity();
      },
      error: () => this.studios.set([])
    });
  }

  private setMode(mode: DialogMode): void {
    this.mode.set(mode);
    this.applyMode();
  }

  private applyMode(): void {
    if (this.mode() === 'view') {
      this.form.disable({ emitEvent: false });
      return;
    }

    this.form.enable({ emitEvent: false });

    if (this.branchLocked()) {
      this.form.controls.branchId.disable({ emitEvent: false });
    }
  }
}
