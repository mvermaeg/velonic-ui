import { Injectable, inject } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../environments/environment'

@Injectable({
  providedIn: 'root',
})
export class LandingPagesService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/landingpages`

  getPages(siteId?: number) {
    const url = siteId ? `${this.apiUrl}?siteId=${siteId}` : this.apiUrl
    return this.http.get<any[]>(url)
  }
}