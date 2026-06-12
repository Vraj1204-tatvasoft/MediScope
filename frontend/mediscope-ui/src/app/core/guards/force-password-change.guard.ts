import {
  CanActivateFn,
  RouterStateSnapshot,
  ActivatedRouteSnapshot,
  Router
} from '@angular/router';

import { inject } from '@angular/core';

export const forcePasswordChangeGuard:
CanActivateFn = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {

  const router =
    inject(Router);

  const mustChange =
    localStorage.getItem(
      'mustChangePassword'
    ) === 'true';
  if (
    mustChange &&
    !state.url.startsWith(
      '/doctor/profile'
    )
  ) {

    router.navigate([
      '/doctor/profile'
    ]);

    return false;
  }

  return true;
};