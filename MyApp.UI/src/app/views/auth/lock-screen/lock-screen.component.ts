import { Component } from '@angular/core'
import { RouterLink } from '@angular/router'
import { AuthLayoutComponent } from '@layouts/auth-layout/auth-layout.component'

@Component({
  selector: 'app-lock-screen',
  standalone: true,
  imports: [AuthLayoutComponent, RouterLink],
  templateUrl: './lock-screen.component.html',
  styles: ``,
})
export class LockScreenComponent {}
