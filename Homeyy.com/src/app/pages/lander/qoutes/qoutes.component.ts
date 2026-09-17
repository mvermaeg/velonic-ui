import { Component, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsService } from 'src/app/services/forms.service';
import { WebsiteLeadService } from 'src/app/services/website-lead.service';

interface FormField {
  label: string;
  key: string;
  placeholder: string;
  type: string;
}

interface Step {
  title: string;
  subtitle?: string;
  type: 'intro' | 'buttons' | 'form' | 'location' | 'contact';
  field?: string;
  options?: string[];
  fields?: FormField[];
}

interface HowItWorksCard {
  number: string;
  icon: string;
  alt: string;
  text: string;
}

@Component({
  selector: 'app-qoutes',
  templateUrl: './qoutes.component.html',
  styleUrls: ['./qoutes.component.scss']
})
export class QoutesComponent implements OnInit {

  step = 1;
  totalSteps = 1;

  path_name = '';
  service_name = '';
  serviceCode = '';
  imageUrl = '';

  quoteForm!: FormGroup;
  howItWorks: HowItWorksCard[] = [];

  zipChecking = false;
  zipVerified = false;
  zipValidationError = '';
  zipLocationMessage = '';

  emailChecking = false;
  emailValidationError = '';

  isSubmitting = false;
  submitError = '';

  validatedPostcode = '';
  validatedCity = '';
  validatedState = '';
  validatedCountry = 'United States';

  private steps: Step[] = [];

  private labels: { [key: string]: string } = {
    zip: 'Zip code',
    firstName: 'First name',
    lastName: 'Last name',
    address: 'Address',
    phone: 'Mobile number',
    bestTimeToCall: 'Best time to call',
    email: 'Email address'
  };

  private customMessages: {
    [key: string]: {
      [error: string]: string
    }
  } = {
    zip: {
      pattern: 'Enter a valid US ZIP code, for example 33021'
    },
    firstName: {
      pattern:
        'First name can only contain letters, spaces, hyphens and apostrophes'
    },
    lastName: {
      pattern:
        'Last name can only contain letters, spaces, hyphens and apostrophes'
    },
    phone: {
      pattern: 'Enter a valid 10-digit US phone number'
    },
    bestTimeToCall: {
      required: 'Please select the best time to call'
    },
    email: {
      pattern: 'Enter a valid email address'
    }
  };

  constructor(
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private formsService: FormsService,
    private websiteLeadService: WebsiteLeadService
  ) {}

  ngOnInit(): void {
    this.path_name =
      this.activatedRoute.snapshot.paramMap.get('qoute-name') ?? '';

    this.serviceCode = this.normalizeServiceCode(this.path_name);
    this.service_name = this.getServiceName(this.serviceCode);

    this.imageUrl =
      this.formsService.qoutes.find(
        x =>
          x.path === this.path_name ||
          x.path === this.serviceCode
      )?.icon ?? '';

    this.buildHowItWorks();
    this.loadSteps(this.serviceCode);
    this.buildForm();
    this.watchValidationFields();
  }

  get activeStep(): Step | undefined {
    return this.steps[this.step - 1];
  }

  private normalizeServiceCode(path: string): string {
    const normalized = (path || '').trim().toLowerCase();

    if (normalized === 'home-security') {
      return 'homesecurity';
    }

    return normalized || 'roofing';
  }

  private getServiceName(serviceCode: string): string {
    const names: { [key: string]: string } = {
      hvac: 'HVAC',
      bathroom: 'Bathroom Remodeling',
      kitchen: 'Kitchen Remodeling',
      plumbing: 'Plumbing',
      window: 'Window Installation',
      door: 'Door Installation',
      flooring: 'Flooring',
      gutter: 'Gutter Installation',
      fencing: 'Fencing',
      solar: 'Solar Installation',
      roofing: 'Roofing',
      siding: 'Siding',
      homesecurity: 'Home Security'
    };

    return names[serviceCode] || 'Roofing';
  }

  private buildHowItWorks(): void {
    const service = this.service_name;

    this.howItWorks = [
      {
        number: '01',
        icon: 'assets/images/icons/form.svg',
        alt: 'Form',
        text:
          `Answer a few simple questions regarding your new ` +
          `${service} requirements`
      },
      {
        number: '02',
        icon: 'assets/images/icons/contact.svg',
        alt: 'Contact',
        text:
          `Fill in your contact details so we can match you with ` +
          `the best ${service} professionals in your area`
      },
      {
        number: '03',
        icon: 'assets/images/icons/check-list.svg',
        alt: 'Quote',
        text:
          `Receive up to 4 free, no-obligation quotes from local ` +
          `${service} experts`
      }
    ];
  }

  private loadSteps(service: string): void {
    const configs: { [key: string]: Step[] } = {
      solar: this.formsService.solar,
      kitchen: this.formsService.kitchen,
      plumbing: this.formsService.plumbing,
      door: this.formsService.door,
      flooring: this.formsService.flooring,
      gutter: this.formsService.gutter,
      fencing: this.formsService.fencing,
      homesecurity: this.formsService.homesecurity,
      roofing: this.formsService.roofing,
      hvac: this.formsService.hvac,
      window: this.formsService.window,
      bathroom: this.formsService.bathroom,
      siding: this.formsService.siding
    };

    this.steps = [
      {
        title:
          `Get Free Quotes on New, Affordable ${this.service_name}`,
        subtitle:
          `The fastest way to compare ${this.service_name} prices. ` +
          `Get a free quote for your home.`,
        type: 'intro'
      },

      ...(configs[service] || configs['roofing']),

      {
        title: 'What is your name?',
        type: 'form',
        fields: [
          {
            label: 'First Name',
            key: 'firstName',
            placeholder: 'First Name',
            type: 'text'
          },
          {
            label: 'Last Name',
            key: 'lastName',
            placeholder: 'Last Name',
            type: 'text'
          }
        ]
      },

      {
        title: 'Where will this project take place?',
        type: 'location',
        options: [
          'Immediately',
          'Within 1 Month',
          '1-3 Months',
          '3+ Months'
        ],
        fields: [
          {
            label: 'Address',
            key: 'address',
            placeholder: '13500 S. Figueroa St.',
            type: 'text'
          }
        ]
      },

      {
        title: 'Please enter your phone number and email',
        type: 'contact',
        options: [
          'Morning',
          'Afternoon',
          'Evening',
          'Anytime'
        ]
      }
    ];

    this.totalSteps = this.steps.length;
  }

  private buildForm(): void {
    const nameRules = [
      Validators.required,
      Validators.minLength(2),
      Validators.maxLength(30),
      Validators.pattern(/^[A-Za-z][A-Za-z '-]*$/)
    ];

    const controls: { [key: string]: any } = {
      zip: [
        '',
        [
          Validators.required,
          Validators.pattern(/^\d{5}$/)
        ]
      ],

      firstName: ['', nameRules],
      lastName: ['', nameRules],

      address: [
        '',
        [
          Validators.required,
          Validators.minLength(5),
          Validators.maxLength(150)
        ]
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

      bestTimeToCall: ['', Validators.required],

      email: [
        '',
        [
          Validators.required,
          Validators.email,
          Validators.pattern(
            /^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$/
          )
        ]
      ]
    };

    this.steps.forEach(step => {
      if (
        step.type === 'buttons' &&
        step.field &&
        !controls[step.field]
      ) {
        controls[step.field] = ['', Validators.required];
      }

      (step.fields || []).forEach(field => {
        if (!controls[field.key]) {
          controls[field.key] = ['', Validators.required];
        }
      });
    });

    this.quoteForm = this.fb.group(controls);
  }

  private watchValidationFields(): void {
    this.quoteForm.get('zip')?.valueChanges.subscribe(() => {
      this.zipVerified = false;
      this.zipValidationError = '';
      this.zipLocationMessage = '';
    });

    this.quoteForm.get('email')?.valueChanges.subscribe(() => {
      this.emailValidationError = '';
    });
  }

  private stepControls(stepNumber: number): string[] {
    const current = this.steps[stepNumber - 1];

    switch (current?.type) {
      case 'intro':
        return ['zip'];

      case 'buttons':
        return current.field ? [current.field] : [];

      case 'form':
      case 'location':
        return (current.fields || []).map(field => field.key);

      case 'contact':
        return ['phone', 'bestTimeToCall', 'email'];

      default:
        return [];
    }
  }

  private stepAbstractControls(
    stepNumber: number
  ): AbstractControl[] {
    return this.stepControls(stepNumber)
      .map(name => this.quoteForm.get(name))
      .filter(
        (control): control is AbstractControl => !!control
      );
  }

  private validateStep(stepNumber: number): boolean {
    const controls =
      this.stepAbstractControls(stepNumber);

    controls.forEach(control => {
      control.markAsTouched();
      control.updateValueAndValidity({
        onlySelf: true
      });
    });

    return controls.every(control => control.valid);
  }

  private control(name: string): AbstractControl | null {
    return this.quoteForm?.get(name) ?? null;
  }

  isInvalid(name: string): boolean {
    const control = this.control(name);

    return !!control &&
      control.invalid &&
      (control.touched || control.dirty);
  }

  getError(name: string, label?: string): string {
    const control = this.control(name);

    if (
      !control?.errors ||
      !(control.touched || control.dirty)
    ) {
      return '';
    }

    const errors = control.errors;
    const title =
      label || this.labels[name] || 'This field';

    const custom =
      this.customMessages[name] || {};

    if (errors['required']) {
      return custom['required'] ||
        `${title} is required`;
    }

    if (errors['minlength']) {
      return custom['minlength'] ||
        `${title} must be at least ` +
        `${errors['minlength'].requiredLength} characters`;
    }

    if (errors['maxlength']) {
      return custom['maxlength'] ||
        `${title} cannot be longer than ` +
        `${errors['maxlength'].requiredLength} characters`;
    }

    if (errors['email'] || errors['pattern']) {
      return custom['pattern'] ||
        `Enter a valid ${title.toLowerCase()}`;
    }

    return `${title} is not valid`;
  }

  async nextStep(): Promise<void> {
    this.submitError = '';

    if (!this.validateStep(this.step)) {
      return;
    }

    const currentType = this.activeStep?.type;

    if (currentType === 'intro') {
      const zipIsValid = await this.verifyZipCode();

      if (!zipIsValid) {
        return;
      }
    }

    if (this.step < this.totalSteps) {
      this.step++;
    }
  }

  previousStep(): void {
    if (
      this.zipChecking ||
      this.emailChecking ||
      this.isSubmitting
    ) {
      return;
    }

    if (this.step > 1) {
      this.step--;
    }
  }

  setValue(field: string, value: any): void {
    const control = this.control(field);

    if (control) {
      control.setValue(value);
      control.markAsTouched();
      control.markAsDirty();
    }
  }

  selectOption(
    field: string | undefined,
    value: string
  ): void {
    if (!field) {
      return;
    }

    this.setValue(field, value);
    void this.nextStep();
  }

  private verifyZipCode(): Promise<boolean> {
    this.zipValidationError = '';
    this.zipLocationMessage = '';

    if (this.zipVerified) {
      return Promise.resolve(true);
    }

    const zip =
      String(this.quoteForm.get('zip')?.value || '').trim();

    if (!/^\d{5}$/.test(zip)) {
      this.zipValidationError =
        'Enter a valid 5-digit US ZIP code.';

      return Promise.resolve(false);
    }

    this.zipChecking = true;

    /*
     * The existing address-validation API is reused here.
     * Passing the ZIP as the address allows Google Geocoding
     * to confirm that the ZIP exists in the United States.
     */
    return new Promise<boolean>(resolve => {
      this.websiteLeadService
        .validateAddress(zip, zip)
        .subscribe({
          next: response => {
            this.zipChecking = false;

            const result =
              response?.result || response;

            const accepted =
              result?.isValid === true ||
              String(result?.status || '')
                .toLowerCase() === 'valid' ||
              String(result?.possibleNextAction || '')
                .toUpperCase() === 'ACCEPT';

            if (!accepted) {
              this.zipVerified = false;
              this.zipValidationError =
                result?.message ||
                'This ZIP code could not be verified.';

              resolve(false);
              return;
            }

            this.zipVerified = true;

            this.validatedPostcode =
              result?.postalCode || zip;

            this.validatedCity =
              result?.city || '';

            this.validatedState =
              result?.state || '';

            this.validatedCountry =
              result?.country || 'United States';

            this.zipLocationMessage =
              this.buildLocationMessage(result);

            resolve(true);
          },

          error: error => {
            this.zipChecking = false;
            this.zipVerified = false;

            this.zipValidationError =
              error?.error?.result?.message ||
              error?.error?.message ||
              'ZIP code verification failed. Please try again.';

            resolve(false);
          }
        });
    });
  }

  private verifyEmail(): Promise<boolean> {
    this.emailValidationError = '';

    const email =
      String(
        this.quoteForm.get('email')?.value || ''
      ).trim();

    this.emailChecking = true;

    return new Promise<boolean>(resolve => {
      this.websiteLeadService
        .validateEmail(email)
        .subscribe({
          next: response => {
            this.emailChecking = false;

            const result =
              response?.result || response;

            const status =
              String(result?.status || '')
                .toLowerCase();

            const accepted =
              result?.deliverable === true ||
              status === 'deliverable';

            if (accepted) {
              resolve(true);
              return;
            }

            this.emailValidationError =
              result?.reason
                ? `Email verification failed: ${result.reason}`
                : 'Please enter a valid, deliverable email address.';

            resolve(false);
          },

          error: error => {
            this.emailChecking = false;

            const providerMessage =
              error?.error?.result?.reason ||
              error?.error?.message ||
              error?.error?.error;

            this.emailValidationError =
              providerMessage
                ? `Email verification failed: ${providerMessage}`
                : 'Email verification failed. Please try again.';

            resolve(false);
          }
        });
    });
  }

  private firstInvalidStep(): number {
    for (
      let stepNumber = 1;
      stepNumber <= this.totalSteps;
      stepNumber++
    ) {
      if (
        this.stepAbstractControls(stepNumber)
          .some(control => control.invalid)
      ) {
        return stepNumber;
      }
    }

    return 0;
  }

  async submitForm(): Promise<void> {
    this.submitError = '';
    this.emailValidationError = '';

    this.quoteForm.markAllAsTouched();

    if (this.quoteForm.invalid) {
      const invalidStep =
        this.firstInvalidStep();

      if (invalidStep) {
        this.step = invalidStep;
      }

      return;
    }

    if (
      this.isSubmitting ||
      this.zipChecking ||
      this.emailChecking
    ) {
      return;
    }

    if (!this.zipVerified) {
      const zipIsValid =
        await this.verifyZipCode();

      if (!zipIsValid) {
        this.step = 1;
        return;
      }
    }

    const emailIsValid =
      await this.verifyEmail();

    if (!emailIsValid) {
      return;
    }

    await this.saveLead();
  }

  private async saveLead(): Promise<void> {
    const value =
      this.quoteForm.getRawValue();

    const trustedFormCertificateUrl =
      await this.websiteLeadService.waitForTrustedFormCertificateUrl();

    this.isSubmitting = true;
    this.submitError = '';

    this.websiteLeadService.createLead({
      fullName:
        `${value.firstName} ${value.lastName}`.trim(),

      email:
        String(value.email || '').trim(),

      phone:
        String(value.phone || '').trim(),

      serviceCode:
        this.serviceCode,

      campaignName:
        this.service_name,

      pageName:
        `Homeyy Services ${this.service_name} Quote Form`,

      address:
        String(value.address || '').trim(),

      postcode:
        this.validatedPostcode ||
        String(value.zip || '').trim(),

      city:
        this.validatedCity || undefined,

      state:
        this.validatedState || undefined,

      country:
        this.validatedCountry ||
        'United States',

      countryCode: 'US',

      step: this.totalSteps,

      isTest: false,
      isCompleted: true,

      affiliateSubId: null,

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
          formType: 'ServicesQuote',
          routeType: '/services/:service',
          serviceCode: this.serviceCode,
          serviceName: this.service_name,
          purchaseTimeFrame:
            value.purchaseTimeFrame || null,
          bestTimeToCall:
            value.bestTimeToCall || null,
          answers: value
        })
    }).subscribe({
      next: response => {
        this.isSubmitting = false;

        if (this.websiteLeadService.isThumbtackService(this.serviceCode)) {
          this.websiteLeadService.rememberThumbtackLead(
            response,
            this.serviceCode
          );
        }

        this.router.navigate([
          '/',
          this.path_name,
          'thank-you'
        ]);
      },

      error: error => {
        this.isSubmitting = false;

        this.submitError =
          error?.error?.message ||
          'We could not submit your request. Please try again.';
      }
    });
  }

  private buildLocationMessage(result: any): string {
    return [
      result?.city,
      result?.state,
      result?.postalCode
    ]
      .filter(Boolean)
      .join(', ');
  }

  private generateFingerprint(): string {
    const source = [
      navigator.userAgent || '',
      navigator.language || '',
      screen.width,
      screen.height,
      new Date().getTimezoneOffset()
    ].join('|');

    let hash = 0;

    for (let index = 0; index < source.length; index++) {
      hash =
        ((hash << 5) - hash) +
        source.charCodeAt(index);

      hash |= 0;
    }

    return Math.abs(hash).toString();
  }
}

 

// import { Component, OnInit } from '@angular/core';
// import {
//   AbstractControl,
//   FormBuilder,
//   FormGroup,
//   Validators
// } from '@angular/forms';
// import { ActivatedRoute, Router } from '@angular/router';
// import { FormsService } from 'src/app/services/forms.service';
// import { WebsiteLeadService } from 'src/app/services/website-lead.service';

// interface FormField {
//   label: string;
//   key: string;
//   placeholder: string;
//   type: string;
// }

// interface Step {
//   title: string;
//   subtitle?: string;
//   type: 'intro' | 'buttons' | 'form' | 'location' | 'contact';
//   field?: string;
//   options?: string[];
//   fields?: FormField[];
// }

// interface HowItWorksCard {
//   number: string;
//   icon: string;
//   alt: string;
//   text: string;
// }

// @Component({
//   selector: 'app-qoutes',
//   templateUrl: './qoutes.component.html',
//   styleUrls: ['./qoutes.component.scss']
// })
// export class QoutesComponent implements OnInit {

//   step = 1;
//   totalSteps = 1;

//   path_name = '';
//   service_name = '';
//   serviceCode = '';
//   imageUrl = '';

//   quoteForm!: FormGroup;
//   howItWorks: HowItWorksCard[] = [];

//   zipChecking = false;
//   zipVerified = false;
//   zipValidationError = '';
//   zipLocationMessage = '';

//   emailChecking = false;
//   emailValidationError = '';

//   isSubmitting = false;
//   submitError = '';

//   validatedPostcode = '';
//   validatedCity = '';
//   validatedState = '';
//   validatedCountry = 'United States';

//   private steps: Step[] = [];

//   private labels: { [key: string]: string } = {
//     zip: 'Zip code',
//     firstName: 'First name',
//     lastName: 'Last name',
//     address: 'Address',
//     phone: 'Mobile number',
//     bestTimeToCall: 'Best time to call',
//     email: 'Email address'
//   };

//   private customMessages: {
//     [key: string]: {
//       [error: string]: string
//     }
//   } = {
//     zip: {
//       pattern: 'Enter a valid US ZIP code, for example 33021'
//     },
//     firstName: {
//       pattern:
//         'First name can only contain letters, spaces, hyphens and apostrophes'
//     },
//     lastName: {
//       pattern:
//         'Last name can only contain letters, spaces, hyphens and apostrophes'
//     },
//     phone: {
//       pattern: 'Enter a valid 10-digit US phone number'
//     },
//     bestTimeToCall: {
//       required: 'Please select the best time to call'
//     },
//     email: {
//       pattern: 'Enter a valid email address'
//     }
//   };

//   constructor(
//     private activatedRoute: ActivatedRoute,
//     private router: Router,
//     private fb: FormBuilder,
//     private formsService: FormsService,
//     private websiteLeadService: WebsiteLeadService
//   ) {}

//   ngOnInit(): void {
//     this.path_name =
//       this.activatedRoute.snapshot.paramMap.get('qoute-name') ?? '';

//     this.serviceCode = this.normalizeServiceCode(this.path_name);
//     this.service_name = this.getServiceName(this.serviceCode);

//     this.imageUrl =
//       this.formsService.qoutes.find(
//         x =>
//           x.path === this.path_name ||
//           x.path === this.serviceCode
//       )?.icon ?? '';

//     this.buildHowItWorks();
//     this.loadSteps(this.serviceCode);
//     this.buildForm();
//     this.watchValidationFields();
//   }

//   get activeStep(): Step | undefined {
//     return this.steps[this.step - 1];
//   }

//   private normalizeServiceCode(path: string): string {
//     const normalized = (path || '').trim().toLowerCase();

//     if (normalized === 'home-security') {
//       return 'homesecurity';
//     }

//     return normalized || 'roofing';
//   }

//   private getServiceName(serviceCode: string): string {
//     const names: { [key: string]: string } = {
//       hvac: 'HVAC',
//       bathroom: 'Bathroom Remodeling',
//       kitchen: 'Kitchen Remodeling',
//       plumbing: 'Plumbing',
//       window: 'Window Installation',
//       door: 'Door Installation',
//       flooring: 'Flooring',
//       gutter: 'Gutter Installation',
//       fencing: 'Fencing',
//       solar: 'Solar Installation',
//       roofing: 'Roofing',
//       siding: 'Siding',
//       homesecurity: 'Home Security'
//     };

//     return names[serviceCode] || 'Roofing';
//   }

//   private buildHowItWorks(): void {
//     const service = this.service_name;

//     this.howItWorks = [
//       {
//         number: '01',
//         icon: 'assets/images/icons/form.svg',
//         alt: 'Form',
//         text:
//           `Answer a few simple questions regarding your new ` +
//           `${service} requirements`
//       },
//       {
//         number: '02',
//         icon: 'assets/images/icons/contact.svg',
//         alt: 'Contact',
//         text:
//           `Fill in your contact details so we can match you with ` +
//           `the best ${service} professionals in your area`
//       },
//       {
//         number: '03',
//         icon: 'assets/images/icons/check-list.svg',
//         alt: 'Quote',
//         text:
//           `Receive up to 4 free, no-obligation quotes from local ` +
//           `${service} experts`
//       }
//     ];
//   }

//   private loadSteps(service: string): void {
//     const configs: { [key: string]: Step[] } = {
//       solar: this.formsService.solar,
//       kitchen: this.formsService.kitchen,
//       plumbing: this.formsService.plumbing,
//       door: this.formsService.door,
//       flooring: this.formsService.flooring,
//       gutter: this.formsService.gutter,
//       fencing: this.formsService.fencing,
//       homesecurity: this.formsService.homesecurity,
//       roofing: this.formsService.roofing,
//       hvac: this.formsService.hvac,
//       window: this.formsService.window,
//       bathroom: this.formsService.bathroom,
//       siding: this.formsService.siding
//     };

//     this.steps = [
//       {
//         title:
//           `Get Free Quotes on New, Affordable ${this.service_name}`,
//         subtitle:
//           `The fastest way to compare ${this.service_name} prices. ` +
//           `Get a free quote for your home.`,
//         type: 'intro'
//       },

//       ...(configs[service] || configs['roofing']),

//       {
//         title: 'What is your name?',
//         type: 'form',
//         fields: [
//           {
//             label: 'First Name',
//             key: 'firstName',
//             placeholder: 'First Name',
//             type: 'text'
//           },
//           {
//             label: 'Last Name',
//             key: 'lastName',
//             placeholder: 'Last Name',
//             type: 'text'
//           }
//         ]
//       },

//       {
//         title: 'Where will this project take place?',
//         type: 'location',
//         options: [
//           'Immediately',
//           'Within 1 Month',
//           '1-3 Months',
//           '3+ Months'
//         ],
//         fields: [
//           {
//             label: 'Address',
//             key: 'address',
//             placeholder: '13500 S. Figueroa St.',
//             type: 'text'
//           }
//         ]
//       },

//       {
//         title: 'Please enter your phone number and email',
//         type: 'contact',
//         options: [
//           'Morning',
//           'Afternoon',
//           'Evening',
//           'Anytime'
//         ]
//       }
//     ];

//     this.totalSteps = this.steps.length;
//   }

//   private buildForm(): void {
//     const nameRules = [
//       Validators.required,
//       Validators.minLength(2),
//       Validators.maxLength(30),
//       Validators.pattern(/^[A-Za-z][A-Za-z '-]*$/)
//     ];

//     const controls: { [key: string]: any } = {
//       zip: [
//         '',
//         [
//           Validators.required,
//           Validators.pattern(/^\d{5}$/)
//         ]
//       ],

//       firstName: ['', nameRules],
//       lastName: ['', nameRules],

//       address: [
//         '',
//         [
//           Validators.required,
//           Validators.minLength(5),
//           Validators.maxLength(150)
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

//       bestTimeToCall: ['', Validators.required],

//       email: [
//         '',
//         [
//           Validators.required,
//           Validators.email,
//           Validators.pattern(
//             /^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$/
//           )
//         ]
//       ]
//     };

//     this.steps.forEach(step => {
//       if (
//         step.type === 'buttons' &&
//         step.field &&
//         !controls[step.field]
//       ) {
//         controls[step.field] = ['', Validators.required];
//       }

//       (step.fields || []).forEach(field => {
//         if (!controls[field.key]) {
//           controls[field.key] = ['', Validators.required];
//         }
//       });
//     });

//     this.quoteForm = this.fb.group(controls);
//   }

//   private watchValidationFields(): void {
//     this.quoteForm.get('zip')?.valueChanges.subscribe(() => {
//       this.zipVerified = false;
//       this.zipValidationError = '';
//       this.zipLocationMessage = '';
//     });

//     this.quoteForm.get('email')?.valueChanges.subscribe(() => {
//       this.emailValidationError = '';
//     });
//   }

//   private stepControls(stepNumber: number): string[] {
//     const current = this.steps[stepNumber - 1];

//     switch (current?.type) {
//       case 'intro':
//         return ['zip'];

//       case 'buttons':
//         return current.field ? [current.field] : [];

//       case 'form':
//       case 'location':
//         return (current.fields || []).map(field => field.key);

//       case 'contact':
//         return ['phone', 'bestTimeToCall', 'email'];

//       default:
//         return [];
//     }
//   }

//   private stepAbstractControls(
//     stepNumber: number
//   ): AbstractControl[] {
//     return this.stepControls(stepNumber)
//       .map(name => this.quoteForm.get(name))
//       .filter(
//         (control): control is AbstractControl => !!control
//       );
//   }

//   private validateStep(stepNumber: number): boolean {
//     const controls =
//       this.stepAbstractControls(stepNumber);

//     controls.forEach(control => {
//       control.markAsTouched();
//       control.updateValueAndValidity({
//         onlySelf: true
//       });
//     });

//     return controls.every(control => control.valid);
//   }

//   private control(name: string): AbstractControl | null {
//     return this.quoteForm?.get(name) ?? null;
//   }

//   isInvalid(name: string): boolean {
//     const control = this.control(name);

//     return !!control &&
//       control.invalid &&
//       (control.touched || control.dirty);
//   }

//   getError(name: string, label?: string): string {
//     const control = this.control(name);

//     if (
//       !control?.errors ||
//       !(control.touched || control.dirty)
//     ) {
//       return '';
//     }

//     const errors = control.errors;
//     const title =
//       label || this.labels[name] || 'This field';

//     const custom =
//       this.customMessages[name] || {};

//     if (errors['required']) {
//       return custom['required'] ||
//         `${title} is required`;
//     }

//     if (errors['minlength']) {
//       return custom['minlength'] ||
//         `${title} must be at least ` +
//         `${errors['minlength'].requiredLength} characters`;
//     }

//     if (errors['maxlength']) {
//       return custom['maxlength'] ||
//         `${title} cannot be longer than ` +
//         `${errors['maxlength'].requiredLength} characters`;
//     }

//     if (errors['email'] || errors['pattern']) {
//       return custom['pattern'] ||
//         `Enter a valid ${title.toLowerCase()}`;
//     }

//     return `${title} is not valid`;
//   }

//   async nextStep(): Promise<void> {
//     this.submitError = '';

//     if (!this.validateStep(this.step)) {
//       return;
//     }

//     const currentType = this.activeStep?.type;

//     if (currentType === 'intro') {
//       const zipIsValid = await this.verifyZipCode();

//       if (!zipIsValid) {
//         return;
//       }
//     }

//     if (this.step < this.totalSteps) {
//       this.step++;
//     }
//   }

//   previousStep(): void {
//     if (
//       this.zipChecking ||
//       this.emailChecking ||
//       this.isSubmitting
//     ) {
//       return;
//     }

//     if (this.step > 1) {
//       this.step--;
//     }
//   }

//   setValue(field: string, value: any): void {
//     const control = this.control(field);

//     if (control) {
//       control.setValue(value);
//       control.markAsTouched();
//       control.markAsDirty();
//     }
//   }

//   selectOption(
//     field: string | undefined,
//     value: string
//   ): void {
//     if (!field) {
//       return;
//     }

//     this.setValue(field, value);
//     void this.nextStep();
//   }

//   private verifyZipCode(): Promise<boolean> {
//     this.zipValidationError = '';
//     this.zipLocationMessage = '';

//     if (this.zipVerified) {
//       return Promise.resolve(true);
//     }

//     const zip =
//       String(this.quoteForm.get('zip')?.value || '').trim();

//     if (!/^\d{5}$/.test(zip)) {
//       this.zipValidationError =
//         'Enter a valid 5-digit US ZIP code.';

//       return Promise.resolve(false);
//     }

//     this.zipChecking = true;

//     /*
//      * The existing address-validation API is reused here.
//      * Passing the ZIP as the address allows Google Geocoding
//      * to confirm that the ZIP exists in the United States.
//      */
//     return new Promise<boolean>(resolve => {
//       this.websiteLeadService
//         .validateAddress(zip, zip)
//         .subscribe({
//           next: response => {
//             this.zipChecking = false;

//             const result =
//               response?.result || response;

//             const accepted =
//               result?.isValid === true ||
//               String(result?.status || '')
//                 .toLowerCase() === 'valid' ||
//               String(result?.possibleNextAction || '')
//                 .toUpperCase() === 'ACCEPT';

//             if (!accepted) {
//               this.zipVerified = false;
//               this.zipValidationError =
//                 result?.message ||
//                 'This ZIP code could not be verified.';

//               resolve(false);
//               return;
//             }

//             this.zipVerified = true;

//             this.validatedPostcode =
//               result?.postalCode || zip;

//             this.validatedCity =
//               result?.city || '';

//             this.validatedState =
//               result?.state || '';

//             this.validatedCountry =
//               result?.country || 'United States';

//             this.zipLocationMessage =
//               this.buildLocationMessage(result);

//             resolve(true);
//           },

//           error: error => {
//             this.zipChecking = false;
//             this.zipVerified = false;

//             this.zipValidationError =
//               error?.error?.result?.message ||
//               error?.error?.message ||
//               'ZIP code verification failed. Please try again.';

//             resolve(false);
//           }
//         });
//     });
//   }

//   private verifyEmail(): Promise<boolean> {
//     this.emailValidationError = '';

//     const email =
//       String(
//         this.quoteForm.get('email')?.value || ''
//       ).trim();

//     this.emailChecking = true;

//     return new Promise<boolean>(resolve => {
//       this.websiteLeadService
//         .validateEmail(email)
//         .subscribe({
//           next: response => {
//             this.emailChecking = false;

//             const result =
//               response?.result || response;

//             const status =
//               String(result?.status || '')
//                 .toLowerCase();

//             const accepted =
//               result?.deliverable === true ||
//               status === 'deliverable';

//             if (accepted) {
//               resolve(true);
//               return;
//             }

//             this.emailValidationError =
//               result?.reason
//                 ? `Email verification failed: ${result.reason}`
//                 : 'Please enter a valid, deliverable email address.';

//             resolve(false);
//           },

//           error: error => {
//             this.emailChecking = false;

//             const providerMessage =
//               error?.error?.result?.reason ||
//               error?.error?.message ||
//               error?.error?.error;

//             this.emailValidationError =
//               providerMessage
//                 ? `Email verification failed: ${providerMessage}`
//                 : 'Email verification failed. Please try again.';

//             resolve(false);
//           }
//         });
//     });
//   }

//   private firstInvalidStep(): number {
//     for (
//       let stepNumber = 1;
//       stepNumber <= this.totalSteps;
//       stepNumber++
//     ) {
//       if (
//         this.stepAbstractControls(stepNumber)
//           .some(control => control.invalid)
//       ) {
//         return stepNumber;
//       }
//     }

//     return 0;
//   }

//   async submitForm(): Promise<void> {
//     this.submitError = '';
//     this.emailValidationError = '';

//     this.quoteForm.markAllAsTouched();

//     if (this.quoteForm.invalid) {
//       const invalidStep =
//         this.firstInvalidStep();

//       if (invalidStep) {
//         this.step = invalidStep;
//       }

//       return;
//     }

//     if (
//       this.isSubmitting ||
//       this.zipChecking ||
//       this.emailChecking
//     ) {
//       return;
//     }

//     if (!this.zipVerified) {
//       const zipIsValid =
//         await this.verifyZipCode();

//       if (!zipIsValid) {
//         this.step = 1;
//         return;
//       }
//     }

//     const emailIsValid =
//       await this.verifyEmail();

//     if (!emailIsValid) {
//       return;
//     }

//     await this.saveLead();
//   }

//   private async saveLead(): Promise<void> {
//     const value =
//       this.quoteForm.getRawValue();

//     const trustedFormCertificateUrl =
//       await this.websiteLeadService.waitForTrustedFormCertificateUrl();

//     this.isSubmitting = true;
//     this.submitError = '';

//     this.websiteLeadService.createLead({
//       fullName:
//         `${value.firstName} ${value.lastName}`.trim(),

//       email:
//         String(value.email || '').trim(),

//       phone:
//         String(value.phone || '').trim(),

//       serviceCode:
//         this.serviceCode,

//       campaignName:
//         this.service_name,

//       pageName:
//         `Homeyy Services ${this.service_name} Quote Form`,

//       address:
//         String(value.address || '').trim(),

//       postcode:
//         this.validatedPostcode ||
//         String(value.zip || '').trim(),

//       city:
//         this.validatedCity || undefined,

//       state:
//         this.validatedState || undefined,

//       country:
//         this.validatedCountry ||
//         'United States',

//       countryCode: 'US',

//       step: this.totalSteps,

//       isTest: false,
//       isCompleted: true,

//       affiliateSubId: null,

//       fingerprintHash:
//         this.generateFingerprint(),

//       landingPageUrl:
//         window.location.href,

//       isTcpaCompliant: true,

//       trustedFormCertificateUrl,

//       ownsProperty:
//         String(value.homeOwner || '')
//           .trim()
//           .toLowerCase() === 'yes',

//       additionalDataJson:
//         JSON.stringify({
//           formType: 'ServicesQuote',
//           routeType: '/services/:service',
//           serviceCode: this.serviceCode,
//           serviceName: this.service_name,
//           purchaseTimeFrame:
//             value.purchaseTimeFrame || null,
//           bestTimeToCall:
//             value.bestTimeToCall || null,
//           answers: value
//         })
//     }).subscribe({
//       next: () => {
//         this.isSubmitting = false;

//         this.router.navigate([
//           '/',
//           this.path_name,
//           'thank-you'
//         ]);
//       },

//       error: error => {
//         this.isSubmitting = false;

//         this.submitError =
//           error?.error?.message ||
//           'We could not submit your request. Please try again.';
//       }
//     });
//   }

//   private buildLocationMessage(result: any): string {
//     return [
//       result?.city,
//       result?.state,
//       result?.postalCode
//     ]
//       .filter(Boolean)
//       .join(', ');
//   }

//   private generateFingerprint(): string {
//     const source = [
//       navigator.userAgent || '',
//       navigator.language || '',
//       screen.width,
//       screen.height,
//       new Date().getTimezoneOffset()
//     ].join('|');

//     let hash = 0;

//     for (let index = 0; index < source.length; index++) {
//       hash =
//         ((hash << 5) - hash) +
//         source.charCodeAt(index);

//       hash |= 0;
//     }

//     return Math.abs(hash).toString();
//   }
// }


 