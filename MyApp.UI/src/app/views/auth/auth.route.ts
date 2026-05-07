import { Route } from '@angular/router'
import { ForgotpwComponent } from './forgotpw/forgotpw.component'
import { LockScreenComponent } from './lock-screen/lock-screen.component'
import { LoginComponent } from './login/login.component'
import { LogoutComponent } from './logout/logout.component'
import { RegisterComponent } from './register/register.component'

export const AUTH_ROUTES: Route[] = [
  {
    path: 'login',
    component: LoginComponent,
    data: { title: 'Login' },
  },
  {
    path: 'register',
    component: RegisterComponent,
    data: { title: 'Register' },
  },
  {
    path: 'logout',
    component: LogoutComponent,
    data: { title: 'Logout' },
  },
  {
    path: 'forgotpw',
    component: ForgotpwComponent,
    data: { title: 'Forgot Password' },
  },
  {
    path: 'lock-screen',
    component: LockScreenComponent,
    data: { title: 'Lock Screen' },
  },
]
