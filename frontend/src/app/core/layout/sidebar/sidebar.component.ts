import { Component, Signal, computed } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { CheckInService } from '../../../check-ins/check-in.service';
import { IconComponent, IconName } from '../../../shared/components/icon/icon.component';
import { AuthService } from '../../services/auth.service';

interface NavItem {
  label: string;
  path: string;
  icon: IconName;
  managerOnly: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, IconComponent],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
  private readonly navItems: NavItem[] = [
    { label: 'Dashboard', path: '/dashboard', icon: 'dashboard', managerOnly: false },
    { label: 'Members', path: '/members', icon: 'members', managerOnly: false },
    { label: 'Classes', path: '/classes', icon: 'classes', managerOnly: false },
    { label: 'Trainers', path: '/trainers', icon: 'trainers', managerOnly: true },
    { label: 'Plans', path: '/plans', icon: 'plans', managerOnly: true }
  ];

  readonly visibleItems: Signal<NavItem[]>;

  constructor(
    private readonly auth: AuthService,
    private readonly checkInService: CheckInService
  ) {
    this.visibleItems = computed(() =>
      this.navItems.filter((item) => !item.managerOnly || this.auth.isManager())
    );
  }

  openCheckIn(): void {
    void this.checkInService.openDialog();
  }
}
