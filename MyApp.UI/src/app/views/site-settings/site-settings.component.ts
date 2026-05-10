import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { HttpClient } from '@angular/common/http'
import { PageTitleComponent } from '@common/page-title.component'
import { environment } from '../../../environments/environment'

@Component({
  selector: 'app-site-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './site-settings.component.html',
  styleUrl: './site-settings.component.scss',
})
export class SiteSettingsComponent implements OnInit {
  private http = inject(HttpClient)

  private sitesApi = `${environment.apiUrl}/sites`
  private settingsApi = `${environment.apiUrl}/sitesettings`

  sites: any[] = []
  selectedSiteId: number | null = null

  loading = false
  saving = false
  successMessage = ''
  errorMessage = ''

  form: any = {
    siteId: null,
    logoUrl: '',
    phoneNumber: '',
    emailAddress: '',
    addressLine1: '',
    addressLine2: '',
    businessHours: '',
    facebookUrl: '',
    linkedinUrl: '',
    instagramUrl: '',
    twitterUrl: '',
    youtubeUrl: '',
    isActive: true,
  }

  ngOnInit(): void {
    this.loadSites()
  }

  loadSites() {
    this.http.get<any[]>(this.sitesApi).subscribe({
      next: (res) => {
        this.sites = res || []
      },
      error: () => {
        this.errorMessage = 'Unable to load sites.'
      },
    })
  }

  onSiteChange() {
    this.successMessage = ''
    this.errorMessage = ''

    if (!this.selectedSiteId) {
      this.resetForm()
      return
    }

    this.loadSettings()
  }

  loadSettings() {
    if (!this.selectedSiteId) return

    this.loading = true

    this.http.get<any>(`${this.settingsApi}/by-site/${this.selectedSiteId}`).subscribe({
      next: (res) => {
        this.loading = false

        this.form = {
          siteId: this.selectedSiteId,
          logoUrl: res?.logoUrl || '/assets/templates/template-modern/images/logo.png',
          phoneNumber: res?.phoneNumber || '',
          emailAddress: res?.emailAddress || '',
          addressLine1: res?.addressLine1 || '',
          addressLine2: res?.addressLine2 || '',
          businessHours: res?.businessHours || '',
          facebookUrl: res?.facebookUrl || '',
          linkedinUrl: res?.linkedinUrl || '',
          instagramUrl: res?.instagramUrl || '',
          twitterUrl: res?.twitterUrl || '',
          youtubeUrl: res?.youtubeUrl || '',
          isActive: res?.isActive ?? true,
        }
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Unable to load site settings.'
      },
    })
  }

  saveSettings() {
    this.successMessage = ''
    this.errorMessage = ''

    if (!this.selectedSiteId) {
      this.errorMessage = 'Please select site.'
      return
    }

    this.form.siteId = this.selectedSiteId
    this.saving = true

    this.http.post<any>(`${this.settingsApi}/save`, this.form).subscribe({
      next: (res) => {
        this.saving = false
        this.successMessage = res?.message || 'Site settings saved successfully.'
      },
      error: (err) => {
        this.saving = false
        this.errorMessage = err?.error?.message || 'Unable to save settings.'
      },
    })
  }

  resetForm() {
    this.form = {
      siteId: null,
      logoUrl: '',
      phoneNumber: '',
      emailAddress: '',
      addressLine1: '',
      addressLine2: '',
      businessHours: '',
      facebookUrl: '',
      linkedinUrl: '',
      instagramUrl: '',
      twitterUrl: '',
      youtubeUrl: '',
      isActive: true,
    }
  }
}