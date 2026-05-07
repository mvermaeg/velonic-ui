import { Injectable, inject } from '@angular/core'
import { HttpClient, HttpParams } from '@angular/common/http'
import { environment } from '../../../environments/environment'

@Injectable({ providedIn: 'root' })
export class LeadReturnsService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/lead-returns`

  getReturns(filters: any) {
    let params = new HttpParams()

    Object.keys(filters).forEach((key) => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.set(key, filters[key])
      }
    })

    return this.http.get<any>(this.apiUrl, { params })
  }

  reviewReturn(id: number, status: string) {
    return this.http.put<any>(`${this.apiUrl}/${id}/review?status=${status}`, {})
  }
}