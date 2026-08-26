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

  checkAddressPostcode():
    Promise<boolean> {

    this.addressPostcodeError = '';
    this.addressPostcodeVerified = false;
    this.expectedLocation = null;

    const address =
      this.contact_Form
        .get('Address')
        ?.value?.trim();

    const postcode =
      this.contact_Form
        .get('Zipcode')
        ?.value?.trim();

    if (!address) {
      this.addressPostcodeError =
        'Full address is required.';

      return Promise.resolve(false);
    }

    if (!postcode) {
      this.addressPostcodeError =
        'ZIP code is required.';

      return Promise.resolve(false);
    }

    this.addressPostcodeChecking = true;

    return new Promise<boolean>(
      (resolve) => {

        this.websiteLeadService
          .validateAddress(
            address,
            postcode
          )
          .subscribe({
            next: (response) => {
              this.addressPostcodeChecking =
                false;

              const result =
                response?.result ||
                response;

              const accepted =
                result?.isValid === true ||
                result?.status === 'Valid' ||
                result?.possibleNextAction ===
                  'ACCEPT';

              if (accepted) {
                this.addressPostcodeVerified =
                  true;

                this.expectedLocation =
                  result;

                if (result?.postalCode) {
                  this.contact_Form
                    .get('Zipcode')
                    ?.setValue(
                      result.postalCode,
                      {
                        emitEvent: false
                      }
                    );
                }

                resolve(true);
                return;
              }

              this.addressPostcodeError =
                result?.message ||
                'Address verification failed.';

              resolve(false);
            },

            error: (error) => {
              this.addressPostcodeChecking =
                false;

              this.addressPostcodeVerified =
                false;

              this.addressPostcodeError =
                error?.error?.message ||
                error?.error?.result?.message ||
                'Address verification failed.';

              resolve(false);
            }
          });
      }
    );
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

    const addressVerified =
      this.addressPostcodeVerified ||
      await this.checkAddressPostcode();

    if (!addressVerified) {
      return;
    }

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

        postcode:
          this.expectedLocation
            ?.postalCode ||
          value.Zipcode.trim(),

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
            error?.error
              ?.addressCheck?.message ||
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


// import { Component, OnInit } from '@angular/core';
// import {
//   FormBuilder,
//   FormGroup,
//   Validators
// } from '@angular/forms';
// import { CommonService } from 'src/app/services/common.service';
// import {
//   WebsiteLeadService
// } from 'src/app/services/website-lead.service';

// @Component({
//   selector: 'app-contact-form',
//   templateUrl: './contact-form.component.html',
//   styleUrls: ['./contact-form.component.scss'],
// })
// export class ContactFormComponent implements OnInit {

//   contact_Form!: FormGroup;
//   Submitted = false;
//   isSubmitting = false;
//   submitError = '';
// isValidatingAddress = false;
// isValidatingEmail = false;

//   services = [
//     'AC & Heat',
//     'Bathroom',
//     'Door',
//     'Flooring',
//     'Fencing',
//     'Gutter',
//     'Home Security',
//     'Kitchen',
//     'Plumbing',
//     'Roofing',
//     'Solar',
//     'Siding',
//     'Window'
//   ];

//   constructor(
//     private FB: FormBuilder,
//     public CF: CommonService,
//     private websiteLeadService: WebsiteLeadService
//   ) {}

//   ngOnInit(): void {
//     this.contact_Form = this.FB.group({
//       Name: ['', Validators.required],

//       Mobile: [
//         '',
//         [
//           Validators.required,
//           Validators.pattern(
//             /^\+?1?[\s.-]?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$/
//           )
//         ]
//       ],

//       Email: [
//         '',
//         [
//           Validators.required,
//           Validators.email
//         ]
//       ],

//       Service: ['', Validators.required],
//       Address: ['', Validators.required],

//       Zipcode: [
//         '',
//         [
//           Validators.required,
//           Validators.pattern(/^\d{5}(-\d{4})?$/)
//         ]
//       ],

//       Msg: ['']
//     });
//   }

//   hasError(control: string, error: string): boolean {
//     return this.Submitted &&
//       !!this.contact_Form.get(control)?.hasError(error);
//   }

//  Submit(): void {
//   this.Submitted = true;
//   this.submitError = '';

//   if (this.contact_Form.invalid) {
//     this.contact_Form.markAllAsTouched();
//     return;
//   }

//   if (
//     this.isSubmitting ||
//     this.isValidatingAddress ||
//     this.isValidatingEmail
//   ) {
//     return;
//   }

//   const value =
//     this.contact_Form.getRawValue();

//   this.isValidatingAddress = true;

//   this.websiteLeadService.validateAddress(
//     value.Address.trim(),
//     value.Zipcode.trim()
//   ).subscribe({
//     next: (addressResult) => {
//       this.isValidatingAddress = false;
//       this.isValidatingEmail = true;

//       this.websiteLeadService
//         .validateEmail(
//           value.Email.trim()
//         )
//         .subscribe({
//           next: () => {
//             this.isValidatingEmail = false;

//             this.saveContactLead(
//               value,
//               addressResult
//             );
//           },

//           error: (error) => {
//             this.isValidatingEmail = false;

//             this.submitError =
//               error?.error?.message ||
//               'Email address could not be verified.';
//           }
//         });
//     },

//     error: (error) => {
//       this.isValidatingAddress = false;

//       this.submitError =
//         error?.error?.message ||
//         'Address and ZIP code could not be verified.';
//     }
//   });
// }

// private saveContactLead(
//   value: any,
//   addressResult: any
// ): void {
//   const serviceCode =
//     this.toServiceCode(value.Service);

//   this.isSubmitting = true;

//   this.websiteLeadService.createLead({
//     fullName: value.Name.trim(),
//     email: value.Email.trim(),
//     phone: value.Mobile.trim(),

//     serviceCode: serviceCode,
//     campaignName: value.Service,
//     pageName: 'Homeyy Contact Form',

//     address: value.Address.trim(),

//     postcode:
//       addressResult.postalCode ||
//       value.Zipcode.trim(),

//     city:
//       addressResult.city || undefined,

//     state:
//       addressResult.state || undefined,

//     country:
//       addressResult.country ||
//       'United States',

//     countryCode: 'US',

//     step: 1,
//     isTest: false,
//     isCompleted: true,

//     landingPageUrl:
//       window.location.href,

//     isTcpaCompliant: true,

//     additionalDataJson:
//       JSON.stringify({
//         formType: 'Contact',
//         service: value.Service,
//         comments:
//           value.Msg || null
//       })
//   }).subscribe({
//     next: () => {
//       this.isSubmitting = false;
//       this.Submitted = false;

//       this.CF.SwalSuccess(
//         'Our representative will reach out to you soon.',
//         'Thank you'
//       );

//       this.contact_Form.reset();
//     },

//     error: (error) => {
//       this.isSubmitting = false;

//       this.submitError =
//         error?.error?.message ||
//         'We could not submit your request. Please try again.';
//     }
//   });
// }


//   private toServiceCode(service: string): string {
//     const mapping: { [key: string]: string } = {
//       'AC & Heat': 'hvac',
//       'Bathroom': 'bathroom',
//       'Door': 'door',
//       'Flooring': 'flooring',
//       'Fencing': 'fencing',
//       'Gutter': 'gutter',
//       'Home Security': 'homesecurity',
//       'Kitchen': 'kitchen',
//       'Plumbing': 'plumbing',
//       'Roofing': 'roofing',
//       'Solar': 'solar',
//       'Siding': 'siding',
//       'Window': 'window'
//     };

//     return mapping[service] || 'contact';
//   }
// }

// // import { Component, OnInit } from '@angular/core';
// // import { FormBuilder, FormGroup, Validators } from '@angular/forms';
// // import { CommonService } from 'src/app/services/common.service';

// // @Component({
// //   selector: 'app-contact-form',
// //   templateUrl: './contact-form.component.html',
// //   styleUrls: ['./contact-form.component.scss'],
// // })
// // export class ContactFormComponent implements OnInit {

// //   contact_Form!: FormGroup;
// //   Submitted = false;

// //   services = [
// //     'AC & Heat',
// //     'Bathroom',
// //     'Door',
// //     'Flooring',
// //     'Fencing',
// //     'Gutter',
// //     'Home Security',
// //     'Kitchen',
// //     'Plumbing',
// //     'Roofing',
// //     'Solar',
// //     'Siding',
// //     'Window'
// //   ];

// //   constructor(
// //     private FB: FormBuilder,
// //     public CF: CommonService
// //   ) {}

// //   ngOnInit(): void {
// //     this.contact_Form = this.FB.group({
// //       Name: ['', [Validators.required]],
// //       Mobile: ['', [Validators.required]],
// //       Email: ['', [Validators.required, Validators.pattern(/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*([\.]\w{2,3})+$/)]],
// //       Service: ['', [Validators.required]],
// //       Address: ['', [Validators.required]],
// //       Zipcode: ['', [Validators.required]],
// //       Msg: ['']
// //     });
// //   }

// //   // Errors only surface once the user has tried to submit
// //   hasError(control: string, error: string): boolean {
// //     return this.Submitted && !!this.contact_Form.get(control)?.hasError(error);
// //   }

// //   Submit() {
// //     this.Submitted = true;

// //     if (this.contact_Form.valid) {
// //       this.Submitted = false;
// //       this.CF.SwalSuccess('Our representative will reach out to you soon.', 'Thank you');
// //       this.contact_Form.reset();
// //     }
// //   }

// // }
