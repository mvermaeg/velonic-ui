import {
  Component,
  Input,
  OnInit
} from '@angular/core';

import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import {
  FormsService
} from 'src/app/services/forms.service';

import {
  WebsiteLeadService
} from 'src/app/services/website-lead.service';
import { Router } from '@angular/router';

interface FormField {
  label: string;
  key: string;
  placeholder: string;
  inputType: string;
}

interface Step {
  title: string;
  subtitle?: string;
  type: string;
  field?: string;
  optionField?: string;
  options?: string[];
  fields?: FormField[];
}

@Component({
  selector: 'app-services-form',
  templateUrl: './services-form.component.html',
  styleUrls: ['./services-form.component.scss']
})
export class ServicesFormComponent
  implements OnInit {

  @Input() quoteSteps = '';

  currentStep = 0;
  steps: Step[] = [];
  form: FormGroup;

  submittedSteps: {
    [key: number]: boolean
  } = {};

  isSubmitting = false;
  submitError = '';
zipCodeChecking = false;
zipCodeError = '';
zipCodeVerified = false;
  verifiedZipLocation: any = null;

  emailChecking = false;
  emailVerificationError = '';

  get activeStep(): Step | undefined {
    return this.steps[this.currentStep];
  }

  links = [
    {
      path: 'hvac',
      title: 'HVAC Systems'
    },
    {
      path: 'bathroom',
      title: 'Bathroom Remodeling'
    },
    {
      path: 'kitchen',
      title: 'Kitchen Remodeling'
    },
    {
      path: 'plumbing',
      title: 'Plumbing Services'
    },
    {
      path: 'window',
      title: 'Window Installation'
    },
    {
      path: 'door',
      title: 'Door Installation'
    },
    {
      path: 'flooring',
      title: 'Flooring Services'
    },
    {
      path: 'gutter',
      title: 'Gutter Installation'
    },
    {
      path: 'fencing',
      title: 'Fencing Installation'
    },
    {
      path: 'solar',
      title: 'Solar Installation'
    },
    {
      path: 'roofing',
      title: 'Roofing Installation'
    },
    {
      path: 'siding',
      title: 'Siding Installation'
    },
    {
      path: 'homesecurity',
      title: 'Home Security Systems'
    }
  ];

  constructor(
    private fb: FormBuilder,
    public formService: FormsService,
    private websiteLeadService:
      WebsiteLeadService,
    private router: Router
  ) {
    this.form = this.fb.group({
      zipCode: [
        '',
        [
          Validators.required,
          Validators.pattern(
            /^\d{5}(-\d{4})?$/
          )
        ]
      ],

      firstName: [
        '',
        Validators.required
      ],

      lastName: [
        '',
        Validators.required
      ],

      address: [
        '',
        Validators.required
      ],

      purchaseTimeFrame: [''],

      phone: [
        '',
        [
          Validators.required,
          Validators.pattern(
            /^\+?1?[\s.-]?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$/
          )
        ]
      ],

      bestTimeToCall: [
        '',
        Validators.required
      ],

      email: [
        '',
        [
          Validators.required,
          Validators.email
        ]
      ]
    });
  }

  ngOnInit(): void {
    this.loadSteps();
  }

  loadSteps(): void {
    const dynamicSteps =
      this.getDynamicSteps(
        this.quoteSteps
      );

    const service =
      this.links.find(
        link =>
          link.path === this.quoteSteps
      ) ?? {
        path: 'roofing',
        title: 'Roofing Installation'
      };

    this.steps = [
      {
        title:
          `Get Free Quotes on New, Affordable ${service.title}`,

        subtitle:
          `Enter your details below to compare ${service.title} prices in your city.`,

        type: 'intro'
      },

      ...dynamicSteps,

      {
        title: 'What is your name?',
        type: 'inputs',

        fields: [
          {
            label: 'First Name',
            key: 'firstName',
            placeholder: 'First Name',
            inputType: 'text'
          },
          {
            label: 'Last Name',
            key: 'lastName',
            placeholder: 'Last Name',
            inputType: 'text'
          }
        ]
      },

      {
        title:
          'Where will this project take place?',

        type: 'location',

        optionField:
          'purchaseTimeFrame',

        options: [
          'Immediately',
          'Within 1 month',
          '1-3 months',
          '3+ months'
        ],

        fields: [
          {
            label: 'Address',
            key: 'address',
            placeholder:
              '123 Main Street',
            inputType: 'text'
          }
        ]
      },

      {
        title:
          'Please enter your phone number and email',

        type: 'contact',

        options: [
          'Morning',
          'Afternoon',
          'Evening',
          'Anytime'
        ]
      },

      {
        title:
          'Thank you for your application!',

        type: 'success'
      }
    ];

    this.ensureDynamicControls(
      dynamicSteps
    );
  }

  getDynamicSteps(
    service: string
  ): Step[] {
    const configs: {
      [key: string]: Step[]
    } = {
      hvac:
        this.formService.hvac,

      bathroom:
        this.formService.bathroom,

      kitchen:
        this.formService.kitchen,

      plumbing:
        this.formService.plumbing,

      window:
        this.formService.window,

      door:
        this.formService.door,

      flooring:
        this.formService.flooring,

      gutter:
        this.formService.gutter,

      fencing:
        this.formService.fencing,

      solar:
        this.formService.solar,

      roofing:
        this.formService.roofing,

      siding:
        this.formService.siding,

      homesecurity:
        this.formService.homesecurity
    };

    return configs[service] ||
      configs['roofing'];
  }

  ensureDynamicControls(
    steps: Step[]
  ): void {
    steps.forEach(step => {
      if (
        step.type === 'buttons' &&
        step.field &&
        !this.form.contains(step.field)
      ) {
        this.form.addControl(
          step.field,

          this.fb.control(
            '',
            Validators.required
          )
        );
      }
    });
  }

 async next(): Promise<void> {
  this.submittedSteps[this.currentStep] = true;

  const step = this.steps[this.currentStep];

  if (!step || !this.isStepValid(step)) {
    return;
  }

  if (step.type === 'intro') {
    const zipVerified =
      await this.checkInitialZipCode();

    if (!zipVerified) {
      return;
    }
  }
if (
    this.currentStep <
    this.steps.length - 1
  ) {
    this.currentStep++;
  }
}


checkInitialZipCode(): Promise<boolean> {
  this.zipCodeError = '';
  this.zipCodeVerified = false;
  this.verifiedZipLocation = null;

  const zipControl =
    this.form.get('zipCode');

  zipControl?.markAsTouched();
  zipControl?.updateValueAndValidity();

  if (!zipControl || zipControl.invalid) {
    return Promise.resolve(false);
  }

  const postcode =
    String(zipControl.value || '').trim();

  if (this.zipCodeChecking) {
    return Promise.resolve(false);
  }

  this.zipCodeChecking = true;

  /*
   * The existing Google address diagnostic requires
   * address and postcode. Passing the ZIP in both fields
   * asks Google to resolve the ZIP as a US location.
   */
  return new Promise<boolean>((resolve) => {
    this.websiteLeadService
      .validateAddress(
        postcode,
        postcode
      )
      .subscribe({
        next: (response) => {
          this.zipCodeChecking = false;

          const result =
            response?.result || response;

          const returnedPostcode =
            String(
              result?.postalCode || ''
            ).trim();

          const accepted =
            (
              result?.isValid === true ||
              String(
                result?.status || ''
              ).toLowerCase() === 'valid' ||
              String(
                result?.possibleNextAction || ''
              ).toUpperCase() === 'ACCEPT'
            ) &&
            returnedPostcode === postcode;

          if (accepted) {
            this.zipCodeVerified = true;
            this.verifiedZipLocation = result;
            this.zipCodeError = '';
            resolve(true);
            return;
          }

          this.zipCodeVerified = false;

          this.zipCodeError =
            'Please enter a valid US ZIP code.';

          resolve(false);
        },

        error: () => {
          this.zipCodeChecking = false;
          this.zipCodeVerified = false;

          this.zipCodeError =
            'We could not verify this ZIP code. Please try again.';

          resolve(false);
        }
      });
  });
}

  previous(): void {
    if (this.currentStep > 0) {
      this.currentStep--;
    }
  }

  setValue(
    field: string,
    value: string
  ): void {
    const control =
      this.form.get(field);

    if (!control) {
      return;
    }

    control.setValue(value);
    control.markAsTouched();
    control.markAsDirty();
  }

  selectOption(
    field: string | undefined,
    value: string
  ): void {
    if (!field) {
      return;
    }

    this.setValue(
      field,
      value
    );

    this.next();
  }
  resetZipValidationState(): void {
    this.zipCodeError = '';
    this.zipCodeVerified = false;
    this.zipCodeChecking = false;
    this.verifiedZipLocation = null;
  }

  validateEmailWithBouncer(
    email: string
  ): Promise<boolean> {

    this.emailVerificationError = '';
    this.emailChecking = true;

    return new Promise<boolean>(
      resolve => {

        this.websiteLeadService
          .validateEmail(email)
          .subscribe({
            next: response => {
              this.emailChecking = false;

              const result =
                response?.result ||
                response;

              const accepted =
                result?.deliverable ===
                  true &&
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

            error: error => {
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

  async submitForm():
    Promise<void> {

    this.submittedSteps[
      this.currentStep
    ] = true;

    this.submitError = '';
    this.emailVerificationError = '';

    const step =
      this.steps[this.currentStep];

    if (
      !step ||
      !this.isStepValid(step)
    ) {
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (
      this.isSubmitting ||
      this.emailChecking ||
      this.zipCodeChecking
    ) {
      return;
    }
    if (!this.zipCodeVerified) {
      const zipVerified = await this.checkInitialZipCode();
      if (!zipVerified) return;
    }

    const value =
      this.form.getRawValue();

    const emailVerified =
      await this
        .validateEmailWithBouncer(
          value.email.trim()
        );

    if (!emailVerified) {
      return;
    }

    await this.saveLead();
  }

  private async saveLead(): Promise<void> {
    const value =
      this.form.getRawValue();

    const service =
      this.links.find(
        item =>
          item.path ===
          this.quoteSteps
      ) ?? {
        path: 'roofing',
        title:
          'Roofing Installation'
      };

    this.isSubmitting = true;

    const trustedFormCertificateUrl =
      await this.websiteLeadService.waitForTrustedFormCertificateUrl();

    this.websiteLeadService
      .createLead({
        fullName:
          `${value.firstName} ${value.lastName}`
            .trim(),

        email:
          value.email.trim(),

        phone:
          value.phone.trim(),

        serviceCode:
          service.path,

        campaignName:
          service.title,

        pageName:
          `Homeyy ${service.title} Form`,

        address:
          value.address.trim(),

        postcode: value.zipCode.trim(),

        city:
          this.verifiedZipLocation
            ?.city ||
          undefined,

        state:
          this.verifiedZipLocation
            ?.state ||
          undefined,

        country:
          this.verifiedZipLocation
            ?.country ||
          'United States',

        countryCode: 'US',

        step:
          this.currentStep + 1,

        isTest: false,
        isCompleted: true,

        fingerprintHash:
          this.generateFingerprint(),

        landingPageUrl:
          window.location.href,

        isTcpaCompliant: true,

        trustedFormCertificateUrl,

        ownsProperty:
          String(value.homeOwner || '')
            .trim()
            .toLowerCase() === 'yes',

        additionalDataJson:
          JSON.stringify({
            formType:
              'ServiceQuote',

            serviceCode:
              service.path,

            serviceName:
              service.title,

            purchaseTimeFrame:
              value
                .purchaseTimeFrame ||
              null,

            bestTimeToCall:
              value.bestTimeToCall ||
              null,

            answers: value
          })
      })
      .subscribe({
        next: response => {
          this.isSubmitting = false;

          if (this.websiteLeadService.isThumbtackService(service.path)) {
            this.websiteLeadService.rememberThumbtackLead(
              response,
              service.path
            );

            this.router.navigate([
              '/',
              service.path,
              'thank-you'
            ]);

            return;
          }

          if (
            this.currentStep <
            this.steps.length - 1
          ) {
            this.currentStep++;
          }
        },

        error: error => {
          this.isSubmitting = false;

          this.submitError =
            error?.error?.message ||
            error?.error
              ?.emailReason ||
            error?.error
              ?.addressCheck?.message ||
            'We could not submit your request. Please try again.';
        }
      });
  }

  private stepControls(
    step: Step
  ): string[] {
    switch (step.type) {
      case 'intro':
        return [
          'zipCode'
        ];

      case 'buttons':
        return step.field
          ? [step.field]
          : [];

      case 'inputs':
        return (
          step.fields || []
        ).map(
          field => field.key
        );

      case 'location':
        return (
          step.fields || []
        ).map(
          field => field.key
        );

      case 'contact':
        return [
          'phone',
          'bestTimeToCall',
          'email'
        ];

      default:
        return [];
    }
  }

  private isStepValid(
    step: Step
  ): boolean {
    return this
      .stepControls(step)
      .every(controlName => {
        const control =
          this.form.get(
            controlName
          );

        if (!control) {
          return true;
        }

        control.markAsTouched();

        control
          .updateValueAndValidity({
            onlySelf: true
          });

        return control.valid;
      });
  }

  isFieldInvalid(
    controlName: string
  ): boolean {
    const control =
      this.form.get(controlName);

    return !!control &&
      control.invalid &&
      (
        control.touched ||
        control.dirty ||
        this.submittedSteps[
          this.currentStep
        ]
      );
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
}



// import {
//   Component,
//   Input,
//   OnInit
// } from '@angular/core';

// import {
//   FormBuilder,
//   FormGroup,
//   Validators
// } from '@angular/forms';

// import {
//   FormsService
// } from 'src/app/services/forms.service';

// import {
//   WebsiteLeadService
// } from 'src/app/services/website-lead.service';

// interface FormField {
//   label: string;
//   key: string;
//   placeholder: string;
//   inputType: string;
// }

// interface Step {
//   title: string;
//   subtitle?: string;
//   type: string;
//   field?: string;
//   optionField?: string;
//   options?: string[];
//   fields?: FormField[];
// }

// @Component({
//   selector: 'app-services-form',
//   templateUrl: './services-form.component.html',
//   styleUrls: ['./services-form.component.scss']
// })
// export class ServicesFormComponent
//   implements OnInit {

//   @Input() quoteSteps = '';

//   currentStep = 0;
//   steps: Step[] = [];
//   form: FormGroup;

//   submittedSteps: {
//     [key: number]: boolean
//   } = {};

//   isSubmitting = false;
//   submitError = '';
// zipCodeChecking = false;
// zipCodeError = '';
// zipCodeVerified = false;
//   verifiedZipLocation: any = null;

//   emailChecking = false;
//   emailVerificationError = '';

//   get activeStep(): Step | undefined {
//     return this.steps[this.currentStep];
//   }

//   links = [
//     {
//       path: 'hvac',
//       title: 'HVAC Systems'
//     },
//     {
//       path: 'bathroom',
//       title: 'Bathroom Remodeling'
//     },
//     {
//       path: 'kitchen',
//       title: 'Kitchen Remodeling'
//     },
//     {
//       path: 'plumbing',
//       title: 'Plumbing Services'
//     },
//     {
//       path: 'window',
//       title: 'Window Installation'
//     },
//     {
//       path: 'door',
//       title: 'Door Installation'
//     },
//     {
//       path: 'flooring',
//       title: 'Flooring Services'
//     },
//     {
//       path: 'gutter',
//       title: 'Gutter Installation'
//     },
//     {
//       path: 'fencing',
//       title: 'Fencing Installation'
//     },
//     {
//       path: 'solar',
//       title: 'Solar Installation'
//     },
//     {
//       path: 'roofing',
//       title: 'Roofing Installation'
//     },
//     {
//       path: 'siding',
//       title: 'Siding Installation'
//     },
//     {
//       path: 'homesecurity',
//       title: 'Home Security Systems'
//     }
//   ];

//   constructor(
//     private fb: FormBuilder,
//     public formService: FormsService,
//     private websiteLeadService:
//       WebsiteLeadService
//   ) {
//     this.form = this.fb.group({
//       zipCode: [
//         '',
//         [
//           Validators.required,
//           Validators.pattern(
//             /^\d{5}(-\d{4})?$/
//           )
//         ]
//       ],

//       firstName: [
//         '',
//         Validators.required
//       ],

//       lastName: [
//         '',
//         Validators.required
//       ],

//       address: [
//         '',
//         Validators.required
//       ],

//       purchaseTimeFrame: [''],

//       phone: [
//         '',
//         [
//           Validators.required,
//           Validators.pattern(
//             /^\+?1?[\s.-]?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$/
//           )
//         ]
//       ],

//       bestTimeToCall: [
//         '',
//         Validators.required
//       ],

//       email: [
//         '',
//         [
//           Validators.required,
//           Validators.email
//         ]
//       ]
//     });
//   }

//   ngOnInit(): void {
//     this.loadSteps();
//   }

//   loadSteps(): void {
//     const dynamicSteps =
//       this.getDynamicSteps(
//         this.quoteSteps
//       );

//     const service =
//       this.links.find(
//         link =>
//           link.path === this.quoteSteps
//       ) ?? {
//         path: 'roofing',
//         title: 'Roofing Installation'
//       };

//     this.steps = [
//       {
//         title:
//           `Get Free Quotes on New, Affordable ${service.title}`,

//         subtitle:
//           `Enter your details below to compare ${service.title} prices in your city.`,

//         type: 'intro'
//       },

//       ...dynamicSteps,

//       {
//         title: 'What is your name?',
//         type: 'inputs',

//         fields: [
//           {
//             label: 'First Name',
//             key: 'firstName',
//             placeholder: 'First Name',
//             inputType: 'text'
//           },
//           {
//             label: 'Last Name',
//             key: 'lastName',
//             placeholder: 'Last Name',
//             inputType: 'text'
//           }
//         ]
//       },

//       {
//         title:
//           'Where will this project take place?',

//         type: 'location',

//         optionField:
//           'purchaseTimeFrame',

//         options: [
//           'Immediately',
//           'Within 1 month',
//           '1-3 months',
//           '3+ months'
//         ],

//         fields: [
//           {
//             label: 'Address',
//             key: 'address',
//             placeholder:
//               '123 Main Street',
//             inputType: 'text'
//           }
//         ]
//       },

//       {
//         title:
//           'Please enter your phone number and email',

//         type: 'contact',

//         options: [
//           'Morning',
//           'Afternoon',
//           'Evening',
//           'Anytime'
//         ]
//       },

//       {
//         title:
//           'Thank you for your application!',

//         type: 'success'
//       }
//     ];

//     this.ensureDynamicControls(
//       dynamicSteps
//     );
//   }

//   getDynamicSteps(
//     service: string
//   ): Step[] {
//     const configs: {
//       [key: string]: Step[]
//     } = {
//       hvac:
//         this.formService.hvac,

//       bathroom:
//         this.formService.bathroom,

//       kitchen:
//         this.formService.kitchen,

//       plumbing:
//         this.formService.plumbing,

//       window:
//         this.formService.window,

//       door:
//         this.formService.door,

//       flooring:
//         this.formService.flooring,

//       gutter:
//         this.formService.gutter,

//       fencing:
//         this.formService.fencing,

//       solar:
//         this.formService.solar,

//       roofing:
//         this.formService.roofing,

//       siding:
//         this.formService.siding,

//       homesecurity:
//         this.formService.homesecurity
//     };

//     return configs[service] ||
//       configs['roofing'];
//   }

//   ensureDynamicControls(
//     steps: Step[]
//   ): void {
//     steps.forEach(step => {
//       if (
//         step.type === 'buttons' &&
//         step.field &&
//         !this.form.contains(step.field)
//       ) {
//         this.form.addControl(
//           step.field,

//           this.fb.control(
//             '',
//             Validators.required
//           )
//         );
//       }
//     });
//   }

//  async next(): Promise<void> {
//   this.submittedSteps[this.currentStep] = true;

//   const step = this.steps[this.currentStep];

//   if (!step || !this.isStepValid(step)) {
//     return;
//   }

//   if (step.type === 'intro') {
//     const zipVerified =
//       await this.checkInitialZipCode();

//     if (!zipVerified) {
//       return;
//     }
//   }
// if (
//     this.currentStep <
//     this.steps.length - 1
//   ) {
//     this.currentStep++;
//   }
// }


// checkInitialZipCode(): Promise<boolean> {
//   this.zipCodeError = '';
//   this.zipCodeVerified = false;
//   this.verifiedZipLocation = null;

//   const zipControl =
//     this.form.get('zipCode');

//   zipControl?.markAsTouched();
//   zipControl?.updateValueAndValidity();

//   if (!zipControl || zipControl.invalid) {
//     return Promise.resolve(false);
//   }

//   const postcode =
//     String(zipControl.value || '').trim();

//   if (this.zipCodeChecking) {
//     return Promise.resolve(false);
//   }

//   this.zipCodeChecking = true;

//   /*
//    * The existing Google address diagnostic requires
//    * address and postcode. Passing the ZIP in both fields
//    * asks Google to resolve the ZIP as a US location.
//    */
//   return new Promise<boolean>((resolve) => {
//     this.websiteLeadService
//       .validateAddress(
//         postcode,
//         postcode
//       )
//       .subscribe({
//         next: (response) => {
//           this.zipCodeChecking = false;

//           const result =
//             response?.result || response;

//           const returnedPostcode =
//             String(
//               result?.postalCode || ''
//             ).trim();

//           const accepted =
//             (
//               result?.isValid === true ||
//               String(
//                 result?.status || ''
//               ).toLowerCase() === 'valid' ||
//               String(
//                 result?.possibleNextAction || ''
//               ).toUpperCase() === 'ACCEPT'
//             ) &&
//             returnedPostcode === postcode;

//           if (accepted) {
//             this.zipCodeVerified = true;
//             this.verifiedZipLocation = result;
//             this.zipCodeError = '';
//             resolve(true);
//             return;
//           }

//           this.zipCodeVerified = false;

//           this.zipCodeError =
//             'Please enter a valid US ZIP code.';

//           resolve(false);
//         },

//         error: () => {
//           this.zipCodeChecking = false;
//           this.zipCodeVerified = false;

//           this.zipCodeError =
//             'We could not verify this ZIP code. Please try again.';

//           resolve(false);
//         }
//       });
//   });
// }

//   previous(): void {
//     if (this.currentStep > 0) {
//       this.currentStep--;
//     }
//   }

//   setValue(
//     field: string,
//     value: string
//   ): void {
//     const control =
//       this.form.get(field);

//     if (!control) {
//       return;
//     }

//     control.setValue(value);
//     control.markAsTouched();
//     control.markAsDirty();
//   }

//   selectOption(
//     field: string | undefined,
//     value: string
//   ): void {
//     if (!field) {
//       return;
//     }

//     this.setValue(
//       field,
//       value
//     );

//     this.next();
//   }
//   resetZipValidationState(): void {
//     this.zipCodeError = '';
//     this.zipCodeVerified = false;
//     this.zipCodeChecking = false;
//     this.verifiedZipLocation = null;
//   }

//   validateEmailWithBouncer(
//     email: string
//   ): Promise<boolean> {

//     this.emailVerificationError = '';
//     this.emailChecking = true;

//     return new Promise<boolean>(
//       resolve => {

//         this.websiteLeadService
//           .validateEmail(email)
//           .subscribe({
//             next: response => {
//               this.emailChecking = false;

//               const result =
//                 response?.result ||
//                 response;

//               const accepted =
//                 result?.deliverable ===
//                   true &&
//                 String(
//                   result?.status || ''
//                 ).toLowerCase() ===
//                   'deliverable';

//               if (accepted) {
//                 resolve(true);
//                 return;
//               }

//               this.emailVerificationError =
//                 `Email verification failed. ${
//                   result?.reason ||
//                   'Please enter a valid working email.'
//                 }`;

//               resolve(false);
//             },

//             error: error => {
//               this.emailChecking = false;

//               this.emailVerificationError =
//                 error?.error?.message ||
//                 error?.error?.error ||
//                 'Email verification failed. Please enter a valid working email address.';

//               resolve(false);
//             }
//           });
//       }
//     );
//   }

//   async submitForm():
//     Promise<void> {

//     this.submittedSteps[
//       this.currentStep
//     ] = true;

//     this.submitError = '';
//     this.emailVerificationError = '';

//     const step =
//       this.steps[this.currentStep];

//     if (
//       !step ||
//       !this.isStepValid(step)
//     ) {
//       return;
//     }

//     if (this.form.invalid) {
//       this.form.markAllAsTouched();
//       return;
//     }

//     if (
//       this.isSubmitting ||
//       this.emailChecking ||
//       this.zipCodeChecking
//     ) {
//       return;
//     }
//     if (!this.zipCodeVerified) {
//       const zipVerified = await this.checkInitialZipCode();
//       if (!zipVerified) return;
//     }

//     const value =
//       this.form.getRawValue();

//     const emailVerified =
//       await this
//         .validateEmailWithBouncer(
//           value.email.trim()
//         );

//     if (!emailVerified) {
//       return;
//     }

//     await this.saveLead();
//   }

//   private async saveLead(): Promise<void> {
//     const value =
//       this.form.getRawValue();

//     const service =
//       this.links.find(
//         item =>
//           item.path ===
//           this.quoteSteps
//       ) ?? {
//         path: 'roofing',
//         title:
//           'Roofing Installation'
//       };

//     this.isSubmitting = true;

//     const trustedFormCertificateUrl =
//       await this.websiteLeadService.waitForTrustedFormCertificateUrl();

//     this.websiteLeadService
//       .createLead({
//         fullName:
//           `${value.firstName} ${value.lastName}`
//             .trim(),

//         email:
//           value.email.trim(),

//         phone:
//           value.phone.trim(),

//         serviceCode:
//           service.path,

//         campaignName:
//           service.title,

//         pageName:
//           `Homeyy ${service.title} Form`,

//         address:
//           value.address.trim(),

//         postcode: value.zipCode.trim(),

//         city:
//           this.verifiedZipLocation
//             ?.city ||
//           undefined,

//         state:
//           this.verifiedZipLocation
//             ?.state ||
//           undefined,

//         country:
//           this.verifiedZipLocation
//             ?.country ||
//           'United States',

//         countryCode: 'US',

//         step:
//           this.currentStep + 1,

//         isTest: false,
//         isCompleted: true,

//         fingerprintHash:
//           this.generateFingerprint(),

//         landingPageUrl:
//           window.location.href,

//         isTcpaCompliant: true,

//         trustedFormCertificateUrl,

//         ownsProperty:
//           String(value.homeOwner || '')
//             .trim()
//             .toLowerCase() === 'yes',

//         additionalDataJson:
//           JSON.stringify({
//             formType:
//               'ServiceQuote',

//             serviceCode:
//               service.path,

//             serviceName:
//               service.title,

//             purchaseTimeFrame:
//               value
//                 .purchaseTimeFrame ||
//               null,

//             bestTimeToCall:
//               value.bestTimeToCall ||
//               null,

//             answers: value
//           })
//       })
//       .subscribe({
//         next: () => {
//           this.isSubmitting = false;

//           if (
//             this.currentStep <
//             this.steps.length - 1
//           ) {
//             this.currentStep++;
//           }
//         },

//         error: error => {
//           this.isSubmitting = false;

//           this.submitError =
//             error?.error?.message ||
//             error?.error
//               ?.emailReason ||
//             error?.error
//               ?.addressCheck?.message ||
//             'We could not submit your request. Please try again.';
//         }
//       });
//   }

//   private stepControls(
//     step: Step
//   ): string[] {
//     switch (step.type) {
//       case 'intro':
//         return [
//           'zipCode'
//         ];

//       case 'buttons':
//         return step.field
//           ? [step.field]
//           : [];

//       case 'inputs':
//         return (
//           step.fields || []
//         ).map(
//           field => field.key
//         );

//       case 'location':
//         return (
//           step.fields || []
//         ).map(
//           field => field.key
//         );

//       case 'contact':
//         return [
//           'phone',
//           'bestTimeToCall',
//           'email'
//         ];

//       default:
//         return [];
//     }
//   }

//   private isStepValid(
//     step: Step
//   ): boolean {
//     return this
//       .stepControls(step)
//       .every(controlName => {
//         const control =
//           this.form.get(
//             controlName
//           );

//         if (!control) {
//           return true;
//         }

//         control.markAsTouched();

//         control
//           .updateValueAndValidity({
//             onlySelf: true
//           });

//         return control.valid;
//       });
//   }

//   isFieldInvalid(
//     controlName: string
//   ): boolean {
//     const control =
//       this.form.get(controlName);

//     return !!control &&
//       control.invalid &&
//       (
//         control.touched ||
//         control.dirty ||
//         this.submittedSteps[
//           this.currentStep
//         ]
//       );
//   }

//   private generateFingerprint():
//     string {

//     const data = [
//       navigator.userAgent,
//       navigator.language,
//       screen.width,
//       screen.height,
//       screen.colorDepth,
//       Intl.DateTimeFormat()
//         .resolvedOptions()
//         .timeZone
//     ].join('|');

//     let hash = 0;

//     for (
//       let i = 0;
//       i < data.length;
//       i++
//     ) {
//       hash =
//         ((hash << 5) - hash) +
//         data.charCodeAt(i);

//       hash |= 0;
//     }

//     return Math.abs(hash).toString();
//   }
// }
