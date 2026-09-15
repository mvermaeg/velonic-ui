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
      WebsiteLeadService
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

    this.saveLead();
  }

  private saveLead(): void {
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
        next: () => {
          this.isSubmitting = false;

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
// export class ServicesFormComponent implements OnInit {

//   @Input() quoteSteps = '';

//   currentStep = 0;
//   steps: Step[] = [];
//   form: FormGroup;

//   submittedSteps: {
//     [key: number]: boolean;
//   } = {};

//   isSubmitting = false;
//   submitError = '';

//   zipCodeChecking = false;
//   zipCodeError = '';
//   zipCodeVerified = false;

//   addressPostcodeError = '';
//   addressPostcodeVerified = false;
//   addressPostcodeChecking = false;
//   expectedLocation: any = null;

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
//     private websiteLeadService: WebsiteLeadService
//   ) {
//     this.form = this.fb.group({

//       zipCode: [
//         '',
//         [
//           Validators.required,
//           Validators.pattern(/^\d{5}$/)
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
//         [
//           Validators.required,
//           Validators.minLength(6)
//         ]
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
//       [key: string]: Step[];
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

//   async next(): Promise<void> {

//     this.submittedSteps[
//       this.currentStep
//     ] = true;

//     const step =
//       this.steps[this.currentStep];

//     if (
//       !step ||
//       !this.isStepValid(step)
//     ) {
//       return;
//     }

//     if (step.type === 'intro') {

//       const zipVerified =
//         await this.checkInitialZipCode();

//       if (!zipVerified) {
//         return;
//       }
//     }

//     if (step.type === 'location') {

//       const addressVerified =
//         await this.checkAddressPostcode();

//       if (!addressVerified) {
//         return;
//       }
//     }

//     if (
//       this.currentStep <
//       this.steps.length - 1
//     ) {
//       this.currentStep++;
//     }
//   }

//   checkInitialZipCode():
//     Promise<boolean> {

//     this.zipCodeError = '';
//     this.zipCodeVerified = false;

//     this.resetAddressPostcodeState();

//     const zipControl =
//       this.form.get('zipCode');

//     zipControl?.markAsTouched();
//     zipControl?.updateValueAndValidity();

//     if (
//       !zipControl ||
//       zipControl.invalid
//     ) {
//       return Promise.resolve(false);
//     }

//     const postcode =
//       String(
//         zipControl.value || ''
//       ).trim();

//     if (this.zipCodeChecking) {
//       return Promise.resolve(false);
//     }

//     this.zipCodeChecking = true;

//     return new Promise<boolean>(
//       resolve => {

//         this.websiteLeadService
//           .validateAddress(
//             postcode,
//             postcode
//           )
//           .subscribe({

//             next: response => {

//               this.zipCodeChecking =
//                 false;

//               const result =
//                 response?.result ||
//                 response;

//               const returnedPostcode =
//                 String(
//                   result?.postalCode ||
//                   ''
//                 )
//                   .trim()
//                   .substring(0, 5);

//               const providerAccepted =
//                 result?.isValid === true ||
//                 String(
//                   result?.status || ''
//                 ).toLowerCase() ===
//                   'valid' ||
//                 String(
//                   result?.possibleNextAction ||
//                   ''
//                 ).toUpperCase() ===
//                   'ACCEPT';

//               const postcodeMatches =
//                 returnedPostcode ===
//                 postcode;

//               if (
//                 providerAccepted &&
//                 postcodeMatches
//               ) {

//                 this.zipCodeVerified =
//                   true;

//                 this.zipCodeError = '';

//                 resolve(true);
//                 return;
//               }

//               this.zipCodeVerified =
//                 false;

//               this.zipCodeError =
//                 'Please enter a valid US ZIP code.';

//               resolve(false);
//             },

//             error: () => {

//               this.zipCodeChecking =
//                 false;

//               this.zipCodeVerified =
//                 false;

//               this.zipCodeError =
//                 'We could not verify this ZIP code. Please try again.';

//               resolve(false);
//             }
//           });
//       }
//     );
//   }

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

//   resetAddressPostcodeState():
//     void {

//     this.addressPostcodeError = '';
//     this.addressPostcodeVerified = false;
//     this.addressPostcodeChecking = false;
//     this.expectedLocation = null;
//   }

//   checkAddressPostcode():
//     Promise<boolean> {

//     this.addressPostcodeError = '';
//     this.addressPostcodeVerified = false;
//     this.expectedLocation = null;

//     const address =
//       String(
//         this.form
//           .get('address')
//           ?.value || ''
//       ).trim();

//     const postcode =
//       String(
//         this.form
//           .get('zipCode')
//           ?.value || ''
//       )
//         .trim()
//         .substring(0, 5);

//     if (!address) {

//       this.addressPostcodeError =
//         'Please enter your full street address.';

//       return Promise.resolve(false);
//     }

//     if (!postcode) {

//       this.addressPostcodeError =
//         'ZIP code is required.';

//       return Promise.resolve(false);
//     }

//     /*
//      * Reject obviously incomplete/non-street text.
//      *
//      * A US project address should normally contain
//      * both a street number and street text.
//      */
//     if (
//       address.length < 6 ||
//       !/\d/.test(address) ||
//       !/[a-zA-Z]/.test(address)
//     ) {

//       this.addressPostcodeError =
//         'Please enter a complete US street address.';

//       return Promise.resolve(false);
//     }

//     if (this.addressPostcodeChecking) {
//       return Promise.resolve(false);
//     }

//     this.addressPostcodeChecking =
//       true;

//     return new Promise<boolean>(
//       resolve => {

//         this.websiteLeadService
//           .validateAddress(
//             address,
//             postcode
//           )
//           .subscribe({

//             next: response => {

//               this.addressPostcodeChecking =
//                 false;

//               const result =
//                 response?.result ||
//                 response;

//               /*
//                * Provider says whether the address
//                * itself was accepted.
//                */
//               const providerAccepted =
//                 result?.isValid === true ||
//                 String(
//                   result?.status || ''
//                 ).toLowerCase() ===
//                   'valid' ||
//                 String(
//                   result?.possibleNextAction ||
//                   ''
//                 ).toUpperCase() ===
//                   'ACCEPT';

//               /*
//                * The validated address must return
//                * exactly the ZIP originally entered
//                * by the user.
//                */
//               const returnedPostcode =
//                 String(
//                   result?.postalCode ||
//                   ''
//                 )
//                   .trim()
//                   .substring(0, 5);

//               const postcodeMatches =
//                 returnedPostcode.length === 5 &&
//                 returnedPostcode === postcode;

//               /*
//                * Country validation.
//                */
//               const returnedCountry =
//                 String(
//                   result?.country ||
//                   ''
//                 )
//                   .trim()
//                   .toLowerCase();

//               const returnedCountryCode =
//                 String(
//                   result?.countryCode ||
//                   result?.countryShortName ||
//                   ''
//                 )
//                   .trim()
//                   .toUpperCase();

//               const countryWasReturned =
//                 returnedCountry.length > 0 ||
//                 returnedCountryCode.length > 0;

//               const isUnitedStates =
//                 returnedCountryCode === 'US' ||
//                 returnedCountry ===
//                   'united states' ||
//                 returnedCountry ===
//                   'united states of america' ||
//                 returnedCountry === 'usa' ||
//                 returnedCountry === 'us';

//               /*
//                * City + state must exist.
//                */
//               const city =
//                 String(
//                   result?.city ||
//                   ''
//                 ).trim();

//               const state =
//                 String(
//                   result?.state ||
//                   ''
//                 ).trim();

//               const hasLocation =
//                 city.length > 0 &&
//                 state.length > 0;

//               /*
//                * Provider rejection.
//                */
//               if (!providerAccepted) {

//                 this.addressPostcodeError =
//                   result?.message ||
//                   'We could not verify this address. Please enter a valid US street address.';

//                 resolve(false);
//                 return;
//               }

//               /*
//                * ZIP mismatch.
//                */
//               if (!postcodeMatches) {

//                 this.addressPostcodeError =
//                   `This address does not match ZIP code ${postcode}. Please enter an address located within this ZIP code.`;

//                 resolve(false);
//                 return;
//               }

//               /*
//                * Explicit non-US result.
//                *
//                * If the API returns a country,
//                * it must be United States.
//                */
//               if (
//                 countryWasReturned &&
//                 !isUnitedStates
//               ) {

//                 this.addressPostcodeError =
//                   'Please enter a valid address located in the United States.';

//                 resolve(false);
//                 return;
//               }

//               /*
//                * City/state are mandatory.
//                */
//               if (!hasLocation) {

//                 this.addressPostcodeError =
//                   'We could not confirm the city and state for this address. Please enter a complete US street address.';

//                 resolve(false);
//                 return;
//               }

//               /*
//                * Valid.
//                *
//                * IMPORTANT:
//                * Never replace the ZIP entered
//                * by the user with another ZIP.
//                */
//               this.addressPostcodeVerified =
//                 true;

//               this.expectedLocation =
//                 result;

//               this.addressPostcodeError =
//                 '';

//               resolve(true);
//             },

//             error: error => {

//               this.addressPostcodeChecking =
//                 false;

//               this.addressPostcodeVerified =
//                 false;

//               this.expectedLocation =
//                 null;

//               this.addressPostcodeError =
//                 error?.error?.message ||
//                 error?.error
//                   ?.result?.message ||
//                 'We could not verify this address. Please enter a valid US street address that matches your ZIP code.';

//               resolve(false);
//             }
//           });
//       }
//     );
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

//               this.emailChecking =
//                 false;

//               const result =
//                 response?.result ||
//                 response;

//               const accepted =
//                 result?.deliverable ===
//                   true &&
//                 String(
//                   result?.status ||
//                   ''
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

//               this.emailChecking =
//                 false;

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
//       this.addressPostcodeChecking
//     ) {
//       return;
//     }

//     /*
//      * Revalidate before final save
//      * if address state was reset.
//      */
//     if (
//       !this.addressPostcodeVerified
//     ) {

//       const addressVerified =
//         await this.checkAddressPostcode();

//       if (!addressVerified) {
//         return;
//       }
//     }

//     const value =
//       this.form.getRawValue();

//     const emailVerified =
//       await this
//         .validateEmailWithBouncer(
//           String(
//             value.email || ''
//           ).trim()
//         );

//     if (!emailVerified) {
//       return;
//     }

//     this.saveLead();
//   }

//   private saveLead(): void {

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

//     this.websiteLeadService
//       .createLead({

//         fullName:
//           `${value.firstName} ${value.lastName}`
//             .trim(),

//         email:
//           String(
//             value.email ||
//             ''
//           ).trim(),

//         phone:
//           String(
//             value.phone ||
//             ''
//           ).trim(),

//         serviceCode:
//           service.path,

//         campaignName:
//           service.title,

//         pageName:
//           `Homeyy ${service.title} Form`,

//         address:
//           String(
//             value.address ||
//             ''
//           ).trim(),

//         /*
//          * Use the ZIP the user actually
//          * entered because validation has
//          * already confirmed that the
//          * returned address matches it.
//          */
//         postcode:
//           String(
//             value.zipCode ||
//             ''
//           )
//             .trim()
//             .substring(0, 5),

//         city:
//           this.expectedLocation
//             ?.city ||
//           undefined,

//         state:
//           this.expectedLocation
//             ?.state ||
//           undefined,

//         country:
//           this.expectedLocation
//             ?.country ||
//           'United States',

//         countryCode:
//           'US',

//         step:
//           this.currentStep + 1,

//         isTest:
//           false,

//         isCompleted:
//           true,

//         fingerprintHash:
//           this.generateFingerprint(),

//         landingPageUrl:
//           window.location.href,

//         isTcpaCompliant:
//           true,

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
//               value
//                 .bestTimeToCall ||
//               null,

//             answers:
//               value
//           })
//       })
//       .subscribe({

//         next: () => {

//           this.isSubmitting =
//             false;

//           if (
//             this.currentStep <
//             this.steps.length - 1
//           ) {
//             this.currentStep++;
//           }
//         },

//         error: error => {

//           this.isSubmitting =
//             false;

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
//       this.form.get(
//         controlName
//       );

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

//     return Math.abs(
//       hash
//     ).toString();
//   }
// }


// // import {
// //   Component,
// //   Input,
// //   OnInit
// // } from '@angular/core';

// // import {
// //   FormBuilder,
// //   FormGroup,
// //   Validators
// // } from '@angular/forms';

// // import {
// //   FormsService
// // } from 'src/app/services/forms.service';

// // import {
// //   WebsiteLeadService
// // } from 'src/app/services/website-lead.service';

// // interface FormField {
// //   label: string;
// //   key: string;
// //   placeholder: string;
// //   inputType: string;
// // }

// // interface Step {
// //   title: string;
// //   subtitle?: string;
// //   type: string;
// //   field?: string;
// //   optionField?: string;
// //   options?: string[];
// //   fields?: FormField[];
// // }

// // @Component({
// //   selector: 'app-services-form',
// //   templateUrl: './services-form.component.html',
// //   styleUrls: ['./services-form.component.scss']
// // })
// // export class ServicesFormComponent
// //   implements OnInit {

// //   @Input() quoteSteps = '';

// //   currentStep = 0;
// //   steps: Step[] = [];
// //   form: FormGroup;

// //   submittedSteps: {
// //     [key: number]: boolean
// //   } = {};

// //   isSubmitting = false;
// //   submitError = '';
// // zipCodeChecking = false;
// // zipCodeError = '';
// // zipCodeVerified = false;
// //   addressPostcodeError = '';
// //   addressPostcodeVerified = false;
// //   addressPostcodeChecking = false;
// //   expectedLocation: any = null;

// //   emailChecking = false;
// //   emailVerificationError = '';

// //   get activeStep(): Step | undefined {
// //     return this.steps[this.currentStep];
// //   }

// //   links = [
// //     {
// //       path: 'hvac',
// //       title: 'HVAC Systems'
// //     },
// //     {
// //       path: 'bathroom',
// //       title: 'Bathroom Remodeling'
// //     },
// //     {
// //       path: 'kitchen',
// //       title: 'Kitchen Remodeling'
// //     },
// //     {
// //       path: 'plumbing',
// //       title: 'Plumbing Services'
// //     },
// //     {
// //       path: 'window',
// //       title: 'Window Installation'
// //     },
// //     {
// //       path: 'door',
// //       title: 'Door Installation'
// //     },
// //     {
// //       path: 'flooring',
// //       title: 'Flooring Services'
// //     },
// //     {
// //       path: 'gutter',
// //       title: 'Gutter Installation'
// //     },
// //     {
// //       path: 'fencing',
// //       title: 'Fencing Installation'
// //     },
// //     {
// //       path: 'solar',
// //       title: 'Solar Installation'
// //     },
// //     {
// //       path: 'roofing',
// //       title: 'Roofing Installation'
// //     },
// //     {
// //       path: 'siding',
// //       title: 'Siding Installation'
// //     },
// //     {
// //       path: 'homesecurity',
// //       title: 'Home Security Systems'
// //     }
// //   ];

// //   constructor(
// //     private fb: FormBuilder,
// //     public formService: FormsService,
// //     private websiteLeadService:
// //       WebsiteLeadService
// //   ) {
// //     this.form = this.fb.group({
// //       zipCode: [
// //         '',
// //         [
// //           Validators.required,
// //           Validators.pattern(
// //             /^\d{5}(-\d{4})?$/
// //           )
// //         ]
// //       ],

// //       firstName: [
// //         '',
// //         Validators.required
// //       ],

// //       lastName: [
// //         '',
// //         Validators.required
// //       ],

// //       address: [
// //         '',
// //         Validators.required
// //       ],

// //       purchaseTimeFrame: [''],

// //       phone: [
// //         '',
// //         [
// //           Validators.required,
// //           Validators.pattern(
// //             /^\+?1?[\s.-]?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$/
// //           )
// //         ]
// //       ],

// //       bestTimeToCall: [
// //         '',
// //         Validators.required
// //       ],

// //       email: [
// //         '',
// //         [
// //           Validators.required,
// //           Validators.email
// //         ]
// //       ]
// //     });
// //   }

// //   ngOnInit(): void {
// //     this.loadSteps();
// //   }

// //   loadSteps(): void {
// //     const dynamicSteps =
// //       this.getDynamicSteps(
// //         this.quoteSteps
// //       );

// //     const service =
// //       this.links.find(
// //         link =>
// //           link.path === this.quoteSteps
// //       ) ?? {
// //         path: 'roofing',
// //         title: 'Roofing Installation'
// //       };

// //     this.steps = [
// //       {
// //         title:
// //           `Get Free Quotes on New, Affordable ${service.title}`,

// //         subtitle:
// //           `Enter your details below to compare ${service.title} prices in your city.`,

// //         type: 'intro'
// //       },

// //       ...dynamicSteps,

// //       {
// //         title: 'What is your name?',
// //         type: 'inputs',

// //         fields: [
// //           {
// //             label: 'First Name',
// //             key: 'firstName',
// //             placeholder: 'First Name',
// //             inputType: 'text'
// //           },
// //           {
// //             label: 'Last Name',
// //             key: 'lastName',
// //             placeholder: 'Last Name',
// //             inputType: 'text'
// //           }
// //         ]
// //       },

// //       {
// //         title:
// //           'Where will this project take place?',

// //         type: 'location',

// //         optionField:
// //           'purchaseTimeFrame',

// //         options: [
// //           'Immediately',
// //           'Within 1 month',
// //           '1-3 months',
// //           '3+ months'
// //         ],

// //         fields: [
// //           {
// //             label: 'Address',
// //             key: 'address',
// //             placeholder:
// //               '123 Main Street',
// //             inputType: 'text'
// //           }
// //         ]
// //       },

// //       {
// //         title:
// //           'Please enter your phone number and email',

// //         type: 'contact',

// //         options: [
// //           'Morning',
// //           'Afternoon',
// //           'Evening',
// //           'Anytime'
// //         ]
// //       },

// //       {
// //         title:
// //           'Thank you for your application!',

// //         type: 'success'
// //       }
// //     ];

// //     this.ensureDynamicControls(
// //       dynamicSteps
// //     );
// //   }

// //   getDynamicSteps(
// //     service: string
// //   ): Step[] {
// //     const configs: {
// //       [key: string]: Step[]
// //     } = {
// //       hvac:
// //         this.formService.hvac,

// //       bathroom:
// //         this.formService.bathroom,

// //       kitchen:
// //         this.formService.kitchen,

// //       plumbing:
// //         this.formService.plumbing,

// //       window:
// //         this.formService.window,

// //       door:
// //         this.formService.door,

// //       flooring:
// //         this.formService.flooring,

// //       gutter:
// //         this.formService.gutter,

// //       fencing:
// //         this.formService.fencing,

// //       solar:
// //         this.formService.solar,

// //       roofing:
// //         this.formService.roofing,

// //       siding:
// //         this.formService.siding,

// //       homesecurity:
// //         this.formService.homesecurity
// //     };

// //     return configs[service] ||
// //       configs['roofing'];
// //   }

// //   ensureDynamicControls(
// //     steps: Step[]
// //   ): void {
// //     steps.forEach(step => {
// //       if (
// //         step.type === 'buttons' &&
// //         step.field &&
// //         !this.form.contains(step.field)
// //       ) {
// //         this.form.addControl(
// //           step.field,

// //           this.fb.control(
// //             '',
// //             Validators.required
// //           )
// //         );
// //       }
// //     });
// //   }

// //  async next(): Promise<void> {
// //   this.submittedSteps[this.currentStep] = true;

// //   const step = this.steps[this.currentStep];

// //   if (!step || !this.isStepValid(step)) {
// //     return;
// //   }

// //   if (step.type === 'intro') {
// //     const zipVerified =
// //       await this.checkInitialZipCode();

// //     if (!zipVerified) {
// //       return;
// //     }
// //   }

// //   if (step.type === 'location') {
// //     const addressVerified =
// //       await this.checkAddressPostcode();

// //     if (!addressVerified) {
// //       return;
// //     }
// //   }

// //   if (
// //     this.currentStep <
// //     this.steps.length - 1
// //   ) {
// //     this.currentStep++;
// //   }
// // }


// // checkInitialZipCode(): Promise<boolean> {
// //   this.zipCodeError = '';
// //   this.zipCodeVerified = false;

// //   const zipControl =
// //     this.form.get('zipCode');

// //   zipControl?.markAsTouched();
// //   zipControl?.updateValueAndValidity();

// //   if (!zipControl || zipControl.invalid) {
// //     return Promise.resolve(false);
// //   }

// //   const postcode =
// //     String(zipControl.value || '').trim();

// //   if (this.zipCodeChecking) {
// //     return Promise.resolve(false);
// //   }

// //   this.zipCodeChecking = true;

// //   /*
// //    * The existing Google address diagnostic requires
// //    * address and postcode. Passing the ZIP in both fields
// //    * asks Google to resolve the ZIP as a US location.
// //    */
// //   return new Promise<boolean>((resolve) => {
// //     this.websiteLeadService
// //       .validateAddress(
// //         postcode,
// //         postcode
// //       )
// //       .subscribe({
// //         next: (response) => {
// //           this.zipCodeChecking = false;

// //           const result =
// //             response?.result || response;

// //           const returnedPostcode =
// //             String(
// //               result?.postalCode || ''
// //             ).trim();

// //           const accepted =
// //             (
// //               result?.isValid === true ||
// //               String(
// //                 result?.status || ''
// //               ).toLowerCase() === 'valid' ||
// //               String(
// //                 result?.possibleNextAction || ''
// //               ).toUpperCase() === 'ACCEPT'
// //             ) &&
// //             returnedPostcode === postcode;

// //           if (accepted) {
// //             this.zipCodeVerified = true;
// //             this.zipCodeError = '';
// //             resolve(true);
// //             return;
// //           }

// //           this.zipCodeVerified = false;

// //           this.zipCodeError =
// //             'Please enter a valid US ZIP code.';

// //           resolve(false);
// //         },

// //         error: () => {
// //           this.zipCodeChecking = false;
// //           this.zipCodeVerified = false;

// //           this.zipCodeError =
// //             'We could not verify this ZIP code. Please try again.';

// //           resolve(false);
// //         }
// //       });
// //   });
// // }

// //   previous(): void {
// //     if (this.currentStep > 0) {
// //       this.currentStep--;
// //     }
// //   }

// //   setValue(
// //     field: string,
// //     value: string
// //   ): void {
// //     const control =
// //       this.form.get(field);

// //     if (!control) {
// //       return;
// //     }

// //     control.setValue(value);
// //     control.markAsTouched();
// //     control.markAsDirty();
// //   }

// //   selectOption(
// //     field: string | undefined,
// //     value: string
// //   ): void {
// //     if (!field) {
// //       return;
// //     }

// //     this.setValue(
// //       field,
// //       value
// //     );

// //     this.next();
// //   }

// //  resetAddressPostcodeState(): void {
// //   this.addressPostcodeError = '';
// //   this.addressPostcodeVerified = false;
// //   this.addressPostcodeChecking = false;
// //   this.expectedLocation = null;
// // }

// //   checkAddressPostcode():
// //     Promise<boolean> {

// //     this.addressPostcodeError = '';
// //     this.addressPostcodeVerified = false;
// //     this.expectedLocation = null;

// //     const address =
// //       this.form
// //         .get('address')
// //         ?.value
// //         ?.trim();

// //     const postcode =
// //       this.form
// //         .get('zipCode')
// //         ?.value
// //         ?.trim();

// //     if (!address) {
// //       this.addressPostcodeError =
// //         'Full address is required.';

// //       return Promise.resolve(false);
// //     }

// //     if (!postcode) {
// //       this.addressPostcodeError =
// //         'ZIP code is required.';

// //       return Promise.resolve(false);
// //     }

// //     this.addressPostcodeChecking = true;

// //     return new Promise<boolean>(
// //       resolve => {

// //         this.websiteLeadService
// //           .validateAddress(
// //             address,
// //             postcode
// //           )
// //           .subscribe({
// //             next: response => {
// //               this.addressPostcodeChecking =
// //                 false;

// //               const result =
// //                 response?.result ||
// //                 response;

// //               const accepted =
// //                 result?.isValid === true ||
// //                 String(
// //                   result?.status || ''
// //                 ).toLowerCase() ===
// //                   'valid' ||
// //                 String(
// //                   result
// //                     ?.possibleNextAction ||
// //                   ''
// //                 ).toUpperCase() ===
// //                   'ACCEPT';

// //               if (accepted) {
// //                 this.addressPostcodeVerified =
// //                   true;

// //                 this.expectedLocation =
// //                   result;

// //                 if (result?.postalCode) {
// //                   this.form
// //                     .get('zipCode')
// //                     ?.setValue(
// //                       result.postalCode,
// //                       {
// //                         emitEvent: false
// //                       }
// //                     );
// //                 }

// //                 resolve(true);
// //                 return;
// //               }

// //               this.addressPostcodeError =
// //                 result?.message ||
// //                 'Address verification failed.';

// //               resolve(false);
// //             },

// //             error: error => {
// //               this.addressPostcodeChecking =
// //                 false;

// //               this.addressPostcodeVerified =
// //                 false;

// //               this.addressPostcodeError =
// //                 error?.error?.message ||
// //                 error?.error
// //                   ?.result?.message ||
// //                 'Address verification failed.';

// //               resolve(false);
// //             }
// //           });
// //       }
// //     );
// //   }

// //   validateEmailWithBouncer(
// //     email: string
// //   ): Promise<boolean> {

// //     this.emailVerificationError = '';
// //     this.emailChecking = true;

// //     return new Promise<boolean>(
// //       resolve => {

// //         this.websiteLeadService
// //           .validateEmail(email)
// //           .subscribe({
// //             next: response => {
// //               this.emailChecking = false;

// //               const result =
// //                 response?.result ||
// //                 response;

// //               const accepted =
// //                 result?.deliverable ===
// //                   true &&
// //                 String(
// //                   result?.status || ''
// //                 ).toLowerCase() ===
// //                   'deliverable';

// //               if (accepted) {
// //                 resolve(true);
// //                 return;
// //               }

// //               this.emailVerificationError =
// //                 `Email verification failed. ${
// //                   result?.reason ||
// //                   'Please enter a valid working email.'
// //                 }`;

// //               resolve(false);
// //             },

// //             error: error => {
// //               this.emailChecking = false;

// //               this.emailVerificationError =
// //                 error?.error?.message ||
// //                 error?.error?.error ||
// //                 'Email verification failed. Please enter a valid working email address.';

// //               resolve(false);
// //             }
// //           });
// //       }
// //     );
// //   }

// //   async submitForm():
// //     Promise<void> {

// //     this.submittedSteps[
// //       this.currentStep
// //     ] = true;

// //     this.submitError = '';
// //     this.emailVerificationError = '';

// //     const step =
// //       this.steps[this.currentStep];

// //     if (
// //       !step ||
// //       !this.isStepValid(step)
// //     ) {
// //       return;
// //     }

// //     if (this.form.invalid) {
// //       this.form.markAllAsTouched();
// //       return;
// //     }

// //     if (
// //       this.isSubmitting ||
// //       this.emailChecking ||
// //       this.addressPostcodeChecking
// //     ) {
// //       return;
// //     }

// //     if (
// //       !this.addressPostcodeVerified
// //     ) {
// //       const addressVerified =
// //         await this.checkAddressPostcode();

// //       if (!addressVerified) {
// //         return;
// //       }
// //     }

// //     const value =
// //       this.form.getRawValue();

// //     const emailVerified =
// //       await this
// //         .validateEmailWithBouncer(
// //           value.email.trim()
// //         );

// //     if (!emailVerified) {
// //       return;
// //     }

// //     this.saveLead();
// //   }

// //   private saveLead(): void {
// //     const value =
// //       this.form.getRawValue();

// //     const service =
// //       this.links.find(
// //         item =>
// //           item.path ===
// //           this.quoteSteps
// //       ) ?? {
// //         path: 'roofing',
// //         title:
// //           'Roofing Installation'
// //       };

// //     this.isSubmitting = true;

// //     this.websiteLeadService
// //       .createLead({
// //         fullName:
// //           `${value.firstName} ${value.lastName}`
// //             .trim(),

// //         email:
// //           value.email.trim(),

// //         phone:
// //           value.phone.trim(),

// //         serviceCode:
// //           service.path,

// //         campaignName:
// //           service.title,

// //         pageName:
// //           `Homeyy ${service.title} Form`,

// //         address:
// //           value.address.trim(),

// //         postcode:
// //           this.expectedLocation
// //             ?.postalCode ||
// //           value.zipCode.trim(),

// //         city:
// //           this.expectedLocation
// //             ?.city ||
// //           undefined,

// //         state:
// //           this.expectedLocation
// //             ?.state ||
// //           undefined,

// //         country:
// //           this.expectedLocation
// //             ?.country ||
// //           'United States',

// //         countryCode: 'US',

// //         step:
// //           this.currentStep + 1,

// //         isTest: false,
// //         isCompleted: true,

// //         fingerprintHash:
// //           this.generateFingerprint(),

// //         landingPageUrl:
// //           window.location.href,

// //         isTcpaCompliant: true,

// //         additionalDataJson:
// //           JSON.stringify({
// //             formType:
// //               'ServiceQuote',

// //             serviceCode:
// //               service.path,

// //             serviceName:
// //               service.title,

// //             purchaseTimeFrame:
// //               value
// //                 .purchaseTimeFrame ||
// //               null,

// //             bestTimeToCall:
// //               value.bestTimeToCall ||
// //               null,

// //             answers: value
// //           })
// //       })
// //       .subscribe({
// //         next: () => {
// //           this.isSubmitting = false;

// //           if (
// //             this.currentStep <
// //             this.steps.length - 1
// //           ) {
// //             this.currentStep++;
// //           }
// //         },

// //         error: error => {
// //           this.isSubmitting = false;

// //           this.submitError =
// //             error?.error?.message ||
// //             error?.error
// //               ?.emailReason ||
// //             error?.error
// //               ?.addressCheck?.message ||
// //             'We could not submit your request. Please try again.';
// //         }
// //       });
// //   }

// //   private stepControls(
// //     step: Step
// //   ): string[] {
// //     switch (step.type) {
// //       case 'intro':
// //         return [
// //           'zipCode'
// //         ];

// //       case 'buttons':
// //         return step.field
// //           ? [step.field]
// //           : [];

// //       case 'inputs':
// //         return (
// //           step.fields || []
// //         ).map(
// //           field => field.key
// //         );

// //       case 'location':
// //         return (
// //           step.fields || []
// //         ).map(
// //           field => field.key
// //         );

// //       case 'contact':
// //         return [
// //           'phone',
// //           'bestTimeToCall',
// //           'email'
// //         ];

// //       default:
// //         return [];
// //     }
// //   }

// //   private isStepValid(
// //     step: Step
// //   ): boolean {
// //     return this
// //       .stepControls(step)
// //       .every(controlName => {
// //         const control =
// //           this.form.get(
// //             controlName
// //           );

// //         if (!control) {
// //           return true;
// //         }

// //         control.markAsTouched();

// //         control
// //           .updateValueAndValidity({
// //             onlySelf: true
// //           });

// //         return control.valid;
// //       });
// //   }

// //   isFieldInvalid(
// //     controlName: string
// //   ): boolean {
// //     const control =
// //       this.form.get(controlName);

// //     return !!control &&
// //       control.invalid &&
// //       (
// //         control.touched ||
// //         control.dirty ||
// //         this.submittedSteps[
// //           this.currentStep
// //         ]
// //       );
// //   }

// //   private generateFingerprint():
// //     string {

// //     const data = [
// //       navigator.userAgent,
// //       navigator.language,
// //       screen.width,
// //       screen.height,
// //       screen.colorDepth,
// //       Intl.DateTimeFormat()
// //         .resolvedOptions()
// //         .timeZone
// //     ].join('|');

// //     let hash = 0;

// //     for (
// //       let i = 0;
// //       i < data.length;
// //       i++
// //     ) {
// //       hash =
// //         ((hash << 5) - hash) +
// //         data.charCodeAt(i);

// //       hash |= 0;
// //     }

// //     return Math.abs(hash).toString();
// //   }
// // }

// // // import { Component, Input, OnInit } from '@angular/core';
// // // import {
// // //   FormBuilder,
// // //   FormGroup,
// // //   Validators
// // // } from '@angular/forms';
// // // import { FormsService } from 'src/app/services/forms.service';
// // // import {
// // //   WebsiteLeadService
// // // } from 'src/app/services/website-lead.service';

// // // interface FormField {
// // //   label: string;
// // //   key: string;
// // //   placeholder: string;
// // //   inputType: string;
// // // }

// // // interface Step {
// // //   title: string;
// // //   subtitle?: string;
// // //   type: string;
// // //   field?: string;
// // //   optionField?: string;
// // //   options?: string[];
// // //   fields?: FormField[];
// // // }

// // // @Component({
// // //   selector: 'app-services-form',
// // //   templateUrl: './services-form.component.html',
// // //   styleUrls: ['./services-form.component.scss']
// // // })
// // // export class ServicesFormComponent implements OnInit {

// // //   @Input() quoteSteps = '';

// // //   currentStep = 0;
// // //   steps: Step[] = [];
// // //   form: FormGroup;

// // //   submittedSteps: {
// // //     [key: number]: boolean
// // //   } = {};

// // //   isSubmitting = false;
// // //   submitError = '';

// // //   addressPostcodeError = '';
// // // addressPostcodeVerified = false;
// // // addressPostcodeChecking = false;
// // // expectedLocation: any = null;

// // // emailChecking = false;
// // // emailVerificationError = '';
// // // isValidatingAddress = false;
// // // addressValidationError = '';

// // // isValidatingEmail = false;
// // // emailValidationError = '';

// // // validatedCity = '';
// // // validatedState = '';
// // // validatedCountry = 'United States';
// // // validatedPostcode = '';
// // //   get activeStep(): Step | undefined {
// // //     return this.steps[this.currentStep];
// // //   }

// // //   links = [
// // //     { path: 'hvac', title: 'HVAC Systems' },
// // //     { path: 'bathroom', title: 'Bathroom Remodeling' },
// // //     { path: 'kitchen', title: 'Kitchen Remodeling' },
// // //     { path: 'plumbing', title: 'Plumbing Services' },
// // //     { path: 'window', title: 'Window Installation' },
// // //     { path: 'door', title: 'Door Installation' },
// // //     { path: 'flooring', title: 'Flooring Services' },
// // //     { path: 'gutter', title: 'Gutter Installation' },
// // //     { path: 'fencing', title: 'Fencing Installation' },
// // //     { path: 'solar', title: 'Solar Installation' },
// // //     { path: 'roofing', title: 'Roofing Installation' },
// // //     { path: 'siding', title: 'Siding Installation' },
// // //     {
// // //       path: 'homesecurity',
// // //       title: 'Home security systems'
// // //     }
// // //   ];

// // //   constructor(
// // //     private fb: FormBuilder,
// // //     public formService: FormsService,
// // //     private websiteLeadService: WebsiteLeadService
// // //   ) {
// // //     this.form = this.fb.group({
// // //       zipCode: [
// // //         '',
// // //         [
// // //           Validators.required,
// // //           Validators.pattern(/^\d{5}(-\d{4})?$/)
// // //         ]
// // //       ],

// // //       firstName: [
// // //         '',
// // //         Validators.required
// // //       ],

// // //       lastName: [
// // //         '',
// // //         Validators.required
// // //       ],

// // //       address: [
// // //         '',
// // //         Validators.required
// // //       ],

// // //       purchaseTimeFrame: [''],

// // //       phone: [
// // //         '',
// // //         [
// // //           Validators.required,
// // //           Validators.pattern(
// // //             /^\+?1?[\s.-]?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$/
// // //           )
// // //         ]
// // //       ],

// // //       bestTimeToCall: [
// // //         '',
// // //         Validators.required
// // //       ],

// // //       email: [
// // //         '',
// // //         [
// // //           Validators.required,
// // //           Validators.email
// // //         ]
// // //       ]
// // //     });
// // //   }

// // //   ngOnInit(): void {
// // //     this.loadSteps();
// // //   }

// // //   loadSteps(): void {
// // //     const dynamicSteps =
// // //       this.getDynamicSteps(this.quoteSteps);

// // //     const service =
// // //       this.links.find(
// // //         link => link.path === this.quoteSteps
// // //       ) ?? {
// // //         path: 'roofing',
// // //         title: 'Roofing Installation'
// // //       };

// // //     this.steps = [
// // //       {
// // //         title:
// // //           `Get Free Quotes on New, Affordable ${service.title}`,
// // //         subtitle:
// // //           `Enter your details below to compare ${service.title} prices in your city.`,
// // //         type: 'intro'
// // //       },

// // //       ...dynamicSteps,

// // //       {
// // //         title: 'What is your name?',
// // //         type: 'inputs',
// // //         fields: [
// // //           {
// // //             label: 'First Name',
// // //             key: 'firstName',
// // //             placeholder: 'First Name',
// // //             inputType: 'text'
// // //           },
// // //           {
// // //             label: 'Last Name',
// // //             key: 'lastName',
// // //             placeholder: 'Last Name',
// // //             inputType: 'text'
// // //           }
// // //         ]
// // //       },

// // //       {
// // //         title: 'Where will this project take place?',
// // //         type: 'location',
// // //         optionField: 'purchaseTimeFrame',
// // //         options: [
// // //           'Immediately',
// // //           'Within 1 month',
// // //           '1-3 months',
// // //           '3+ months'
// // //         ],
// // //         fields: [
// // //           {
// // //             label: 'Address',
// // //             key: 'address',
// // //             placeholder: '123 Your St.',
// // //             inputType: 'text'
// // //           }
// // //         ]
// // //       },

// // //       {
// // //         title:
// // //           'Please enter your phone number and email',
// // //         type: 'contact',
// // //         options: [
// // //           'Morning',
// // //           'Afternoon',
// // //           'Evening',
// // //           'Anytime'
// // //         ]
// // //       },

// // //       {
// // //         title: 'Thank you for your application!',
// // //         type: 'success'
// // //       }
// // //     ];

// // //     this.ensureDynamicControls(dynamicSteps);
// // //   }

// // //   getDynamicSteps(service: string): Step[] {
// // //     const configs: {
// // //       [key: string]: Step[]
// // //     } = {
// // //       hvac: this.formService.hvac,
// // //       bathroom: this.formService.bathroom,
// // //       kitchen: this.formService.kitchen,
// // //       plumbing: this.formService.plumbing,
// // //       window: this.formService.window,
// // //       door: this.formService.door,
// // //       flooring: this.formService.flooring,
// // //       gutter: this.formService.gutter,
// // //       fencing: this.formService.fencing,
// // //       solar: this.formService.solar,
// // //       roofing: this.formService.roofing,
// // //       siding: this.formService.siding,
// // //       homesecurity: this.formService.homesecurity
// // //     };

// // //     return configs[service] || configs['roofing'];
// // //   }

// // //   ensureDynamicControls(steps: Step[]): void {
// // //     steps.forEach(step => {
// // //       if (
// // //         step.type === 'buttons' &&
// // //         step.field &&
// // //         !this.form.contains(step.field)
// // //       ) {
// // //         this.form.addControl(
// // //           step.field,
// // //           this.fb.control(
// // //             '',
// // //             Validators.required
// // //           )
// // //         );
// // //       }
// // //     });
// // //   }

 
// // //  async next(): Promise<void> {
// // //   this.submittedSteps[this.currentStep] = true;

// // //   const step = this.steps[this.currentStep];

// // //   if (!step || !this.isStepValid(step)) {
// // //     return;
// // //   }

// // //   if (step.type === 'location') {
// // //     const addressVerified =
// // //       await this.checkAddressPostcode();

// // //     if (!addressVerified) {
// // //       return;
// // //     }
// // //   }

// // //   if (this.currentStep < this.steps.length - 1) {
// // //     this.currentStep++;
// // //   }
// // // }


// // // resetAddressPostcodeState(): void {
// // //   this.addressPostcodeError = '';
// // //   this.addressPostcodeVerified = false;
// // //   this.addressPostcodeChecking = false;
// // //   this.expectedLocation = null;
// // // }

// // // checkAddressPostcode(): Promise<boolean> {
// // //   this.addressPostcodeError = '';
// // //   this.addressPostcodeVerified = false;
// // //   this.expectedLocation = null;

// // //   const address =
// // //     this.form.get('address')?.value?.trim();

// // //   const postcode =
// // //     this.form.get('zipCode')?.value?.trim();

// // //   if (!address) {
// // //     this.addressPostcodeError =
// // //       'Full address is required.';

// // //     return Promise.resolve(false);
// // //   }

// // //   if (!postcode) {
// // //     this.addressPostcodeError =
// // //       'ZIP code is required.';

// // //     return Promise.resolve(false);
// // //   }

// // //   this.addressPostcodeChecking = true;

// // //   return new Promise<boolean>((resolve) => {
// // //     this.websiteLeadService
// // //       .validateAddress(address, postcode)
// // //       .subscribe({
// // //         next: (response) => {
// // //           this.addressPostcodeChecking = false;

// // //           const result =
// // //             response?.result || response;

// // //           const accepted =
// // //             result?.isValid === true ||
// // //             result?.status === 'Valid' ||
// // //             result?.possibleNextAction === 'ACCEPT';

// // //           if (accepted) {
// // //             this.addressPostcodeVerified = true;
// // //             this.expectedLocation = result;

// // //             if (result?.postalCode) {
// // //               this.form
// // //                 .get('zipCode')
// // //                 ?.setValue(
// // //                   result.postalCode,
// // //                   {
// // //                     emitEvent: false
// // //                   }
// // //                 );
// // //             }

// // //             resolve(true);
// // //             return;
// // //           }

// // //           this.addressPostcodeError =
// // //             result?.message ||
// // //             'Address verification failed.';

// // //           resolve(false);
// // //         },

// // //         error: (error) => {
// // //           this.addressPostcodeChecking = false;
// // //           this.addressPostcodeVerified = false;

// // //           this.addressPostcodeError =
// // //             error?.error?.message ||
// // //             error?.error?.result?.message ||
// // //             'Address verification failed.';

// // //           resolve(false);
// // //         }
// // //       });
// // //   });
// // // }


// // // validateEmailWithBouncer(
// // //   email: string
// // // ): Promise<boolean> {
// // //   this.emailVerificationError = '';
// // //   this.emailChecking = true;

// // //   return new Promise<boolean>((resolve) => {
// // //     this.websiteLeadService
// // //       .validateEmail(email)
// // //       .subscribe({
// // //         next: (response) => {
// // //           this.emailChecking = false;

// // //           const result =
// // //             response?.result || response;

// // //           const accepted =
// // //             result?.deliverable === true &&
// // //             String(
// // //               result?.status || ''
// // //             ).toLowerCase() === 'deliverable';

// // //           if (accepted) {
// // //             resolve(true);
// // //             return;
// // //           }

// // //           this.emailVerificationError =
// // //             `Email verification failed. ${
// // //               result?.reason ||
// // //               'Please enter a valid working email.'
// // //             }`;

// // //           resolve(false);
// // //         },

// // //         error: (error) => {
// // //           this.emailChecking = false;

// // //           this.emailVerificationError =
// // //             error?.error?.message ||
// // //             error?.error?.error ||
// // //             'Email verification failed. Please enter a valid working email address.';

// // //           resolve(false);
// // //         }
// // //       });
// // //   });
// // // }



// // // validateAddressAndContinue(): void {
// // //   this.addressValidationError = '';

// // //   const addressControl =
// // //     this.form.get('address');

// // //   const postcodeControl =
// // //     this.form.get('zipCode');

// // //   addressControl?.markAsTouched();
// // //   postcodeControl?.markAsTouched();

// // //   if (
// // //     addressControl?.invalid ||
// // //     postcodeControl?.invalid
// // //   ) {
// // //     return;
// // //   }

// // //   if (this.isValidatingAddress) {
// // //     return;
// // //   }

// // //   this.isValidatingAddress = true;

// // //   this.websiteLeadService.validateAddress(
// // //     addressControl?.value,
// // //     postcodeControl?.value
// // //   ).subscribe({
// // //     next: (response) => {
// // //       this.isValidatingAddress = false;

// // //       this.validatedCity =
// // //         response.city || '';

// // //       this.validatedState =
// // //         response.state || '';

// // //       this.validatedCountry =
// // //         response.country || 'United States';

// // //       this.validatedPostcode =
// // //         response.postalCode ||
// // //         postcodeControl?.value;

// // //       if (
// // //         this.currentStep <
// // //         this.steps.length - 1
// // //       ) {
// // //         this.currentStep++;
// // //       }
// // //     },

// // //     error: (error) => {
// // //       this.isValidatingAddress = false;

// // //       this.addressValidationError =
// // //         error?.error?.message ||
// // //         'Address and ZIP code could not be verified.';
// // //     }
// // //   });
// // // }


// // //   previous(): void {
// // //     if (this.currentStep > 0) {
// // //       this.currentStep--;
// // //     }
// // //   }

// // //   setValue(
// // //     field: string,
// // //     value: string
// // //   ): void {
// // //     const control = this.form.get(field);

// // //     if (control) {
// // //       control.setValue(value);
// // //       control.markAsTouched();
// // //       control.markAsDirty();
// // //     }
// // //   }

// // //   selectOption(
// // //     field: string | undefined,
// // //     value: string
// // //   ): void {
// // //     if (!field) {
// // //       return;
// // //     }

// // //     this.setValue(field, value);
// // //     this.next();
// // //   }

// // //   async submitForm(): Promise<void> {
// // //   this.submittedSteps[this.currentStep] = true;

// // //   this.submitError = '';
// // //   this.emailVerificationError = '';

// // //   const step = this.steps[this.currentStep];

// // //   if (!step || !this.isStepValid(step)) {
// // //     return;
// // //   }

// // //   if (this.form.invalid) {
// // //     this.form.markAllAsTouched();
// // //     return;
// // //   }

// // //   if (
// // //     this.isSubmitting ||
// // //     this.emailChecking
// // //   ) {
// // //     return;
// // //   }

// // //   if (!this.addressPostcodeVerified) {
// // //     const addressVerified =
// // //       await this.checkAddressPostcode();

// // //     if (!addressVerified) {
// // //       return;
// // //     }
// // //   }

// // //   const value = this.form.getRawValue();

// // //   const emailVerified =
// // //     await this.validateEmailWithBouncer(
// // //       value.email.trim()
// // //     );

// // //   if (!emailVerified) {
// // //     return;
// // //   }

// // //   this.saveLead();
// // // }



// // // private saveLead(): void {
// // //   const value = this.form.getRawValue();

// // //   const service =
// // //     this.links.find(
// // //       x => x.path === this.quoteSteps
// // //     ) ?? {
// // //       path: 'roofing',
// // //       title: 'Roofing Installation'
// // //     };

// // //   this.isSubmitting = true;

// // //   this.websiteLeadService.createLead({
// // //     fullName:
// // //       `${value.firstName} ${value.lastName}`.trim(),

// // //     email: value.email.trim(),
// // //     phone: value.phone.trim(),

// // //     serviceCode: service.path,
// // //     campaignName: service.title,
// // //     pageName:
// // //       `Homeyy ${service.title} Form`,

// // //     address: value.address.trim(),

// // //     postcode:
// // //       this.expectedLocation?.postalCode ||
// // //       value.zipCode.trim(),

// // //     city:
// // //       this.expectedLocation?.city || undefined,

// // //     state:
// // //       this.expectedLocation?.state || undefined,

// // //     country:
// // //       this.expectedLocation?.country ||
// // //       'United States',

// // //     countryCode: 'US',

// // //     step: this.currentStep + 1,
// // //     isTest: false,
// // //     isCompleted: true,

// // //     fingerprintHash:
// // //       this.generateFingerprint(),

// // //     landingPageUrl:
// // //       window.location.href,

// // //     isTcpaCompliant: true,

// // //     additionalDataJson:
// // //       JSON.stringify({
// // //         formType: 'ServiceQuote',
// // //         serviceCode: service.path,
// // //         serviceName: service.title,

// // //         purchaseTimeFrame:
// // //           value.purchaseTimeFrame || null,

// // //         bestTimeToCall:
// // //           value.bestTimeToCall || null,

// // //         answers: value
// // //       })
// // //   }).subscribe({
// // //     next: () => {
// // //       this.isSubmitting = false;
// // //       this.currentStep++;
// // //     },

// // //     error: (error) => {
// // //       this.isSubmitting = false;

// // //       this.submitError =
// // //         error?.error?.message ||
// // //         error?.error?.emailReason ||
// // //         error?.error?.addressCheck?.message ||
// // //         'We could not submit your request. Please try again.';
// // //     }
// // //   });
// // // }



// // //   private stepControls(
// // //     step: Step
// // //   ): string[] {
// // //     switch (step.type) {
// // //       case 'intro':
// // //         return ['zipCode'];

// // //       case 'buttons':
// // //         return step.field
// // //           ? [step.field]
// // //           : [];

// // //       case 'inputs':
// // //         return (step.fields || [])
// // //           .map(field => field.key);

// // //       case 'location':
// // //         return (step.fields || [])
// // //           .map(field => field.key);

// // //       case 'contact':
// // //         return [
// // //           'phone',
// // //           'bestTimeToCall',
// // //           'email'
// // //         ];

// // //       default:
// // //         return [];
// // //     }
// // //   }

// // //   private isStepValid(
// // //     step: Step
// // //   ): boolean {
// // //     return this.stepControls(step)
// // //       .every(controlName => {
// // //         const control =
// // //           this.form.get(controlName);

// // //         if (!control) {
// // //           return true;
// // //         }

// // //         control.markAsTouched();

// // //         control.updateValueAndValidity({
// // //           onlySelf: true
// // //         });

// // //         return control.valid;
// // //       });
// // //   }

// // //   isFieldInvalid(
// // //     controlName: string
// // //   ): boolean {
// // //     const control =
// // //       this.form.get(controlName);

// // //     return !!control &&
// // //       control.invalid &&
// // //       (
// // //         control.touched ||
// // //         control.dirty ||
// // //         this.submittedSteps[this.currentStep]
// // //       );
// // //   }
// // // }


// // // private generateFingerprint(): string {
// // //   const data = [
// // //     navigator.userAgent,
// // //     navigator.language,
// // //     screen.width,
// // //     screen.height,
// // //     screen.colorDepth,
// // //     Intl.DateTimeFormat()
// // //       .resolvedOptions()
// // //       .timeZone
// // //   ].join('|');

// // //   let hash = 0;

// // //   for (let i = 0; i < data.length; i++) {
// // //     hash =
// // //       ((hash << 5) - hash) +
// // //       data.charCodeAt(i);

// // //     hash |= 0;
// // //   }

// // //   return Math.abs(hash).toString();
// // // }