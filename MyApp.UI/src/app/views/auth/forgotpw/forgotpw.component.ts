import { Component } from '@angular/core'
import { RouterLink } from '@angular/router'
import { AuthLayoutComponent } from '@layouts/auth-layout/auth-layout.component'

@Component({
  selector: 'app-forgotpw',
  standalone: true,
  imports: [AuthLayoutComponent, RouterLink],
  templateUrl: './forgotpw.component.html',
  styles: ``,
})
export class ForgotpwComponent {}
