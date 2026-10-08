import { Routes } from '@angular/router';

import { managerGuard } from './core/guards/manager.guard';
import { ShellComponent } from './core/layout/shell/shell.component';

export const routes: Routes = [
  // ----- Pages outside the shell (no sidebar / header) -----
  {
    path: 'login',
    title: 'Sign in',
    loadComponent: () =>
      import('./core/pages/login/login.component')
        .then((m) => m.LoginComponent)
  },

  // ----- Staff portal (inside the shell) -----
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: 'Dashboard',
        loadComponent: () =>
          import('./dashboard/dashboard.component')
            .then((m) => m.DashboardComponent)
      },
      {
        path: 'members',
        loadChildren: () =>
          import('./members/members.routes')
            .then((m) => m.MEMBERS_ROUTES)
      },
      {
        path: 'classes',
        loadChildren: () =>
          import('./classes/classes.routes')
            .then((m) => m.CLASSES_ROUTES)
      },
      {
        path: 'trainers',
        canActivate: [managerGuard],
        loadChildren: () =>
          import('./trainers/trainers.routes')
            .then((m) => m.TRAINERS_ROUTES)
      },
      {
        path: 'plans',
        canActivate: [managerGuard],
        loadChildren: () =>
          import('./plans/plans.routes')
            .then((m) => m.PLANS_ROUTES)
      },
      {
        path: 'access-denied',
        title: 'Access denied',
        loadComponent: () =>
          import('./core/pages/access-denied/access-denied.component')
            .then((m) => m.AccessDeniedComponent)
      },
      {
        path: '**',
        title: 'Page not found',
        loadComponent: () =>
          import('./core/pages/not-found/not-found.component')
            .then((m) => m.NotFoundComponent)
      }
    ]
  }
];
