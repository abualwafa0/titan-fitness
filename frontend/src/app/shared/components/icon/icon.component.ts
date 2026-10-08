import { Component, computed, input } from '@angular/core';

export type IconName =
  | 'logo'
  | 'dashboard'
  | 'members'
  | 'classes'
  | 'trainers'
  | 'plans'
  | 'help'
  | 'bell'
  | 'calendar'
  | 'calendar-plus'
  | 'search'
  | 'plus'
  | 'close'
  | 'check'
  | 'info'
  | 'arrow-left'
  | 'chevron-down'
  | 'chevron-left'
  | 'chevron-right'
  | 'more-vertical'
  | 'pencil'
  | 'filter'
  | 'snowflake'
  | 'chart'
  | 'clock'
  | 'pin'
  | 'user'
  | 'users'
  | 'user-plus'
  | 'user-check'
  | 'door'
  | 'entry'
  | 'mail'
  | 'phone'
  | 'location'
  | 'ticket'
  | 'qr';

/** Names that reuse the drawing of another icon. */
const ALIASES: Partial<Record<IconName, IconName>> = {
  users: 'members',
  location: 'pin'
};

/** Inline SVG icon (no icon font, no external request). Uses currentColor. */
@Component({
  selector: 'app-icon',
  standalone: true,
  templateUrl: './icon.component.html',
  styleUrl: './icon.component.css'
})
export class IconComponent {
  readonly name = input.required<IconName>();
  readonly size = input<number>(18);

  readonly shape = computed<IconName>(() => ALIASES[this.name()] ?? this.name());
}
