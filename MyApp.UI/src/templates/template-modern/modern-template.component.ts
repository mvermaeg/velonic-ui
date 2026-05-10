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

  activePage: any = null
  navItems: any[] = []

  leadForm: any = {
    fullName: '',
    phone: '',
    email: '',
    postcode: '',
    message: '',
  }

  features = [
    {
      icon: '<i class="fa-solid fa-award"></i>',
      title: 'Certified Experts',
      desc: 'Professional and trusted service experts.',
    },
    {
      icon: '<i class="fa-solid fa-clock"></i>',
      title: 'Fast Response',
      desc: 'Quick support and emergency availability.',
    },
    {
      icon: '<i class="fa-solid fa-shield-halved"></i>',
      title: 'Reliable Service',
      desc: 'Quality-focused and customer-first approach.',
    },
  ]

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['site'] && this.site) {
      this.preparePages()
    }
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

    const requestedPageSlug = this.site?.activePageSlug

    this.activePage =
      pages.find((p: any) => p.pageSlug === requestedPageSlug) ||
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

    const url =
      pageSlug === 'home'
        ? `/w/${this.site.slug}`
        : `/w/${this.site.slug}/${pageSlug}`

    this.router.navigateByUrl(url)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  toggleNavbar() {
    this.isCollapsed = !this.isCollapsed
  }

  get isHomePage() {
    return this.activePage?.pageSlug === 'home'
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
    try {
      const raw = this.activeForm?.settingsJson

      if (!raw) {
        return ['fullName', 'phone', 'email', 'postcode', 'message']
      }

      const parsed = JSON.parse(raw)

      if (Array.isArray(parsed?.fields) && parsed.fields.length > 0) {
        return parsed.fields
      }

      return ['fullName', 'phone', 'email', 'postcode', 'message']
    } catch {
      return ['fullName', 'phone', 'email', 'postcode', 'message']
    }
  }

  showField(fieldName: string): boolean {
    return this.formFields.includes(fieldName)
  }

  submitLead() {
    this.successMessage = ''
    this.errorMessage = ''

    if (!this.leadForm.fullName || !this.leadForm.phone) {
      this.errorMessage = 'Name and phone are required.'
      return
    }

    const payload = {
      fullName: this.leadForm.fullName,
      phone: this.leadForm.phone,
      email: this.leadForm.email,
      postcode: this.leadForm.postcode,
      message: this.leadForm.message,
      pageName: this.activePage?.pageName || 'Home',
      pageSlug: this.activePage?.pageSlug || this.site?.slug,
      sourceName: this.formSourceName,
      campaignName: this.formCampaignName,
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
          postcode: '',
          message: '',
        }

        this.setActivePage('thank-you')
      },
      error: (err) => {
        this.submitting = false
        this.errorMessage = err?.error?.message || 'Unable to submit enquiry.'
      },
    })
  }
}

 