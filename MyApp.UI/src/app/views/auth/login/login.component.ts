import { CommonModule } from '@angular/common'
import { Component, inject } from '@angular/core'
import {
  FormsModule,
  ReactiveFormsModule,
  UntypedFormBuilder,
  Validators,
  type UntypedFormGroup,
} from '@angular/forms'
import { Router, RouterModule } from '@angular/router'
import { HttpClient } from '@angular/common/http'
import { AuthLayoutComponent } from '@layouts/auth-layout/auth-layout.component'
import { environment } from '../../../../environments/environment'

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    AuthLayoutComponent,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
  ],
  templateUrl: './login.component.html',
  styles: ``,
})
export class LoginComponent {
  signinForm!: UntypedFormGroup
  submitted: boolean = false
  loading: boolean = false
  errorMessage: string = ''

  public fb = inject(UntypedFormBuilder)
  private http = inject(HttpClient)
  private router = inject(Router)

  constructor() {
    this.signinForm = this.fb.group({
      email: ['user@demo.com', [Validators.required, Validators.email]],
      password: ['123456', [Validators.required]],
    })
  }

  get form() {
    return this.signinForm.controls
  }

  onLogin() {
    this.submitted = true
    this.errorMessage = ''

    if (this.signinForm.invalid) {
      return
    }

    this.loading = true

    const payload = {
      email: this.form['email'].value,
      password: this.form['password'].value,
    }

    this.http.post<any>(`${environment.apiUrl}/auth/login`, payload).subscribe({
      next: (res) => {
        localStorage.setItem('token', res.token)
        localStorage.setItem('user', JSON.stringify(res.user))
        this.router.navigate(['/dashboard'])
      },
      error: (err) => {
        this.loading = false
        this.errorMessage = err?.error?.message || 'Invalid email or password.'
      },
    })
  }
}

// import { CommonModule } from '@angular/common'
// import { Component, inject } from '@angular/core'
// import {
//   FormsModule,
//   ReactiveFormsModule,
//   UntypedFormBuilder,
//   Validators,
//   type UntypedFormGroup,
// } from '@angular/forms'
// import { RouterModule } from '@angular/router'
// import { AuthLayoutComponent } from '@layouts/auth-layout/auth-layout.component'
// import { Store } from '@ngrx/store'
// import { login } from '@store/authentication/authentication.actions'

// @Component({
//   selector: 'app-login',
//   standalone: true,
//   imports: [
//     AuthLayoutComponent,
//     CommonModule,
//     FormsModule,
//     ReactiveFormsModule,
//     RouterModule,
//   ],
//   templateUrl: './login.component.html',
//   styles: ``,
// })
// export class LoginComponent {
//   signinForm!: UntypedFormGroup
//   submitted: boolean = false

//   public fb = inject(UntypedFormBuilder)
//   store = inject(Store)

//   constructor() {
//     this.signinForm = this.fb.group({
//       email: ['user@demo.com', [Validators.required, Validators.email]],
//       password: ['123456', [Validators.required]],
//     })
//   }

//   get form() {
//     return this.signinForm.controls
//   }

//   onLogin() {
//     this.submitted = true
//     if (this.signinForm.valid) {
//       const email = this.form['email'].value // Get the username from the form
//       const password = this.form['password'].value // Get the password from the form

//       // Login Api
//       this.store.dispatch(login({ email: email, password: password }))
//     }
//   }
// }
