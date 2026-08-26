import { Injectable } from '@angular/core';

declare let fbq: any;

@Injectable({
  providedIn: 'root'
})
export class MetaPixelService {

  pageView(): void {
    if (typeof fbq !== 'undefined') {
      fbq('track', 'PageView');
    }
  }

  track(eventName: string, eventData?: any): void {
    if (typeof fbq !== 'undefined') {
      fbq('track', eventName, eventData);
    }
  }
}