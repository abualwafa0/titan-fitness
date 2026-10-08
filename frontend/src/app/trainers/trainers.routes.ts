import { Routes } from '@angular/router';

export const TRAINERS_ROUTES: Routes = [
  {
    path: '',
    title: 'Trainers',
    loadComponent: () =>
      import('./trainer-directory/trainer-directory.component')
        .then((m) => m.TrainerDirectoryComponent)
  },
  {
    path: 'new',
    title: 'New Trainer',
    data: { mode: 'add' },
    loadComponent: () =>
      import('./trainer-details/trainer-details.component')
        .then((m) => m.TrainerDetailsComponent)
  },
  {
    path: ':id/edit',
    title: 'Update Trainer',
    data: { mode: 'edit' },
    loadComponent: () =>
      import('./trainer-details/trainer-details.component')
        .then((m) => m.TrainerDetailsComponent)
  },
  {
    path: ':id',
    title: 'Trainer',
    data: { mode: 'view' },
    loadComponent: () =>
      import('./trainer-details/trainer-details.component')
        .then((m) => m.TrainerDetailsComponent)
  }
];
