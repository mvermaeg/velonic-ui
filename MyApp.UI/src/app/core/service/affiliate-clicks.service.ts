import { Injectable, inject } from '@angular/core'
import { HttpClient, HttpParams } from '@angular/common/http'
import { environment } from '../../../environments/environment'

@Injectable({ providedIn: 'root' })
export class AffiliateClicksService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/affiliate-clicks`

  getClicks(filters: any) {
    let params = new HttpParams()

    Object.keys(filters).forEach((key) => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.set(key, filters[key])
      }
    })

    return this.http.get<any>(this.apiUrl, { params })
  }
}