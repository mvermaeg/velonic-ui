import { Injectable, inject } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../environments/environment'

@Injectable({
  providedIn: 'root',
})
export class ReportsService {
  private http = inject(HttpClient)
  private apiUrl = environment.apiUrl

  getGeneral() {
    return this.http.get<any>(`${this.apiUrl}/reports/general`)
  }

  getCampaign() {
    return this.http.get<any[]>(`${this.apiUrl}/reports/campaign`)
  }

  getContracts() {
    return this.http.get<any[]>(`${this.apiUrl}/reports/contracts`)
  }

  getVendorDaily() {
    return this.http.get<any[]>(`${this.apiUrl}/reports/vendors/daily`)
  }

  getVendorSubId() {
    return this.http.get<any[]>(`${this.apiUrl}/reports/vendors/sub-id`)
  }

  getAffiliateDailyReport() {
    return this.http.get<any[]>(`${this.apiUrl}/reports/affiliates/daily-report`)
  }

  getAffiliateConversions() {
    return this.http.get<any[]>(`${this.apiUrl}/reports/affiliates/conversions`)
  }
}