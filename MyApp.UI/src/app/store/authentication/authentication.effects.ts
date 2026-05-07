import { Inject, Injectable } from '@angular/core'
import { Router } from '@angular/router'
import { Actions, createEffect, ofType } from '@ngrx/effects'
import { of } from 'rxjs'
import { catchError, exhaustMap, map } from 'rxjs/operators'
import {
  login,
  loginFailure,
  loginSuccess,
  logout,
  logoutSuccess,
} from './authentication.actions'
import { AuthenticationService } from '@core/service/auth.service'

@Injectable()
export class AuthenticationEffects {
  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(login),
      exhaustMap(({ email, password }) => {
        return this.authenticationService.login(email, password).pipe(
          map((user) => {
            if (user) {
              this.router.navigate(['/dashboard'])
            }

            return loginSuccess({ user })
          }),
          catchError((error) => of(loginFailure({ error })))
        )
      })
    )
  )

  logout$ = createEffect(() =>
    this.actions$.pipe(
      ofType(logout),
      exhaustMap(() => {
        this.authenticationService.logout()
        this.router.navigate(['/auth/login'])
        return of(logoutSuccess())
      })
    )
  )

  constructor(
    @Inject(Actions) private actions$: Actions,
    private authenticationService: AuthenticationService,
    private router: Router
  ) {}
}