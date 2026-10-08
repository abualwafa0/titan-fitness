import { Component, Inject, Signal, signal } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { Branch } from '../../core/models/branch.model';
import { BranchService } from '../../core/services/branch.service';
import { ToastService } from '../../core/services/toast.service';
import {
  applyServerErrors,
  clearServerErrors,
  getApiMessage,
  getHttpStatus
} from '../../core/utils/form-errors.util';
import { FieldErrorComponent } from '../../shared/components/field-error/field-error.component';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { AutofocusDirective } from '../../shared/directives/autofocus.directive';
import {
  MemberFormSource,
  MemberSaveResult,
  formatMemberNumber
} from '../member.model';
import { MemberService } from '../member.service';

export interface MemberFormDialogData {
  mode: 'add' | 'edit';
  member?: MemberFormSource;
}

/** Trimmed name, 2-80 characters, letters / spaces / hyphens / apostrophes only. */
function memberName(control: AbstractControl): ValidationErrors | null {
  const value = String(control.value ?? '').trim();

  if (value.length === 0) {
    return { required: true };
  }

  if (value.length < 2 || value.length > 80) {
    return { range: 'Member name must be between 2 and 80 characters.' };
  }

  if (!/^[\p{L}\s'’-]+$/u.test(value)) {
    return { letters: 'Use letters, spaces, hyphens and apostrophes only.' };
  }

  return null;
}

/** Add Member / Edit Member dialog. Closes with the saved result (or nothing when cancelled). */
@Component({
  selector: 'app-member-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    AutofocusDirective,
    FieldErrorComponent,
    IconComponent,
    StatusBadgeComponent
  ],
  templateUrl: './member-form-dialog.component.html',
  styleUrl: './member-form-dialog.component.css'
})
export class MemberFormDialogComponent {
  readonly mode: 'add' | 'edit';
  readonly member: MemberFormSource | null;
  readonly branches: Signal<Branch[]>;

  readonly saving = signal(false);
  readonly banner = signal<string[]>([]);

  readonly form = new FormGroup({
    fullName: new FormControl('', { nonNullable: true, validators: [memberName] }),
    homeBranchId: new FormControl<number | null>(null, [Validators.required])
  });

  constructor(
    private readonly dialogRef: MatDialogRef<MemberFormDialogComponent, MemberSaveResult>,
    @Inject(MAT_DIALOG_DATA) data: MemberFormDialogData,
    private readonly memberService: MemberService,
    private readonly toast: ToastService,
    branchService: BranchService
  ) {
    this.mode = data.mode;
    this.member = data.mode === 'edit' ? (data.member ?? null) : null;
    this.branches = branchService.branches;

    if (this.member) {
      this.form.setValue({
        fullName: this.member.fullName,
        homeBranchId: this.member.homeBranchId
      });
    } else {
      this.form.controls.homeBranchId.setValue(branchService.currentBranchId());
    }
  }

  get title(): string {
    return this.mode === 'add' ? 'Add Member' : 'Edit Member';
  }

  get memberNumber(): string {
    return formatMemberNumber(this.member?.membershipNumber);
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

    const fullName = this.form.controls.fullName.value.trim();
    const branchId = this.form.controls.homeBranchId.value as number;

    this.saving.set(true);

    if (this.mode === 'edit' && this.member) {
      const memberId = this.member.memberId;

      this.memberService.update(memberId, fullName, branchId).subscribe({
        next: () => {
          this.saving.set(false);
          this.toast.success('Member updated');
          this.dialogRef.close({ memberId });
        },
        error: (error: unknown) => this.onSaveError(error)
      });

      return;
    }

    this.memberService.create(fullName, branchId).subscribe({
      next: (created) => {
        this.saving.set(false);
        this.toast.success('Member created');
        this.dialogRef.close({ memberId: created.memberId });
      },
      error: (error: unknown) => this.onSaveError(error)
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }

  private onSaveError(error: unknown): void {
    this.saving.set(false);

    const messages = applyServerErrors(this.form, error);
    const status = getHttpStatus(error);

    if (status === 409 || (status === 400 && messages.length === 0)) {
      const message = getApiMessage(error);

      if (message) {
        messages.push(message);
      }
    }

    this.banner.set(messages);
  }
}
