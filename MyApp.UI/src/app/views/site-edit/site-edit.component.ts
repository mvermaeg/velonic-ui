import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { HttpClient } from '@angular/common/http'
import { PageTitleComponent } from '@common/page-title.component'
import { environment } from '../../../environments/environment'

@Component({
  selector: 'app-site-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './site-edit.component.html',
  styleUrl: './site-edit.component.scss',
})
export class SiteEditComponent implements OnInit {
  private route = inject(ActivatedRoute)
  private router = inject(Router)
  private http = inject(HttpClient)

  private sitesApi = `${environment.apiUrl}/sites`
  private templatesApi = `${environment.apiUrl}/websitetemplates`
  private themesApi = `${environment.apiUrl}/websitethemes`

  siteId: number | null = null
  site: any = null

  templates: any[] = []
  themes: any[] = []

  loading = false
  saving = false
  successMessage = ''
  errorMessage = ''

  form: any = {
    siteName: '',
    domainName: '',
    slug: '',
    themeKey: '',
    websiteTemplateId: null,
    websiteThemeId: null,
    isActive: true,
  }

  ngOnInit(): void {
    this.siteId = Number(this.route.snapshot.paramMap.get('id'))

    if (!this.siteId) {
      this.errorMessage = 'Invalid site.'
      return
    }

    this.loadLookups()
    this.loadSite()
  }

  loadLookups() {
    this.http.get<any[]>(this.templatesApi).subscribe({
      next: (res) => {
        this.templates = res || []
      },
      error: () => {
        this.errorMessage = 'Unable to load templates.'
      },
    })

    this.http.get<any[]>(this.themesApi).subscribe({
      next: (res) => {
        this.themes = res || []
      },
      error: () => {
        this.errorMessage = 'Unable to load themes.'
      },
    })
  }

  loadSite() {
    this.loading = true
    this.errorMessage = ''

    this.http.get<any[]>(this.sitesApi).subscribe({
      next: (res) => {
        this.site = (res || []).find((x) => Number(x.id) === Number(this.siteId))

        if (!this.site) {
          this.errorMessage = 'Site not found.'
          this.loading = false
          return
        }

        this.form = {
          siteName: this.site.siteName || '',
          domainName: this.site.domainName || '',
          slug: this.site.slug || '',
          themeKey: this.site.themeKey || '',
          websiteTemplateId: this.site.websiteTemplateId || null,
          websiteThemeId: this.site.websiteThemeId || null,
          isActive: this.site.isActive,
        }

        this.loading = false
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Unable to load site.'
      },
    })
  }

  onThemeChange() {
    const selectedTheme = this.themes.find(
      (x) => Number(x.id) === Number(this.form.websiteThemeId)
    )

    if (selectedTheme?.themeKey) {
      this.form.themeKey = selectedTheme.themeKey
    }
  }

  save() {
    this.successMessage = ''
    this.errorMessage = ''

    if (!this.siteId) return

    if (!this.form.siteName?.trim()) {
      this.errorMessage = 'Site name is required.'
      return
    }

    if (!this.form.slug?.trim()) {
      this.errorMessage = 'Slug is required.'
      return
    }

    if (!this.form.websiteTemplateId) {
      this.errorMessage = 'Please select website template.'
      return
    }

    if (!this.form.websiteThemeId) {
      this.errorMessage = 'Please select website theme.'
      return
    }

    this.saving = true

    const payload = {
      siteName: this.form.siteName,
      domainName: this.form.domainName,
      slug: this.form.slug,
      themeKey: this.form.themeKey,
      websiteTemplateId: Number(this.form.websiteTemplateId),
      websiteThemeId: Number(this.form.websiteThemeId),
      isActive: this.form.isActive,
    }

    this.http.put(`${this.sitesApi}/${this.siteId}`, payload).subscribe({
      next: () => {
        this.saving = false
        this.successMessage = 'Site updated successfully.'
        this.loadSite()
      },
      error: (err) => {
        this.saving = false
        this.errorMessage = err?.error?.message || 'Unable to update site.'
      },
    })
  }

  openPreview() {
    if (!this.form.slug) return
    this.router.navigateByUrl(`/w/${this.form.slug}`)
  }

  goBack() {
    this.router.navigateByUrl('/sites')
  }
}