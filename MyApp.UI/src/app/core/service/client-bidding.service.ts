import { Injectable, inject } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../environments/environment'

@Injectable({ providedIn: 'root' })
export class ClientBiddingService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/client-bidding`

  getByClient(clientId: number) {
    return this.http.get<any[]>(`${this.apiUrl}/${clientId}`)
  }

  getAllSettings(page = 1, pageSize = 20) {
  return this.http.get<any>(
    `${environment.apiUrl}/client-bidding/settings?page=${page}&pageSize=${pageSize}`
  )
}

  create(model: any) {
    return this.http.post<any>(this.apiUrl, model)
  }

  updateStatus(id: number, isActive: boolean) {
    return this.http.put<any>(`${this.apiUrl}/${id}/status?isActive=${isActive}`, {})
  }
}