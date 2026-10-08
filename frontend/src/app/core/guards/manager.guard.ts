import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../services/auth.service';

/** Trainers and Plans are for the branch manager only; front-desk staff see "Access denied". */
export const managerGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.isManager() ? true : router.createUrlTree(['/access-denied']);
};
