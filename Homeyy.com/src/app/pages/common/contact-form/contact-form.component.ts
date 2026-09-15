import {
  Component,
  OnInit
} from '@angular/core';

import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import {
  CommonService
} from 'src/app/services/common.service';

import {
  WebsiteLeadService
} from 'src/app/services/website-lead.service';

@Component({
  selector: 'app-contact-form',
  templateUrl: './contact-form.component.html',
  styleUrls: ['./contact-form.component.scss'],
})
export class ContactFormComponent
  implements OnInit {

  contact_Form!: FormGroup;

  Submitted = false;
  isSubmitting = false;
  submitError = '';

  addressPostcodeError = '';
  addressPostcodeVerified = false;
  addressPostcodeChecking = false;
  expectedLocation: any = null;

  emailChecking = false;
  emailVerificationError = '';

  services = [
    'AC & Heat',
    'Bathroom',
    'Door',
    'Flooring',
    'Fencing',
    'Gutter',
    'Home Security',
    'Kitchen',
    'Plumbing',
    'Roofing',
    'Solar',
    'Siding',
    'Window'
  ];

  constructor(
    private FB: FormBuilder,
    public CF: CommonService,
    private websiteLeadService:
      WebsiteLeadService
  ) {}

  ngOnInit(): void {
    this.contact_Form =
      this.FB.group({
        Name: [
          '',
          Validators.required
        ],

        Mobile: [
          '',
          [
            Validators.required,
            Validators.pattern(
              /^\+?1?[\s.-]?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$/
            )
          ]
        ],

        Email: [
          '',
          [
            Validators.required,
            Validators.email
          ]
        ],

        Service: [
          '',
          Validators.required
        ],

        Address: [
          '',
          Validators.required
        ],

        Zipcode: [
          '',
          [
            Validators.required,
            Validators.pattern(
              /^\d{5}(-\d{4})?$/
            )
          ]
        ],

        Msg: ['']
      });
  }

  hasError(
    control: string,
    error: string
  ): boolean {
    return this.Submitted &&
      !!this.contact_Form
        .get(control)
        ?.hasError(error);
  }

  resetAddressPostcodeState(): void {
    this.addressPostcodeError = '';
    this.addressPostcodeVerified = false;
    this.addressPostcodeChecking = false;
    this.expectedLocation = null;
  }

  checkZipCode(): Promise<boolean> {
    this.addressPostcodeError = '';
    this.addressPostcodeVerified = false;
    this.expectedLocation = null;

    const postcode = String(this.contact_Form.get('Zipcode')?.value || '').trim();
    if (!/^\d{5}$/.test(postcode)) {
      this.addressPostcodeError = 'Please enter a valid 5-digit US ZIP code.';
      return Promise.resolve(false);
    }
    if (this.addressPostcodeChecking) return Promise.resolve(false);

    this.addressPostcodeChecking = true;
    return new Promise<boolean>((resolve) => {
      this.websiteLeadService.validateAddress(postcode, postcode).subscribe({
        next: (response) => {
          this.addressPostcodeChecking = false;
          const result = response?.result || response;
          const returnedPostcode = String(result?.postalCode || '').trim().substring(0, 5);
          const country = String(result?.country || '').trim().toLowerCase();
          const countryCode = String(result?.countryCode || result?.countryShortName || '').trim().toUpperCase();
          const providerAccepted =
            result?.isValid === true ||
            String(result?.status || '').toLowerCase() === 'valid' ||
            String(result?.possibleNextAction || '').toUpperCase() === 'ACCEPT';
          const isUS =
            countryCode === 'US' ||
            country === 'united states' ||
            country === 'united states of america' ||
            country === 'usa' ||
            country === 'us';

          if (providerAccepted && returnedPostcode === postcode && isUS) {
            this.addressPostcodeVerified = true;
            this.expectedLocation = result;
            resolve(true);
            return;
          }
          this.addressPostcodeError = 'Please enter a valid United States ZIP code.';
          resolve(false);
        },
        error: () => {
          this.addressPostcodeChecking = false;
          this.addressPostcodeVerified = false;
          this.expectedLocation = null;
          this.addressPostcodeError = 'We could not verify this ZIP code. Please try again.';
          resolve(false);
        }
      });
    });
  }

  validateEmailWithBouncer(
    email: string
  ): Promise<boolean> {

    this.emailVerificationError = '';
    this.emailChecking = true;

    return new Promise<boolean>(
      (resolve) => {

        this.websiteLeadService
          .validateEmail(email)
          .subscribe({
            next: (response) => {
              this.emailChecking = false;

              const result =
                response?.result ||
                response;

              const accepted =
                result?.deliverable === true &&
                String(
                  result?.status || ''
                ).toLowerCase() ===
                  'deliverable';

              if (accepted) {
                resolve(true);
                return;
              }

              this.emailVerificationError =
                `Email verification failed. ${
                  result?.reason ||
                  'Please enter a valid working email.'
                }`;

              resolve(false);
            },

            error: (error) => {
              this.emailChecking = false;

              this.emailVerificationError =
                error?.error?.message ||
                error?.error?.error ||
                'Email verification failed. Please enter a valid working email address.';

              resolve(false);
            }
          });
      }
    );
  }

  async Submit(): Promise<void> {
    this.Submitted = true;

    this.addressPostcodeError = '';
    this.submitError = '';
    this.emailVerificationError = '';

    if (this.contact_Form.invalid) {
      this.contact_Form.markAllAsTouched();
      return;
    }

    if (
      this.isSubmitting ||
      this.addressPostcodeChecking ||
      this.emailChecking
    ) {
      return;
    }

    const zipVerified = this.addressPostcodeVerified || await this.checkZipCode();

    if (!zipVerified) return;

    const value =
      this.contact_Form.getRawValue();

    const emailVerified =
      await this.validateEmailWithBouncer(
        value.Email.trim()
      );

    if (!emailVerified) {
      return;
    }

    this.saveContactLead(value);
  }

  private saveContactLead(
    value: any
  ): void {
    const serviceCode =
      this.toServiceCode(
        value.Service
      );

    this.isSubmitting = true;

    this.websiteLeadService
      .createLead({
        fullName:
          value.Name.trim(),

        email:
          value.Email.trim(),

        phone:
          value.Mobile.trim(),

        serviceCode:
          serviceCode,

        campaignName:
          value.Service,

        pageName:
          'Homeyy Contact Form',

        address:
          value.Address.trim(),

        postcode: value.Zipcode.trim(),

        city:
          this.expectedLocation
            ?.city ||
          undefined,

        state:
          this.expectedLocation
            ?.state ||
          undefined,

        country:
          this.expectedLocation
            ?.country ||
          'United States',

        countryCode: 'US',

        step: 1,
        isTest: false,
        isCompleted: true,

        fingerprintHash:
          this.generateFingerprint(),

        landingPageUrl:
          window.location.href,

        isTcpaCompliant: true,

        additionalDataJson:
          JSON.stringify({
            formType: 'Contact',
            service: value.Service,
            comments:
              value.Msg || null
          })
      })
      .subscribe({
        next: () => {
          this.isSubmitting = false;
          this.Submitted = false;

          this.CF.SwalSuccess(
            'Our representative will reach out to you soon.',
            'Thank you'
          );

          this.contact_Form.reset();
          this.resetAddressPostcodeState();
        },

        error: (error) => {
          this.isSubmitting = false;

          this.submitError =
            error?.error?.message ||
            error?.error?.emailReason ||
            'We could not submit your request. Please try again.';
        }
      });
  }

  private generateFingerprint():
    string {

    const data = [
      navigator.userAgent,
      navigator.language,
      screen.width,
      screen.height,
      screen.colorDepth,
      Intl.DateTimeFormat()
        .resolvedOptions()
        .timeZone
    ].join('|');

    let hash = 0;

    for (
      let i = 0;
      i < data.length;
      i++
    ) {
      hash =
        ((hash << 5) - hash) +
        data.charCodeAt(i);

      hash |= 0;
    }

    return Math.abs(hash).toString();
  }

  private toServiceCode(
    service: string
  ): string {

    const mapping: {
      [key: string]: string
    } = {
      'AC & Heat': 'hvac',
      'Bathroom': 'bathroom',
      'Door': 'door',
      'Flooring': 'flooring',
      'Fencing': 'fencing',
      'Gutter': 'gutter',
      'Home Security': 'homesecurity',
      'Kitchen': 'kitchen',
      'Plumbing': 'plumbing',
      'Roofing': 'roofing',
      'Solar': 'solar',
      'Siding': 'siding',
      'Window': 'window'
    };

    return mapping[service] || 'contact';
  }
}