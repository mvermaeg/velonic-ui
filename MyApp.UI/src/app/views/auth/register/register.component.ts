import { CommonModule } from '@angular/common'
import { Component, inject } from '@angular/core'
import {
  FormsModule,
  ReactiveFormsModule,
  UntypedFormBuilder,
  Validators,
  type UntypedFormGroup,
} from '@angular/forms'
import { HttpClient } from '@angular/common/http'
import { RouterModule, Router } from '@angular/router'
import { AuthLayoutComponent } from '@layouts/auth-layout/auth-layout.component'
import { environment } from '../../../../environments/environment'

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    AuthLayoutComponent,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
  ],
  templateUrl: './register.component.html',
  styles: ``,
})
export class RegisterComponent {
  registerForm!: UntypedFormGroup
  submitted = false
  loading = false
  errorMessage = ''
  successMessage = ''

  private fb = inject(UntypedFormBuilder)
  private http = inject(HttpClient)
  private router = inject(Router)

  constructor() {
    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      acceptTerms: [false, [Validators.requiredTrue]],
    })
  }

  get form() {
    return this.registerForm.controls
  }

  onRegister() {
    this.submitted = true
    this.errorMessage = ''
    this.successMessage = ''

    if (this.registerForm.invalid) {
      return
    }

    this.loading = true

    const payload = {
      fullName: this.form['fullName'].value,
      email: this.form['email'].value,
      password: this.form['password'].value,
    }

    this.http.post<any>(`${environment.apiUrl}/auth/register`, payload).subscribe({
      next: () => {
        this.loading = false
        this.successMessage = 'Registration successful. Please login.'
        setTimeout(() => {
          this.router.navigate(['/auth/login'])
        }, 1000)
      },
      error: (err) => {
        this.loading = false
        this.errorMessage =
          err?.error?.message ||
          err?.error?.[0]?.description ||
          'Registration failed. Please try again.'
      },
    })
  }
}


// import { Component } from '@angular/core'
// import { RouterLink } from '@angular/router'
// import { AuthLayoutComponent } from '@layouts/auth-layout/auth-layout.component'

// @Component({
//   selector: 'app-register',
//   standalone: true,
//   imports: [AuthLayoutComponent, RouterLink],
//   templateUrl: './register.component.html',
//   styles: ``,
// })
// export class RegisterComponent {}
