import { Injectable } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { Observable } from 'rxjs'
import { environment } from '../../../environments/environment'

export interface WebsiteTemplate {
  id: number
  templateName: string
  templateKey: string
  previewImage?: string
  sourceFolder?: string
  buildFolder?: string
  version?: string
  description?: string
  isActive: boolean
  createdOn: string
}

@Injectable({
  providedIn: 'root',
})
export class WebsiteTemplateService {
  private apiUrl = `${environment.apiUrl}/WebsiteTemplates`

  constructor(private http: HttpClient) {}

  getAll(): Observable<WebsiteTemplate[]> {
    return this.http.get<WebsiteTemplate[]>(this.apiUrl)
  }
}