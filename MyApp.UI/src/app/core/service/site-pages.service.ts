import { Injectable, inject } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../environments/environment'

@Injectable({
  providedIn: 'root',
})
export class SitePagesService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/sitepages`

  getAll() {
    return this.http.get<any[]>(this.apiUrl)
  }

  getBySite(siteId: number) {
    return this.http.get<any[]>(`${this.apiUrl}/by-site/${siteId}`)
  }

  create(data: any) {
    return this.http.post(this.apiUrl, data)
  }

  update(id: number, data: any) {
    return this.http.put(`${this.apiUrl}/${id}`, data)
  }

  updateStatus(id: number, isActive: boolean) {
    return this.http.put(
      `${this.apiUrl}/${id}/status?isActive=${isActive}`,
      {}
    )
  }
}