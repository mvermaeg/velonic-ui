import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface WebsiteTheme {
  id: number;
  themeName: string;
  themeKey: string;
  primaryColor?: string;
  secondaryColor?: string;
  fontFamily?: string;
  logoUrl?: string;
  customCss?: string;
  isActive: boolean;
  createdOn: string;
}

@Injectable({
  providedIn: 'root'
})
export class WebsiteThemeService {
  private apiUrl = `${environment.apiUrl}/WebsiteThemes`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<WebsiteTheme[]> {
    return this.http.get<WebsiteTheme[]>(this.apiUrl);
  }
}