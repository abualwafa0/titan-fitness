import { Routes } from '@angular/router';

export const PLANS_ROUTES: Routes = [
  {
    path: '',
    title: 'Plans',
    loadComponent: () =>
      import('./plan-catalogue/plan-catalogue.component')
        .then((m) => m.PlanCatalogueComponent)
  },
  {
    path: 'new',
    title: 'New Plan',
    data: { mode: 'add' },
    loadComponent: () =>
      import('./plan-details/plan-details.component')
        .then((m) => m.PlanDetailsComponent)
  },
  {
    path: ':id/edit',
    title: 'Update Plan',
    data: { mode: 'edit' },
    loadComponent: () =>
      import('./plan-details/plan-details.component')
        .then((m) => m.PlanDetailsComponent)
  },
  {
    path: ':id',
    title: 'Plan',
    data: { mode: 'view' },
    loadComponent: () =>
      import('./plan-details/plan-details.component')
        .then((m) => m.PlanDetailsComponent)
  }
];
