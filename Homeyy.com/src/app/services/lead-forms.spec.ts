import { FormBuilder } from '@angular/forms';
import { Subject } from 'rxjs';
import { ServicesFormComponent } from '../pages/common/services-form/services-form.component';
import { ContactFormComponent } from '../pages/common/contact-form/contact-form.component';
import { QoutesComponent } from '../pages/lander/qoutes/qoutes.component';

describe('Public lead form payloads and duplicate protection', () => {
  for (const kind of ['service', 'contact', 'quote']) {
    for (const comments of ['  My roof leaks near the chimney. Please call after 5.  ', '   ']) {
    it(kind + (comments.trim() ? ' with customer notes' : ' without optional notes') + ' sends the API fields once while waiting for the certificate and request', async () => {
      let resolveCertificate!: (value: string | null) => void;
      const response = new Subject<any>();
      const api: any = {
        waitForTrustedFormCertificateUrl: jasmine.createSpy().and.returnValue(
          new Promise<string | null>(resolve => resolveCertificate = resolve)),
        createLead: jasmine.createSpy().and.returnValue(response)
      };
      const fb = new FormBuilder();
      const values = { firstName: 'Test', lastName: 'Homeowner', email: 'test@example.com',
        phone: '2025550123', address: '1 Test Street', zipCode: '02108', zip: '02108',
        homeOwner: 'yes', comments, purchaseTimeFrame: 'Immediately', bestTimeToCall: 'Morning' };
      let component: any;
      let submit: () => Promise<void>;
      if (kind === 'service') {
        component = new ServicesFormComponent(fb, {} as any, api, {} as any);
        component.quoteSteps = 'roofing'; component.form = fb.group(values);
        submit = () => component.saveLead();
      } else if (kind === 'contact') {
        component = new ContactFormComponent(fb, {} as any, api, {} as any);
        submit = () => component.saveContactLead({ Name: 'Test Homeowner', Email: values.email,
          Mobile: values.phone, Service: 'Roofing', Address: values.address, Zipcode: '02108', Msg: comments });
      } else {
        component = new QoutesComponent({} as any, {} as any, fb, {} as any, api);
        component.serviceCode = 'roofing'; component.service_name = 'Roofing';
        component.quoteForm = fb.group(values);
        submit = () => component.saveLead();
      }
      const first = submit(); await submit();
      expect(component.isSubmitting).toBeTrue();
      expect(api.waitForTrustedFormCertificateUrl).toHaveBeenCalledTimes(1);
      expect(api.createLead).not.toHaveBeenCalled();
      resolveCertificate(null); await first; await submit();
      expect(api.createLead).toHaveBeenCalledTimes(1);
      const body = api.createLead.calls.mostRecent().args[0];
      expect(body).toEqual(jasmine.objectContaining({ fullName: 'Test Homeowner', email: values.email,
        phone: values.phone, address: values.address, postcode: '02108', serviceCode: 'roofing',
        countryCode: 'US', isTest: false, isCompleted: true, isTcpaCompliant: true,
        trustedFormCertificateUrl: null, landingPageUrl: window.location.href }));
      expect(body.fingerprintHash).toBeTruthy();
      const details = JSON.parse(body.additionalDataJson);
      expect(details.formType).toBeTruthy();
      expect(details.comments).toBe(comments.trim() || null);
      expect(body.provider).toBeUndefined();
      expect(body.winner).toBeUndefined();
      response.error({ error: { message: 'Test failure' } });
      expect(component.isSubmitting).toBeFalse();
    });
    }
  }
});

describe('Optional customer comments on quote wizards', () => {
  it('landing wizard accepts empty comments and bounds the input length', () => {
    const component: any = new ServicesFormComponent(new FormBuilder(), {} as any, {} as any, {} as any);
    const control = component.form.get('comments');
    expect(control).toBeTruthy();
    expect(control.valid).toBeTrue();
    control.setValue('x'.repeat(2001));
    expect(control.hasError('maxlength')).toBeTrue();
  });
  it('service quote form accepts empty comments and bounds the input length', () => {
    const component: any = new QoutesComponent({} as any, {} as any, new FormBuilder(), {} as any, {} as any);
    component.buildForm();
    const control = component.quoteForm.get('comments');
    expect(control).toBeTruthy();
    expect(control.valid).toBeTrue();
    control.setValue('x'.repeat(2001));
    expect(control.hasError('maxlength')).toBeTrue();
  });
});
