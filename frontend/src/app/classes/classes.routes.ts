import { Routes } from '@angular/router';

export const CLASSES_ROUTES: Routes = [
  {
    path: '',
    title: 'Class Schedule',
    loadComponent: () =>
      import('./class-schedule/class-schedule.component')
        .then((m) => m.ClassScheduleComponent)
  }
];
