import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { WebsiteLeadService, WebsiteLeadRequest } from './website-lead.service';
import { environment } from '../../environments/environment.prod';

describe('Public lead API contract', () => {
  let service: WebsiteLeadService;
  let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [HttpClientTestingModule] });
    service = TestBed.inject(WebsiteLeadService);
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());
  it('uses the live production API', () => {
    expect(environment.production).toBeTrue();
    expect(environment.serviceUrl).toBe('https://auth.homeyy.com/api');
  });
  it('posts contact and qualification data once to the API and preserves leading zero ZIPs', () => {
    const payload: WebsiteLeadRequest = {
      fullName: 'Test Homeowner', email: 'test@example.com', phone: '2025550123',
      serviceCode: 'roofing', address: '1 Test Street', postcode: '02108',
      country: 'United States', countryCode: 'US', isTest: false, isCompleted: true,
      landingPageUrl: 'https://homeyy.com/roofing', isTcpaCompliant: true,
      ownsProperty: true, additionalDataJson: '{"purchaseTimeFrame":"Immediately"}'
    };
    service.createLead(payload).subscribe(result => expect(result.leadId).toBe(42));
    const request = http.expectOne(environment.serviceUrl + '/public/leads/website');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush({ leadId: 42, leadUuid: 'test-id', message: 'Created' });
  });
  it('requests the backend routing result using only the saved lead identity', () => {
    service.getThumbtackBusinesses(42, 'test-id').subscribe();
    const request = http.expectOne(environment.serviceUrl + '/public/thumbtack/businesses');
    expect(request.request.body).toEqual({ leadId: 42, leadUuid: 'test-id' });
    request.flush({ available: false, routingPending: true });
  });
  it('uses API diagnostics without losing leading zeros', () => {
    service.validateAddress('1 Test Street', '02108').subscribe();
    const request = http.expectOne(environment.serviceUrl + '/diagnostics/address');
    expect(request.request.body).toEqual({ address: '1 Test Street', postcode: '02108', countryCode: 'US' });
    request.flush({});
    service.validateEmail('test@example.com').subscribe();
    const email = http.expectOne(r => r.url === environment.serviceUrl + '/diagnostics/email');
    expect(email.request.params.get('email')).toBe('test@example.com');
    email.flush({});
  });
});
