import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class BiddingService {

  private baseUrl = '/api/client-bidding';

  constructor(private http: HttpClient) {}

  getResults(filters: any) {
    let params = new HttpParams();

    Object.keys(filters).forEach(key => {
      if (filters[key] !== null && filters[key] !== '') {
        params = params.set(key, filters[key]);
      }
    });

    return this.http.get<any>(`${this.baseUrl}/results`, { params });
  }

  deliverResult(id: number) {
  return this.http.post<any>(`${this.baseUrl}/results/${id}/deliver`, {})
}
}