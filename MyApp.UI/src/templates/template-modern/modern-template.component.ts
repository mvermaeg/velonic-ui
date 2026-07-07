import { CommonModule } from '@angular/common'
import { Component, Input, OnChanges, SimpleChanges, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { HttpClient } from '@angular/common/http'
import { Router } from '@angular/router'
import { environment } from '../../environments/environment'

@Component({
  selector: 'app-modern-template',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './modern-template.component.html',
  styleUrl: './modern-template.component.scss',
})
export class ModernTemplateComponent implements OnChanges {
  private http = inject(HttpClient)
  private router = inject(Router)

  @Input() site: any

  isCollapsed = false
  submitting = false
  successMessage = ''
  errorMessage = ''

  addressPostcodeError = ''
  addressPostcodeVerified = false
  addressPostcodeChecking = false
  expectedLocation: any = null

  activePage: any = null
  navItems: any[] = []

  leadForm: any = {
    fullName: '',
    phone: '',
    email: '',
    leadTypeId: null,
    postcode: '',
    address: '',
    city: '',
    state: '',
    country: '',
    countryCode: 'US',
    message: '',
  }

  leadTypes = [
    { id: 1, name: 'Roofing' },
    { id: 2, name: 'Windows' },
    { id: 3, name: 'Siding' },
    { id: 4, name: 'HVAC' },
    { id: 5, name: 'Bathroom' },
  ]

  features = [
    { icon: '<i class="fa-solid fa-award"></i>', title: 'Certified Experts', desc: 'Professional and trusted service experts.' },
    { icon: '<i class="fa-solid fa-clock"></i>', title: 'Fast Response', desc: 'Quick support and emergency availability.' },
    { icon: '<i class="fa-solid fa-shield-halved"></i>', title: 'Reliable Service', desc: 'Quality-focused and customer-first approach.' },
  ]

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['site'] && this.site) {
      this.preparePages()
    }
  }
private getApiErrorMessage(err: any): string {
  const apiError = err?.error

  if (typeof apiError === 'string') return apiError

  return (
    apiError?.message ||
    apiError?.error ||
    apiError?.innerMessage ||
    apiError?.emailReason ||
    apiError?.addressCheck?.message ||
    err?.message ||
    'Unable to submit enquiry.'
  )
}
  preparePages() {
    const pages = this.site?.pages || []

    this.navItems = pages
      .filter((p: any) => p.isActive !== false)
      .sort((a: any, b: any) => (a.sortOrder || 0) - (b.sortOrder || 0))
      .map((p: any) => ({
        label: p.pageName,
        slug: p.pageSlug,
      }))

    this.activePage =
      pages.find((p: any) => p.isHomePage) ||
      pages.find((p: any) => p.pageSlug === 'home') ||
      pages[0] ||
      null
  }

  setActivePage(pageSlug: string) {
    const page = this.site?.pages?.find((p: any) => p.pageSlug === pageSlug)
    if (!page) return

    this.activePage = page
    this.isCollapsed = false

    if (this.site?.slug) {
      const url = pageSlug === 'home'
        ? `/w/${this.site.slug}`
        : `/w/${this.site.slug}/${pageSlug}`

      this.router.navigateByUrl(url)
    }

    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  toggleNavbar() {
    this.isCollapsed = !this.isCollapsed
  }

  get isHomePage() {
    return this.activePage?.pageSlug === 'home'
  }

  get hasCustomHtmlContent() {
    return !!this.activePage?.htmlContent?.trim()
  }

  get isAboutPage() {
    return this.activePage?.pageSlug === 'about'
  }

  get isServicesPage() {
    return this.activePage?.pageSlug === 'services'
  }

  get isContactPage() {
    return this.activePage?.pageSlug === 'contact'
  }

  get isThankYouPage() {
    return this.activePage?.pageSlug === 'thank-you'
  }

  get activeSections() {
    return this.activePage?.sections || []
  }

  get settings() {
    return this.site?.settings || {}
  }

  get logoUrl() {
    return this.settings.logoUrl || '/assets/templates/template-modern/images/logo.png'
  }

  get phoneNumber() {
    return this.settings.phoneNumber || '+1 (712) 724-8001'
  }

  get emailAddress() {
    return this.settings.emailAddress || 'info@bcgnj.com'
  }

  get addressLine1() {
    return this.settings.addressLine1 || '15 Corporate Pl S Suite #210'
  }

  get addressLine2() {
    return this.settings.addressLine2 || 'Piscataway, NJ, United States'
  }

  get businessHours() {
    return this.settings.businessHours || 'Mon - Sat : 9am to 7pm Sunday is CLOSED'
  }

  get facebookUrl() {
    return this.settings.facebookUrl || ''
  }

  get linkedinUrl() {
    return this.settings.linkedinUrl || ''
  }

  get instagramUrl() {
    return this.settings.instagramUrl || ''
  }

  get twitterUrl() {
    return this.settings.twitterUrl || ''
  }

  get youtubeUrl() {
    return this.settings.youtubeUrl || ''
  }

  get activeForm() {
    const forms = this.site?.forms || []
    const pageId = this.activePage?.id

    return (
      forms.find((f: any) => f.sitePageId === pageId && f.isActive !== false) ||
      forms.find((f: any) => !f.sitePageId && f.isActive !== false) ||
      null
    )
  }

  get formTitle() {
    return this.activeForm?.displayName || 'Request A Free Service Quote'
  }

  get formButtonText() {
    return this.activeForm?.submitButtonText || 'Submit Enquiry'
  }

  get formSuccessMessage() {
    return this.activeForm?.successMessage || 'Thank you. Your enquiry has been submitted.'
  }

  get formCampaignName() {
    return this.activeForm?.campaignName || this.site?.siteName || ''
  }

  get formSourceName() {
    return this.activeForm?.sourceName || 'Website'
  }

  get formFields(): string[] {
    const defaultFields = ['fullName', 'phone', 'email', 'leadTypeId', 'address', 'postcode', 'message']

    try {
      const raw = this.activeForm?.settingsJson
      if (!raw) return defaultFields

      const parsed = JSON.parse(raw)

      if (Array.isArray(parsed?.fields) && parsed.fields.length > 0) {
        const fields = [...parsed.fields]

        if (!fields.includes('leadTypeId')) {
          fields.splice(3, 0, 'leadTypeId')
        }

        if (fields.includes('postcode') && !fields.includes('address')) {
          fields.splice(fields.indexOf('postcode'), 0, 'address')
        }

        return fields
      }

      return defaultFields
    } catch {
      return defaultFields
    }
  }

  showField(fieldName: string): boolean {
    return this.formFields.includes(fieldName)
  }

  resetAddressPostcodeState() {
    this.addressPostcodeError = ''
    this.addressPostcodeVerified = false
    this.addressPostcodeChecking = false
    this.expectedLocation = null

    this.leadForm.city = ''
    this.leadForm.state = ''
    this.leadForm.country = ''
  }

  checkAddressPostcode() {
    this.addressPostcodeError = ''
    this.addressPostcodeVerified = false
    this.expectedLocation = null

    const address = this.leadForm?.address?.trim()
    const postcode = this.leadForm?.postcode?.trim()

    if (!address || !postcode) return

    this.addressPostcodeChecking = true

    this.http.post<any>(`${environment.apiUrl}/diagnostics/address`, {
      address,
      postcode,
      countryCode: this.leadForm.countryCode || 'IN',
    }).subscribe({
      next: (res) => {
        this.addressPostcodeChecking = false

        const result = res?.result || res

        if (result?.isValid === true || result?.status === 'Valid' || result?.possibleNextAction === 'ACCEPT') {
          this.addressPostcodeVerified = true
          this.expectedLocation = result

          this.leadForm.city = result.city || ''
          this.leadForm.state = result.state || ''
          this.leadForm.country = result.country || ''
          this.leadForm.postcode = result.postalCode || this.leadForm.postcode
        } else {
          this.addressPostcodeVerified = false
          this.addressPostcodeError = result?.message || 'Address verification failed.'
        }
      },
      error: (err) => {
        this.addressPostcodeChecking = false
        this.addressPostcodeVerified = false
        this.addressPostcodeError =
          err?.error?.message ||
          err?.error?.result?.message ||
          'Address verification failed.'
      },
    })
  }

  allowOnlyNumbers(event: KeyboardEvent) {
    if (!/^[0-9]$/.test(event.key)) {
      event.preventDefault()
    }
  }

  generateFingerprint(): string {
    const data = [
      navigator.userAgent,
      navigator.language,
      screen.width,
      screen.height,
      screen.colorDepth,
      Intl.DateTimeFormat().resolvedOptions().timeZone,
    ].join('|')

    let hash = 0

    for (let i = 0; i < data.length; i++) {
      hash = ((hash << 5) - hash) + data.charCodeAt(i)
      hash |= 0
    }

    return Math.abs(hash).toString()
  }

  validateEmailWithBouncer(email: string): Promise<boolean> {
  this.errorMessage = ''

  return new Promise((resolve) => {
    this.http
      .get<any>(`${environment.apiUrl}/diagnostics/email?email=${encodeURIComponent(email)}`)
      .subscribe({
        next: (res) => {
          const result = res?.result

          if (
            result?.deliverable === true &&
            String(result?.status || '').toLowerCase() === 'deliverable'
          ) {
            resolve(true)
            return
          }

          this.errorMessage =
            `Email verification failed. Status: ${result?.status || 'Unknown'}, Reason: ${result?.reason || 'Email is not deliverable.'}`

          resolve(false)
        },
        error: (err) => {
          this.errorMessage =
            err?.error?.message ||
            err?.error?.error ||
            'Email verification failed. Please enter a valid working email address.'

          resolve(false)
        },
      })
  })
}


 async submitLead() {
    this.successMessage = ''
    this.errorMessage = ''

    const phoneOnlyDigits = /^[0-9]{7,15}$/
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

    if (!this.leadForm.fullName?.trim()) {
      this.errorMessage = 'Full name is required.'
      return
    }

    if (!this.leadForm.phone?.trim()) {
      this.errorMessage = 'Phone number is required.'
      return
    }

    if (!phoneOnlyDigits.test(this.leadForm.phone || '')) {
      this.errorMessage = 'Phone number must contain numbers only, 7 to 15 digits.'
      return
    }

    if (!this.leadForm.email?.trim()) {
      this.errorMessage = 'Email address is required.'
      return
    }

    if (!emailPattern.test(this.leadForm.email)) {
      this.errorMessage = 'Please enter a valid email address.'
      return
    }
const emailVerified = await this.validateEmailWithBouncer(this.leadForm.email)

if (!emailVerified) {
  return
}
    if (!this.leadForm.leadTypeId) {
      this.errorMessage = 'Please select a service.'
      return
    }

    if (!this.leadForm.address?.trim()) {
      this.errorMessage = 'Full address is required.'
      return
    }

    if (!this.leadForm.postcode?.trim()) {
      this.errorMessage = 'Postcode is required.'
      return
    }

    const selectedLeadType = this.leadTypes.find(
      (x) => x.id === Number(this.leadForm.leadTypeId)
    )

    const payload = {
      fullName: this.leadForm.fullName,
      phone: this.leadForm.phone,
      email: this.leadForm.email,
      leadTypeId: this.leadForm.leadTypeId,
      fingerprintHash: this.generateFingerprint(),

      postcode: this.leadForm.postcode,
      address: this.leadForm.address,
      city: this.leadForm.city,
      state: this.leadForm.state,
      country: this.leadForm.country,
      countryCode: this.leadForm.countryCode || 'IN',

      message: this.leadForm.message,
      pageName: this.activePage?.pageName || 'Home',
      pageSlug: this.activePage?.pageSlug || this.site?.slug,
      sourceName: this.formSourceName,
      campaignName: selectedLeadType?.name || this.formCampaignName,
      siteId: this.site?.id,
    }

    this.submitting = true

    this.http.post<any>(`${environment.apiUrl}/public/leads/website`, payload).subscribe({
      next: () => {
        this.submitting = false
        this.successMessage = this.formSuccessMessage

        this.leadForm = {
          fullName: '',
          phone: '',
          email: '',
          leadTypeId: null,
          postcode: '',
          address: '',
          city: '',
          state: '',
          country: '',
          countryCode: 'IN',
          message: '',
        }

        this.resetAddressPostcodeState()
        this.setActivePage('thank-you')
      },
      error: (err) => {
        this.submitting = false

        const apiError = err?.error

        this.errorMessage =
          apiError?.message ||
          apiError?.error ||
          apiError?.innerMessage ||
          apiError?.emailReason ||
          apiError?.addressCheck?.message ||
          err?.message ||
          'Unable to submit enquiry.'
      },
    })
  }
}