import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';
import { Router } from '@angular/router';

import { IconComponent } from '../../shared/components/icon/icon.component';
import { MemberProfile } from '../member.model';

/** Current plan of the member: name, price, renewal date and the Freeze Membership button. */
@Component({
  selector: 'app-plan-card',
  standalone: true,
  imports: [CurrencyPipe, DatePipe, IconComponent],
  templateUrl: './plan-card.component.html',
  styleUrl: './plan-card.component.css'
})
export class PlanCardComponent {
  readonly profile = input.required<MemberProfile>();

  readonly membership = computed(() => this.profile().currentMembership);

  /** "/year", "/month" or "/N months" (needs the matching entry of memberships[]). */
  readonly period = computed(() => {
    const current = this.membership();

    if (!current) {
      return '';
    }

    const months = this.profile().memberships.find(
      (item) => item.membershipId === current.membershipId
    )?.durationInMonths;

    if (!months) {
      return '';
    }

    if (months === 12) {
      return '/year';
    }

    return months === 1 ? '/month' : `/${months} months`;
  });

  readonly freezesRemaining = computed(() => {
    const current = this.membership();

    return current ? current.maximumNumberOfFreezes - current.freezesUsed : 0;
  });

  readonly freezeDisabledReason = computed<string | null>(() => {
    const status = this.profile().status;

    if (status !== 'Active') {
      return status === 'Frozen'
        ? 'Membership is Frozen'
        : status === 'Expired'
          ? 'Membership expired'
          : 'Membership is not active';
    }

    return this.freezesRemaining() <= 0 ? 'No freezes remaining on this plan' : null;
  });

  constructor(private readonly router: Router) {}

  freeze(): void {
    if (this.freezeDisabledReason() === null) {
      this.router.navigate(['/members', this.profile().memberId, 'freeze']);
    }
  }
}
