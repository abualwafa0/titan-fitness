import { Routes } from '@angular/router';

export const MEMBERS_ROUTES: Routes = [
  {
    path: '',
    title: 'Members',
    loadComponent: () =>
      import('./member-directory/member-directory.component')
        .then((m) => m.MemberDirectoryComponent)
  },
  {
    path: ':id/freeze',
    title: 'Freeze Membership',
    loadComponent: () =>
      import('./freeze-membership/freeze-membership.component')
        .then((m) => m.FreezeMembershipComponent)
  },
  {
    path: ':id',
    title: 'Member Profile',
    loadComponent: () =>
      import('./member-profile/member-profile.component')
        .then((m) => m.MemberProfileComponent)
  }
];
