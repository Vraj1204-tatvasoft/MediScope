import { inject } from '@angular/core';
import { CanActivateFn, ActivatedRouteSnapshot, Router } from '@angular/router';
import { TokenService } from '../services/token.service';
 
export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const tokenService   = inject(TokenService);
  const router         = inject(Router);
  const allowedRoles   = route.data['roles'] as string[];
  const userRole       = tokenService.getRole();
 
  if (userRole && allowedRoles.includes(userRole)) {
    return true;
  }
  router.navigate(['/unauthorized']);
  return false;
};