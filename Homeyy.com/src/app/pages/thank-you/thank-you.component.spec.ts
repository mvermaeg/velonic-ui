import { fakeAsync, tick } from '@angular/core/testing';
import { of, NEVER, throwError } from 'rxjs';
import { ThankYouComponent } from './thank-you.component';

describe('Thank-you routing observation', () => {
  let component: ThankYouComponent;
  let service: any;
  beforeEach(() => {
    service = {
      getPendingThumbtackLead: () => ({ leadId: 42, leadUuid: 'test-id', serviceCode: 'roofing' }),
      getThumbtackBusinesses: jasmine.createSpy().and.returnValue(of({ available: false, routingPending: true }))
    };
    component = new ThankYouComponent({ snapshot: { paramMap: { get: () => 'roofing' } } } as any,
      { qoutes: [] } as any, service);
  });
  afterEach(() => component.ngOnDestroy());
  it('waits for the API decision, displays returned businesses, then stops', fakeAsync(() => {
    component.ngOnInit(); tick(0);
    expect(component.thumbtackLoading).toBeTrue();
    service.getThumbtackBusinesses.and.returnValue(of({ available: true, routingPending: false,
      businesses: [{ businessName: 'Test', requestFlowUrl: 'https://example.com/request' }, { businessName: 'No link' }] }));
    tick(3000);
    expect(component.thumbtackBusinesses.length).toBe(1);
    expect(component.thumbtackLoading).toBeFalse();
    tick(9000);
    expect(service.getThumbtackBusinesses).toHaveBeenCalledTimes(2);
    expect(service.getThumbtackBusinesses).toHaveBeenCalledWith(42, 'test-id');
  }));
  it('stops on a final unavailable result', fakeAsync(() => {
    service.getThumbtackBusinesses.and.returnValue(of({ available: false, routingPending: false, message: 'Received' }));
    component.ngOnInit(); tick(9000);
    expect(service.getThumbtackBusinesses).toHaveBeenCalledTimes(1);
    expect(component.thumbtackMessage).toContain('No additional Thumbtack professionals');
  }));
  it('bounds pending observation to two minutes', fakeAsync(() => {
    component.ngOnInit(); tick(120000);
    const count = service.getThumbtackBusinesses.calls.count(); tick(9000);
    expect(service.getThumbtackBusinesses.calls.count()).toBe(count);
    expect(component.thumbtackLoading).toBeFalse();
    expect(component.thumbtackMessage).toContain('submitted successfully');
  }));
  it('shows no results when the API returns no usable request links', fakeAsync(() => {
    service.getThumbtackBusinesses.and.returnValue(of({ available: true, routingPending: false,
      businesses: [{ businessName: 'No request URL' }] }));
    component.ngOnInit(); tick(0);
    expect(component.thumbtackBusinesses).toEqual([]);
    expect(component.thumbtackMessage).toContain('No additional Thumbtack professionals');
  }));
  it('does not look up a stale lead belonging to a different service', fakeAsync(() => {
    service.getPendingThumbtackLead = () => ({ leadId: 42, leadUuid: 'test-id', serviceCode: 'window' });
    component.ngOnInit(); tick(9000);
    expect(service.getThumbtackBusinesses).not.toHaveBeenCalled();
  }));
  it('cancels polling when the page is destroyed', fakeAsync(() => {
    component.ngOnInit(); tick(0); component.ngOnDestroy(); tick(9000);
    expect(service.getThumbtackBusinesses).toHaveBeenCalledTimes(1);
  }));
  it('does not overlap slow requests and times out without resubmitting the lead', fakeAsync(() => {
    service.getThumbtackBusinesses.and.returnValue(NEVER);
    component.ngOnInit(); tick(10000);
    expect(service.getThumbtackBusinesses).toHaveBeenCalledTimes(1);
    expect(component.thumbtackLoading).toBeFalse();
  }));
  it('shows a saved Thumbtack failure without submitting or rerouting the lead', fakeAsync(() => {
    service.getThumbtackBusinesses.and.returnValue(of({ success: false, available: false, routingPending: false }));
    component.ngOnInit(); tick(9000);
    expect(service.getThumbtackBusinesses).toHaveBeenCalledTimes(1);
    expect(component.thumbtackMessage).toContain('temporarily unavailable');
    expect(component.thumbtackLoading).toBeFalse();
  }));
  it('keeps submission success visible when result lookup fails', fakeAsync(() => {
    service.getThumbtackBusinesses.and.returnValue(throwError(() => new Error('offline')));
    component.ngOnInit(); tick(0);
    expect(component.thumbtackMessage).toContain('submitted successfully');
  }));
});
