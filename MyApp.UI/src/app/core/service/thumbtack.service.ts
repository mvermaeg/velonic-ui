import { Injectable, inject } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../environments/environment'

@Injectable({
  providedIn: 'root',
})
export class ThumbtackService {
  private http = inject(HttpClient)

  createSession(leadId: number) {
    return this.http.post<any>(
      `${environment.serviceUrl}thumbtack/session/${leadId}`,
      {}
    )
  }

  markOpened(sessionId: number) {
    return this.http.post(
      `${environment.serviceUrl}thumbtack/session/${sessionId}/opened`,
      {}
    )
  }

  markPending(sessionId: number) {
    return this.http.post(
      `${environment.serviceUrl}thumbtack/session/${sessionId}/pending`,
      {}
    )
  }
}