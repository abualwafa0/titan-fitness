import { Location } from '@angular/common';
import { Component, Signal, computed, signal } from '@angular/core';
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

import { Branch } from '../../core/models/branch.model';
import { BranchService } from '../../core/services/branch.service';
import { ListContextService } from '../../core/services/list-context.service';
import { ToastService } from '../../core/services/toast.service';
import {
  applyServerErrors,
  clearServerErrors,
  getApiMessage,
  getHttpStatus
} from '../../core/utils/form-errors.util';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { FieldErrorComponent } from '../../shared/components/field-error/field-error.component';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StateMessageComponent } from '../../shared/components/state-message/state-message.component';
import { TrainerDetails, TrainerRequest, formatTrainerNumber } from '../trainer.model';
import { TrainerService } from '../trainer.service';

type TrainerMode = 'add' | 'view' | 'edit';
type LoadState = 'loading' | 'ready' | 'notFound' | 'error';

/** Required text whose trimmed length must be between min and max. */
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

/** Optional phone number: digits, spaces and + ( ) - . only. */
function optionalPhone(control: AbstractControl): ValidationErrors | null {
  const value = String(control.value ?? '').trim();

  if (value === '') {
    return null;
  }

  const valid = /^\+?[0-9\s().-]{7,20}$/.test(value) && /\d{7,}/.test(value.replace(/\D/g, ''));

  return valid ? null : { phone: 'Enter a valid phone number.' };
}

/**
 * ONE component for the three trainer screens: add (/trainers/new),
 * view (/trainers/:id) and update (/trainers/:id/edit). The mode comes from
 * the route data and can be switched in place (Edit Trainer / Save / Cancel).
 */
@Component({
  selector: 'app-trainer-details',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    FieldErrorComponent,
    IconComponent,
    PageHeaderComponent,
    StateMessageComponent
  ],
  templateUrl: './trainer-details.component.html',
  styleUrl: './trainer-details.component.css'
})
export class TrainerDetailsComponent {
  readonly mode = signal<TrainerMode>('add');
  readonly loadState = signal<LoadState>('loading');
  readonly trainer = signal<TrainerDetails | null>(null);
  readonly saving = signal(false);
  readonly banner = signal<string[]>([]);

  /** True when "Edit Trainer" was pressed on the view screen (Cancel then returns to view). */
  private readonly cameFromView = signal(false);

  readonly form = new FormGroup({
    trainerName: new FormControl('', {
      nonNullable: true,
      validators: [trimmedLength('Trainer name', 2, 80)]
    }),
    specialty: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(100)]
    }),
    branchId: new FormControl<number | null>(null, [Validators.required]),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(150)]
    }),
    phone: new FormControl('', { nonNullable: true, validators: [optionalPhone] }),
    isActive: new FormControl(true, { nonNullable: true })
  });

  readonly branches: Signal<Branch[]>;

  readonly title = computed(() => {
    if (this.mode() === 'add') {
      return 'New Trainer';
    }

    return this.trainer()?.trainerName ?? 'Trainer';
  });

  readonly subtitle = computed(() => {
    if (this.mode() === 'add') {
      return 'Add a trainer to the roster.';
    }

    const number = formatTrainerNumber(this.trainer()?.trainerNumber);

    return number ? `Trainer ${number}` : '';
  });

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

  readonly backQueryParams: Params;

  private trainerId: number | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly location: Location,
    private readonly dialog: MatDialog,
    private readonly trainerService: TrainerService,
    private readonly toast: ToastService,
    private readonly listContext: ListContextService,
    branchService: BranchService
  ) {
    this.branches = branchService.branches;
    this.backQueryParams = this.listContext.get('trainers');

    const mode = (this.route.snapshot.data['mode'] as TrainerMode | undefined) ?? 'add';
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.setMode(mode);

    if (mode === 'add') {
      this.loadState.set('ready');
    } else if (Number.isInteger(id) && id > 0) {
      this.trainerId = id;
      this.load();
    } else {
      this.loadState.set('notFound');
    }
  }

  load(): void {
    if (this.trainerId === null) {
      return;
    }

    this.loadState.set('loading');

    this.trainerService.getById(this.trainerId).subscribe({
      next: (trainer) => {
        this.trainer.set(trainer);
        this.patchForm(trainer);
        this.loadState.set('ready');
      },
      error: (error: unknown) => {
        this.loadState.set(getHttpStatus(error) === 404 ? 'notFound' : 'error');
      }
    });
  }

  startEdit(): void {
    if (this.trainerId === null) {
      return;
    }

    this.cameFromView.set(true);
    this.banner.set([]);
    this.setMode('edit');
    this.location.go(`/trainers/${this.trainerId}/edit`);
  }

  cancel(): void {
    const leave = (): void => {
      const trainer = this.trainer();

      if (this.mode() === 'edit' && this.cameFromView() && trainer && this.trainerId !== null) {
        this.patchForm(trainer);
        this.banner.set([]);
        this.setMode('view');
        this.location.go(`/trainers/${this.trainerId}`);

        return;
      }

      this.router.navigate(['/trainers'], {
        queryParams: this.listContext.get('trainers')
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
    const original = this.trainer();

    if (this.mode() === 'edit' && original?.isActive && !request.isActive) {
      this.confirm({
        title: 'Deactivate this trainer?',
        message:
          'Sessions already scheduled for this trainer will be left without a trainer. Do you want to continue?',
        confirmLabel: 'Deactivate',
        danger: true
      }, () => this.send(request));

      return;
    }

    this.send(request);
  }

  private send(request: TrainerRequest): void {
    this.saving.set(true);

    if (this.mode() === 'add') {
      this.trainerService.create(request).subscribe({
        next: (created) => {
          this.saving.set(false);
          this.form.markAsPristine();
          this.toast.success('Trainer created');
          this.router.navigate(['/trainers', created.trainerId]);
        },
        error: (error: unknown) => this.onSaveError(error)
      });

      return;
    }

    if (this.trainerId === null) {
      this.saving.set(false);
      return;
    }

    const trainerId = this.trainerId;

    this.trainerService.update(trainerId, request).subscribe({
      next: () => {
        const previous = this.trainer();
        const branchName =
          this.branches().find((branch) => branch.branchId === request.branchId)?.branchName ??
          previous?.branchName ??
          '';

        const saved: TrainerDetails = {
          trainerId,
          trainerNumber: previous?.trainerNumber ?? '',
          trainerName: request.trainerName,
          specialty: request.specialty,
          email: request.email,
          phone: request.phone,
          branchId: request.branchId,
          branchName,
          isActive: request.isActive
        };

        this.saving.set(false);
        this.trainer.set(saved);
        this.patchForm(saved);
        this.setMode('view');
        this.location.go(`/trainers/${trainerId}`);
        this.toast.success('Trainer updated');
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

  private buildRequest(): TrainerRequest {
    const value = this.form.getRawValue();
    const specialty = value.specialty.trim();
    const phone = value.phone.trim();

    return {
      trainerName: value.trainerName.trim(),
      specialty: specialty === '' ? null : specialty,
      email: value.email.trim(),
      phone: phone === '' ? null : phone,
      branchId: value.branchId as number,
      isActive: value.isActive
    };
  }

  private patchForm(trainer: TrainerDetails): void {
    this.form.reset({
      trainerName: trainer.trainerName,
      specialty: trainer.specialty ?? '',
      branchId: trainer.branchId,
      email: trainer.email ?? '',
      phone: trainer.phone ?? '',
      isActive: trainer.isActive
    });
  }

  private setMode(mode: TrainerMode): void {
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
