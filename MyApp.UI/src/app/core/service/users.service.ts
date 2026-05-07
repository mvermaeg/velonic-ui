import { Injectable, inject } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../environments/environment'
import { Observable } from 'rxjs'

export interface AppUser {
  id: string
  fullName: string
  email: string
  isActive: boolean
  roles: string[]
}

export interface CreateUserRequest {
  fullName: string
  email: string
  password: string
  role: string
}

@Injectable({
  providedIn: 'root',
})
export class UsersService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/users`

  getRoles(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/roles`)
  }

  getUsers(): Observable<AppUser[]> {
    return this.http.get<AppUser[]>(this.apiUrl)
  }

  createUser(model: CreateUserRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, model)
  }

  updateStatus(id: string, isActive: boolean): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/status?isActive=${isActive}`, {})
  }
}