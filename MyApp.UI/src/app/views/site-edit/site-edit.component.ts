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

  siteId: number | null = null
  site: any = null

  loading = false
  saving = false
  successMessage = ''
  errorMessage = ''

  form: any = {
    siteName: '',
    domainName: '',
    slug: '',
    themeKey: 'theme-1',
    isActive: true,
  }

  ngOnInit(): void {
    this.siteId = Number(this.route.snapshot.paramMap.get('id'))

    if (!this.siteId) {
      this.errorMessage = 'Invalid site.'
      return
    }

    this.loadSite()
  }

  loadSite() {
    this.loading = true

    this.http.get<any[]>(this.sitesApi).subscribe({
      next: (res) => {
        this.site = (res || []).find((x) => Number(x.id) === Number(this.siteId))

        if (!this.site) {
          this.errorMessage = 'Site not found.'
          this.loading = false
          return
        }

        this.form = {
          siteName: this.site.siteName,
          domainName: this.site.domainName,
          slug: this.site.slug,
          themeKey: this.site.themeKey,
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

  save() {
    this.successMessage = ''
    this.errorMessage = ''

    if (!this.siteId) return

    this.saving = true

    this.http.put(`${this.sitesApi}/${this.siteId}`, this.form).subscribe({
      next: () => {
        this.saving = false
        this.successMessage = 'Site updated successfully.'
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