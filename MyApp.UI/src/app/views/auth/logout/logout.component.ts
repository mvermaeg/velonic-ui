import { Component, inject, type OnInit } from '@angular/core'
import { RouterLink } from '@angular/router'
import { AuthLayoutComponent } from '@layouts/auth-layout/auth-layout.component'
import { Store } from '@ngrx/store'
import { logout } from '@store/authentication/authentication.actions'

@Component({
  selector: 'app-logout',
  standalone: true,
  imports: [AuthLayoutComponent, RouterLink],
  templateUrl: './logout.component.html',
  styles: ``,
})
export class LogoutComponent implements OnInit {
  public store = inject(Store)

  ngOnInit(): void {
    this.store.dispatch(logout())
  }
}
