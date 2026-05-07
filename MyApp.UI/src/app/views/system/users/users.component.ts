import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import {
  ReactiveFormsModule,
  UntypedFormBuilder,
  Validators,
  type UntypedFormGroup,
} from '@angular/forms'
import { UsersService, AppUser } from '@core/service/users.service'

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './users.component.html',
})
export class UsersComponent implements OnInit {
  private usersService = inject(UsersService)
  private fb = inject(UntypedFormBuilder)

  users: AppUser[] = []
  roles: string[] = []

  userForm!: UntypedFormGroup

  loading = false
  saving = false
  submitted = false
  errorMessage = ''
  successMessage = ''

  ngOnInit(): void {
    this.userForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['123456', [Validators.required, Validators.minLength(6)]],
      role: ['', Validators.required],
    })

    this.loadRoles()
    this.loadUsers()
  }

  get form() {
    return this.userForm.controls
  }

  loadRoles() {
    this.usersService.getRoles().subscribe({
      next: (res) => {
        this.roles = res
      },
      error: () => {
        this.errorMessage = 'Unable to load roles.'
      },
    })
  }

  loadUsers() {
    this.loading = true

    this.usersService.getUsers().subscribe({
      next: (res) => {
        this.users = res
        this.loading = false
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Unable to load users.'
      },
    })
  }

  createUser() {
    this.submitted = true
    this.errorMessage = ''
    this.successMessage = ''

    if (this.userForm.invalid) {
      return
    }

    this.saving = true

    this.usersService.createUser(this.userForm.value).subscribe({
      next: () => {
        this.saving = false
        this.successMessage = 'User created successfully.'
        this.userForm.reset({
          fullName: '',
          email: '',
          password: '123456',
          role: '',
        })
        this.submitted = false
        this.loadUsers()
      },
      error: (err) => {
        this.saving = false
        this.errorMessage =
          err?.error?.message ||
          err?.error?.[0]?.description ||
          'Unable to create user.'
      },
    })
  }

  toggleStatus(user: AppUser) {
    this.usersService.updateStatus(user.id, !user.isActive).subscribe({
      next: () => {
        this.loadUsers()
      },
      error: () => {
        this.errorMessage = 'Unable to update user status.'
      },
    })
  }
}