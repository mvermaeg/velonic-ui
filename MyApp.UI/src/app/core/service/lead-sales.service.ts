import { Injectable, inject } from '@angular/core'
import { HttpClient, HttpParams } from '@angular/common/http'
import { environment } from '../../../environments/environment'

@Injectable({
  providedIn: 'root',
})
export class LeadSalesService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/lead-sales`
  private returnsApiUrl = `${environment.apiUrl}/lead-returns`

  getSales(filters: any) {
    let params = new HttpParams()

    Object.keys(filters).forEach((key) => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.set(key, filters[key])
      }
    })

    return this.http.get<any>(this.apiUrl, { params })
  }

  createReturn(payload: any) {
    return this.http.post<any>(this.returnsApiUrl, payload)
  }
}