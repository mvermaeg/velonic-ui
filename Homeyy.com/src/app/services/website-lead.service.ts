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
  trustedFormCertificateUrl?: string | null;
  jornayaLeadId?: string | null;
  additionalDataJson?: string | null;
}

export interface WebsiteLeadResponse {
  message: string;
  leadId: number;
  leadUuid: string;
}

export interface ThumbtackBusiness {
  businessId?: string | null;
  businessName?: string | null;
  businessLocation?: string | null;
  rating?: number | null;
  numberOfReviews?: number | null;
  isTopPro?: boolean | null;
  requestFlowUrl?: string | null;
  servicePageUrl?: string | null;
}

export interface ThumbtackBusinessResponse {
  success: boolean;
  available: boolean;
  message?: string | null;
  environment?: string | null;
  service?: string | null;
  categoryId?: string | null;
  zipCode?: string | null;
  trackingId?: string | null;
  searchId?: string | null;
  count?: number;
  businesses?: ThumbtackBusiness[];
  metadata?: any;
}

export interface PendingThumbtackLead {
  leadId: number;
  leadUuid: string;
  serviceCode: string;
}

@Injectable({ providedIn: 'root' })
export class WebsiteLeadService {
  private readonly apiUrl = environment.serviceUrl.replace(/\/$/, '');
  private readonly thumbtackStorageKey = 'homeyy.pendingThumbtackLead';

  constructor(private http: HttpClient) {}

  validateEmail(email: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/diagnostics/email`, { params: { email } });
  }

  validateAddress(address: string, postcode: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/diagnostics/address`, {
      address, postcode, countryCode: 'US'
    });
  }

  createLead(request: WebsiteLeadRequest): Observable<WebsiteLeadResponse> {
    return this.http.post<WebsiteLeadResponse>(
      `${this.apiUrl}/public/leads/website`, request
    );
  }

  getThumbtackBusinesses(
    leadId: number,
    leadUuid: string
  ): Observable<ThumbtackBusinessResponse> {
    return this.http.post<ThumbtackBusinessResponse>(
      `${this.apiUrl}/public/thumbtack/businesses`,
      { leadId, leadUuid }
    );
  }

  isThumbtackService(serviceCode: string): boolean {
    return ['roofing', 'window', 'bathroom', 'gutter']
      .includes(String(serviceCode || '').trim().toLowerCase());
  }

  rememberThumbtackLead(
    response: WebsiteLeadResponse,
    serviceCode: string
  ): void {
    if (!this.isThumbtackService(serviceCode)) {
      sessionStorage.removeItem(this.thumbtackStorageKey);
      return;
    }

    const value: PendingThumbtackLead = {
      leadId: response.leadId,
      leadUuid: response.leadUuid,
      serviceCode: String(serviceCode).trim().toLowerCase()
    };

    sessionStorage.setItem(
      this.thumbtackStorageKey,
      JSON.stringify(value)
    );
  }

  getPendingThumbtackLead(): PendingThumbtackLead | null {
    const raw = sessionStorage.getItem(this.thumbtackStorageKey);

    if (!raw) return null;

    try {
      const value = JSON.parse(raw) as PendingThumbtackLead;

      if (
        !Number.isFinite(value?.leadId) ||
        value.leadId <= 0 ||
        !value?.leadUuid ||
        !this.isThumbtackService(value.serviceCode)
      ) {
        return null;
      }

      return value;
    } catch {
      return null;
    }
  }

  getTrustedFormCertificateUrl(): string | null {
    const field = document.querySelector<HTMLInputElement>(
      'input[name="xxTrustedFormCertUrl"]'
    );
    const value = String(field?.value || '').trim();
    return value || null;
  }

  async waitForTrustedFormCertificateUrl(
    timeoutMs = 5000,
    intervalMs = 100
  ): Promise<string | null> {
    const started = Date.now();
    while (Date.now() - started < timeoutMs) {
      const url = this.getTrustedFormCertificateUrl();
      if (url) return url;
      await new Promise(resolve => setTimeout(resolve, intervalMs));
    }
    return this.getTrustedFormCertificateUrl();
  }
}


 

// import { Injectable } from '@angular/core';
// import { HttpClient } from '@angular/common/http';
// import { Observable } from 'rxjs';
// import { environment } from 'src/environments/environment';

// export interface WebsiteLeadRequest {
//   fullName: string;
//   email: string;
//   phone: string;
//   serviceCode: string;
//   campaignName?: string;
//   pageName?: string;
//   address: string;
//   postcode: string;
//   city?: string;
//   state?: string;
//   country: string;
//   countryCode: string;
//   step?: number;
//   isTest: boolean;
//   isCompleted: boolean;
//   affiliateSubId?: string | null;
//   fingerprintHash?: string | null;
//   landingPageUrl: string;
//   isTcpaCompliant: boolean;
//   ownsProperty?: boolean | null;
//   trustedFormCertificateUrl?: string | null;
//   jornayaLeadId?: string | null;
//   additionalDataJson?: string | null;
// }

// export interface WebsiteLeadResponse {
//   message: string;
//   leadId: number;
//   leadUuid: string;
// }

// @Injectable({ providedIn: 'root' })
// export class WebsiteLeadService {
//   private readonly apiUrl = environment.serviceUrl.replace(/\/$/, '');

//   constructor(private http: HttpClient) {}

//   validateEmail(email: string): Observable<any> {
//     return this.http.get<any>(`${this.apiUrl}/diagnostics/email`, { params: { email } });
//   }

//   validateAddress(address: string, postcode: string): Observable<any> {
//     return this.http.post<any>(`${this.apiUrl}/diagnostics/address`, {
//       address, postcode, countryCode: 'US'
//     });
//   }

//   createLead(request: WebsiteLeadRequest): Observable<WebsiteLeadResponse> {
//     return this.http.post<WebsiteLeadResponse>(
//       `${this.apiUrl}/public/leads/website`, request
//     );
//   }

//   getTrustedFormCertificateUrl(): string | null {
//     const field = document.querySelector<HTMLInputElement>(
//       'input[name="xxTrustedFormCertUrl"]'
//     );
//     const value = String(field?.value || '').trim();
//     return value || null;
//   }

//   async waitForTrustedFormCertificateUrl(
//     timeoutMs = 5000,
//     intervalMs = 100
//   ): Promise<string | null> {
//     const started = Date.now();
//     while (Date.now() - started < timeoutMs) {
//       const url = this.getTrustedFormCertificateUrl();
//       if (url) return url;
//       await new Promise(resolve => setTimeout(resolve, intervalMs));
//     }
//     return this.getTrustedFormCertificateUrl();
//   }
// }


 