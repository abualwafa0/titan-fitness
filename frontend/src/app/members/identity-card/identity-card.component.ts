import { DatePipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';

import { AvatarComponent } from '../../shared/components/avatar/avatar.component';
import { IconComponent, } from '../../shared/components/icon/icon.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { MemberProfile, formatMemberNumber } from '../member.model';

/** Left card of the member profile: photo, status, name, ID and contact details. */
@Component({
  selector: 'app-identity-card',
  standalone: true,
  imports: [DatePipe, AvatarComponent, IconComponent, StatusBadgeComponent],
  templateUrl: './identity-card.component.html',
  styleUrl: './identity-card.component.css'
})
export class IdentityCardComponent {
  readonly profile = input.required<MemberProfile>();

  readonly idLabel = computed(() => formatMemberNumber(this.profile().membershipNumber));
}
