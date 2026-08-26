import { Injectable, inject } from '@angular/core'
import { HttpClient, HttpHeaders } from '@angular/common/http'
import { environment } from '../../../environments/environment'
import { AuthenticationService } from '@core/service/auth.service'

@Injectable({
  providedIn: 'root',
})
export class LeadsService {
  private http = inject(HttpClient)
  private authService = inject(AuthenticationService)

  private apiUrl = `${environment.apiUrl}/leads`

  getLeads(page = 1, pageSize = 20) {
    const token = this.authService.getToken()

    const headers = token
      ? new HttpHeaders({
          Authorization: `Bearer ${token}`,
        })
      : new HttpHeaders()

    return this.http.get<any>(
      `${this.apiUrl}?page=${page}&pageSize=${pageSize}`,
      { headers }
    )
  }
}

// import { Injectable, inject } from '@angular/core'
// import { HttpClient } from '@angular/common/http'
// import { environment } from '../../../environments/environment'

// @Injectable({
//   providedIn: 'root',
// })
// export class LeadsService {
//   private http = inject(HttpClient)
//   private apiUrl = `${environment.apiUrl}/leads`

//   getLeads(page = 1, pageSize = 20) {
//     return this.http.get<any>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}`)
//   }
// }