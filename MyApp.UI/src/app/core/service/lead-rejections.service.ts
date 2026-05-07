import { Injectable, inject } from '@angular/core'
import { HttpClient, HttpParams } from '@angular/common/http'
import { environment } from '../../../environments/environment'

@Injectable({ providedIn: 'root' })
export class LeadRejectionsService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/lead-rejections`

  getRejections(filters: any) {
    let params = new HttpParams()

    Object.keys(filters).forEach((key) => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.set(key, filters[key])
      }
    })

    return this.http.get<any>(this.apiUrl, { params })
  }

  createRejection(payload: any) {
    return this.http.post<any>(this.apiUrl, payload)
  }
}