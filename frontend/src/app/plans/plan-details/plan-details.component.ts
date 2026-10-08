import { Location } from '@angular/common';
import { Component, Signal, computed, signal } from '@angular/core';
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
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Params, Router } from '@angular/router';

import { ListContextService } from '../../core/services/list-context.service';
import { ToastService } from '../../core/services/toast.service';
import {
  applyServerErrors,
  clearServerErrors,
  getApiMessage,
  getHttpStatus
} from '../../core/utils/form-errors.util';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { TwoDecimalsDirective } from '../../shared/directives/two-decimals.directive';
import { FieldErrorComponent } from '../../shared/components/field-error/field-error.component';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { OptionGroupComponent, OptionItem } from '../../shared/components/option-group/option-group.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StateMessageComponent } from '../../shared/components/state-message/state-message.component';
import { PlanAccessScope, PlanDetails, PlanRequest } from '../plan.model';
import { PlanService } from '../plan.service';

type PlanMode = 'add' | 'view' | 'edit';
type LoadState = 'loading' | 'ready' | 'notFound' | 'error';

function trimmedLength(label: string, min: number, max: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const length = String(control.value ?? '').trim().length;

    if (length === 0) {
      return { required: true };
    }

    if (length < min || length > max) {
      return { range: `${label} must be between ${min} and ${max} characters.` };
    }

    return null;
  };
}

/** Price with at most 2 decimals. */
function twoDecimals(control: AbstractControl): ValidationErrors | null {
  const value = control.value as number | null;

  if (value === null || value === undefined) {
    return null;
  }

  return Math.abs(Math.round(value * 100) - value * 100) < 1e-6
    ? null
    : { decimals: 'Price can have at most 2 decimal places.' };
}

/** Whole number inside [min, max]; empty is allowed (optional fields). */
function wholeNumber(label: string, min: number, max: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value as number | null;

    if (value === null || value === undefined) {
      return null;
    }

    if (!Number.isInteger(value) || value < min || value > max) {
      return { whole: `${label} must be a whole number between ${min} and ${max}.` };
    }

    return null;
  };
}

/** Number of freezes must be 0 when the maximum freeze days is 0 (cross-field). */
const freezesNeedFreezeDays: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const freezes = (control.value as number | null) ?? 0;
  const days = (control.parent?.get('maximumFreezeDays')?.value as number | null) ?? 0;

  return days === 0 && freezes > 0
    ? { freezes: 'Maximum number of freezes must be 0 when freeze days is 0.' }
    : null;
};

/**
 * ONE component for the three plan screens: add (/plans/new), view
 * (/plans/:id) and update (/plans/:id/edit). The mode comes from the route
 * data and can be switched in place (Edit Plan / Save / Cancel).
 */
@Component({
  selector: 'app-plan-details',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TwoDecimalsDirective,
    FieldErrorComponent,
    IconComponent,
    OptionGroupComponent,
    PageHeaderComponent,
    StateMessageComponent
  ],
  templateUrl: './plan-details.component.html',
  styleUrl: './plan-details.component.css'
})
export class PlanDetailsComponent {
  readonly mode = signal<PlanMode>('add');
  readonly loadState = signal<LoadState>('loading');
  readonly plan = signal<PlanDetails | null>(null);
  readonly saving = signal(false);
  readonly banner = signal<string[]>([]);
  readonly accessScope = signal<PlanAccessScope | null>(null);

  readonly accessOptions: OptionItem[] = [
    { value: 'HomeBranchOnly', label: 'Home branch only' },
    { value: 'AllBranches', label: 'All branches' }
  ];

  /** True when "Edit Plan" was pressed on the view screen (Cancel then returns to view). */
  private readonly cameFromView = signal(false);

  readonly form = new FormGroup({
    planName: new FormControl('', {
      nonNullable: true,
      validators: [trimmedLength('Plan name', 2, 60)]
    }),
    price: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0),
      twoDecimals
    ]),
    durationInMonths: new FormControl<number | null>(null, [
      Validators.required,
      wholeNumber('Duration', 1, 36)
    ]),
    isPublished: new FormControl(false, { nonNullable: true }),
    maximumFreezeDays: new FormControl<number | null>(null, [
      wholeNumber('Maximum freeze days', 0, 3650)
    ]),
    maximumNumberOfFreezes: new FormControl<number | null>(null, [
      wholeNumber('Maximum number of freezes', 0, 1000),
      freezesNeedFreezeDays
    ]),
    guestPassQuota: new FormControl<number | null>(null, [
      wholeNumber('Guest pass quota', 0, 1000)
    ])
  });

  readonly backQueryParams: Params;

  readonly title = computed(() =>
    this.mode() === 'add' ? 'New Plan' : (this.plan()?.planName ?? 'Plan')
  );

  readonly subtitle = computed(() =>
    this.mode() === 'add' ? 'Publish a new membership plan.' : 'Plan details'
  );

  readonly modeLabel = computed(() => {
    switch (this.mode()) {
      case 'add':
        return 'Add mode';
      case 'edit':
        return 'Update mode';
      default:
        return 'View mode';
    }
  });

  readonly soldCount: Signal<number> = computed(() => this.plan()?.soldMembershipsCount ?? 0);

  private planId: number | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly location: Location,
    private readonly dialog: MatDialog,
    private readonly planService: PlanService,
    private readonly toast: ToastService,
    private readonly listContext: ListContextService
  ) {
    this.backQueryParams = this.listContext.get('plans');

    // Re-check "number of freezes" whenever "freeze days" changes.
    this.form.controls.maximumFreezeDays.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.maximumNumberOfFreezes.updateValueAndValidity());

    const mode = (this.route.snapshot.data['mode'] as PlanMode | undefined) ?? 'add';
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.setMode(mode);

    if (mode === 'add') {
      this.loadState.set('ready');
    } else if (Number.isInteger(id) && id > 0) {
      this.planId = id;
      this.load();
    } else {
      this.loadState.set('notFound');
    }
  }

  load(): void {
    if (this.planId === null) {
      return;
    }

    this.loadState.set('loading');

    this.planService.getById(this.planId).subscribe({
      next: (plan) => {
        this.plan.set(plan);
        this.patchForm(plan);
        this.loadState.set('ready');
      },
      error: (error: unknown) => {
        this.loadState.set(getHttpStatus(error) === 404 ? 'notFound' : 'error');
      }
    });
  }

  onAccessChange(value: string | number | null): void {
    this.accessScope.set(value === 'AllBranches' || value === 'HomeBranchOnly' ? value : null);
    this.form.markAsDirty();
  }

  startEdit(): void {
    if (this.planId === null) {
      return;
    }

    this.cameFromView.set(true);
    this.banner.set([]);
    this.setMode('edit');
    this.location.go(`/plans/${this.planId}/edit`);
  }

  cancel(): void {
    const leave = (): void => {
      const plan = this.plan();

      if (this.mode() === 'edit' && this.cameFromView() && plan && this.planId !== null) {
        this.patchForm(plan);
        this.banner.set([]);
        this.setMode('view');
        this.location.go(`/plans/${this.planId}`);

        return;
      }

      this.router.navigate(['/plans'], {
        queryParams: this.listContext.get('plans')
      });
    };

    if (!this.form.dirty) {
      leave();
      return;
    }

    this.confirm({
      title: 'Discard changes?',
      message: 'You have unsaved changes. If you leave now they will be lost.',
      confirmLabel: 'Discard',
      cancelLabel: 'Keep editing',
      danger: true
    }, leave);
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

    if (this.mode() === 'add') {
      this.planService.create(request).subscribe({
        next: (created) => {
          this.saving.set(false);
          this.form.markAsPristine();
          this.toast.success(`${request.planName} created`);
          this.router.navigate(['/plans', created.planId]);
        },
        error: (error: unknown) => this.onSaveError(error)
      });

      return;
    }

    if (this.planId === null) {
      this.saving.set(false);
      return;
    }

    const planId = this.planId;

    this.planService.update(planId, request).subscribe({
      next: () => {
        const saved: PlanDetails = {
          planId,
          ...request,
          soldMembershipsCount: this.plan()?.soldMembershipsCount ?? 0
        };

        this.saving.set(false);
        this.plan.set(saved);
        this.patchForm(saved);
        this.setMode('view');
        this.location.go(`/plans/${planId}`);
        this.toast.success('Plan updated');
      },
      error: (error: unknown) => this.onSaveError(error)
    });
  }

  private onSaveError(error: unknown): void {
    this.saving.set(false);

    const messages = applyServerErrors(this.form, error);

    if (getHttpStatus(error) === 409) {
      const message = getApiMessage(error);

      if (message) {
        messages.push(message);
      }
    }

    this.banner.set(messages);
  }

  private buildRequest(): PlanRequest {
    const value = this.form.getRawValue();

    return {
      planName: value.planName.trim(),
      price: value.price as number,
      durationInMonths: value.durationInMonths as number,
      maximumFreezeDays: value.maximumFreezeDays ?? 0,
      maximumNumberOfFreezes: value.maximumNumberOfFreezes ?? 0,
      guestPassQuota: value.guestPassQuota ?? 0,
      // neither card selected is allowed: it is sent as HomeBranchOnly
      accessScope: this.accessScope() ?? 'HomeBranchOnly',
      isPublished: value.isPublished
    };
  }

  private patchForm(plan: PlanDetails): void {
    this.form.reset({
      planName: plan.planName,
      price: plan.price,
      durationInMonths: plan.durationInMonths,
      isPublished: plan.isPublished,
      maximumFreezeDays: plan.maximumFreezeDays,
      maximumNumberOfFreezes: plan.maximumNumberOfFreezes,
      guestPassQuota: plan.guestPassQuota
    });

    this.accessScope.set(plan.accessScope);
  }

  private setMode(mode: PlanMode): void {
    this.mode.set(mode);

    if (mode === 'view') {
      this.form.disable({ emitEvent: false });
    } else {
      this.form.enable({ emitEvent: false });
    }
  }

  private confirm(data: ConfirmDialogData, onConfirm: () => void): void {
    this.dialog
      .open<ConfirmDialogComponent, ConfirmDialogData, boolean>(ConfirmDialogComponent, {
        data,
        panelClass: 'titan-dialog',
        width: '420px'
      })
      .afterClosed()
      .subscribe((confirmed) => {
        if (confirmed) {
          onConfirm();
        }
      });
  }
}
