import { Injectable, inject } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../environments/environment'

export interface Client {
  id: number
  clientName: string
  tier: string
  accountStatus: string
  timezone: string
  acceptsWebLeads: boolean
  acceptsInboundCalls: boolean
  isBuyer: boolean
  isVendor: boolean
  returnAgreement: string
  createdOn: string
}

@Injectable({ providedIn: 'root' })
export class ClientsService {
  private http = inject(HttpClient)
  private apiUrl = `${environment.apiUrl}/clients`

  getClients() {
    return this.http.get<any>(`${this.apiUrl}?page=1&pageSize=50`)
  }

  createClient(model: any) {
    return this.http.post<any>(this.apiUrl, model)
  }
}