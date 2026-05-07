import { Injectable, inject } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../environments/environment'
import { Observable, map } from 'rxjs'

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  private http = inject(HttpClient)

  get session(): string | null {
    return localStorage.getItem('token')
  }
removeSession() {
  localStorage.removeItem('token')
  localStorage.removeItem('user')
}
  login(email: string, password: string): Observable<any> {
    return this.http
      .post<any>(`${environment.apiUrl}/auth/login`, {
        email,
        password,
      })
      .pipe(
        map((res) => {
          localStorage.setItem('token', res.token)
          localStorage.setItem('user', JSON.stringify(res.user))
          return res.user
        })
      )
  }

  logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  }

  getToken(): string | null {
    return localStorage.getItem('token')
  }

  isLoggedIn(): boolean {
    return !!this.getToken()
  }
}