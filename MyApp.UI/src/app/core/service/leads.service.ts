import { Injectable, inject } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../environments/environment'

@Injectable({
  providedIn: 'root',
})
export class LeadsService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/leads`

  getLeads(page = 1, pageSize = 20) {
    return this.http.get<any>(`${this.apiUrl}?page=${page}&pageSize=${pageSize}`)
  }
}