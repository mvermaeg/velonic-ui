import { inject } from '@angular/core'
import { CanActivateFn, Router } from '@angular/router'
import { AuthenticationService } from '@core/service/auth.service'

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthenticationService)
  const router = inject(Router)

  if (authService.isLoggedIn()) {
    return true
  }

  router.navigate(['/auth/login'])
  return false
}