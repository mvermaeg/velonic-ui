import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface WebsiteLeadRequest {
  fullName: string;
  email: string;
  phone: string;

  serviceCode: string;
  campaignName?: string;
  pageName?: string;

  address: string;
  postcode: string;
  city?: string;
  state?: string;

  country: string;
  countryCode: string;

  step?: number;
  isTest: boolean;
  isCompleted: boolean;

  affiliateSubId?: string | null;
  fingerprintHash?: string | null;

  landingPageUrl: string;
  isTcpaCompliant: boolean;
  ownsProperty?: boolean | null;
  additionalDataJson?: string | null;
}

export interface WebsiteLeadResponse {
  message: string;
  leadId: number;
  leadUuid: string;
}

@Injectable({
  providedIn: 'root'
})
export class WebsiteLeadService {

  private readonly apiUrl =
    environment.serviceUrl.replace(/\/$/, '');

  constructor(private http: HttpClient) {}

  validateEmail(email: string): Observable<any> {
    return this.http.get<any>(
      `${this.apiUrl}/diagnostics/email`,
      {
        params: {
          email: email
        }
      }
    );
  }

  validateAddress(
    address: string,
    postcode: string
  ): Observable<any> {
    return this.http.post<any>(
      `${this.apiUrl}/diagnostics/address`,
      {
        address: address,
        postcode: postcode,
        countryCode: 'US'
      }
    );
  }

  createLead(
    request: WebsiteLeadRequest
  ): Observable<WebsiteLeadResponse> {
    return this.http.post<WebsiteLeadResponse>(
      `${this.apiUrl}/public/leads/website`,
      request
    );
  }
}