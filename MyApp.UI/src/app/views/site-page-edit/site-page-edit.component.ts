import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { ActivatedRoute, Router } from '@angular/router'
import { HttpClient } from '@angular/common/http'
import { PageTitleComponent } from '@common/page-title.component'
import { environment } from '../../../environments/environment'

@Component({
  selector: 'app-site-page-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './site-page-edit.component.html',
  styleUrl: './site-page-edit.component.scss',
})
export class SitePageEditComponent implements OnInit {
  private route = inject(ActivatedRoute)
  private router = inject(Router)
  private http = inject(HttpClient)

  private pagesApi = `${environment.apiUrl}/sitepages`
  private mediaApi = `${environment.apiUrl}/sitemediafiles`

  pageId: number | null = null
  siteId: number | null = null

  mediaFiles: any[] = []

  loading = false
  mediaLoading = false
  saving = false

  successMessage = ''
  errorMessage = ''

  form: any = {
    siteId: null,
    pageName: '',
    pageSlug: '',
    pageTitle: '',
    metaTitle: '',
    metaDescription: '',
    heroTitle: '',
    heroSubtitle: '',
    htmlContent: '',
    jsonContent: '',
    sortOrder: 0,
    isHomePage: false,
    isActive: true,
  }

  ngOnInit(): void {
    this.pageId = Number(this.route.snapshot.paramMap.get('id'))

    if (!this.pageId) {
      this.errorMessage = 'Invalid page.'
      return
    }

    this.loadPage()
  }

  loadPage() {
    this.loading = true
    this.errorMessage = ''

    this.http.get<any[]>(this.pagesApi).subscribe({
      next: (res) => {
        const page = (res || []).find((x) => Number(x.id) === Number(this.pageId))

        if (!page) {
          this.loading = false
          this.errorMessage = 'Page not found.'
          return
        }

        this.siteId = page.siteId

        this.form = {
          siteId: page.siteId,
          pageName: page.pageName || '',
          pageSlug: page.pageSlug || '',
          pageTitle: page.pageTitle || '',
          metaTitle: page.metaTitle || '',
          metaDescription: page.metaDescription || '',
          heroTitle: page.heroTitle || '',
          heroSubtitle: page.heroSubtitle || '',
          htmlContent: page.htmlContent || '',
          jsonContent: page.jsonContent || '',
          sortOrder: page.sortOrder || 0,
          isHomePage: page.isHomePage,
          isActive: page.isActive,
        }

        this.loading = false
        this.loadMedia()
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Unable to load page.'
      },
    })
  }

  loadMedia() {
    if (!this.siteId) return

    this.mediaLoading = true

    this.http.get<any[]>(`${this.mediaApi}/by-site/${this.siteId}`).subscribe({
      next: (res) => {
        this.mediaFiles = res || []
        this.mediaLoading = false
      },
      error: () => {
        this.mediaLoading = false
      },
    })
  }

  getFullUrl(fileUrl: string) {
    if (!fileUrl) return ''
    if (fileUrl.startsWith('http')) return fileUrl

    const apiRoot = environment.apiUrl.replace('/api', '')
    return `${apiRoot}${fileUrl}`
  }



  openPreview() {
  if (!this.siteId || !this.form.pageSlug) return

  this.http.get<any[]>(`${environment.apiUrl}/sites`).subscribe({
    next: (sites) => {
      const site = (sites || []).find((x) => Number(x.id) === Number(this.siteId))

      if (!site) {
        this.errorMessage = 'Site not found for preview.'
        return
      }

      const url =
        this.form.pageSlug === 'home'
          ? `/w/${site.slug}`
          : `/w/${site.slug}/${this.form.pageSlug}`

      window.open(url, '_blank')
    },
    error: () => {
      this.errorMessage = 'Unable to open preview.'
    },
  })
}


  copyUrl(fileUrl: string) {
    const fullUrl = this.getFullUrl(fileUrl)

    navigator.clipboard.writeText(fullUrl).then(() => {
      this.successMessage = 'Media URL copied.'
    })
  }

  insertImageHtml(fileUrl: string) {
    const fullUrl = this.getFullUrl(fileUrl)

    const html =
      `<p><img src="${fullUrl}" alt="" style="max-width:100%;height:auto;border-radius:14px;" /></p>`

    this.form.htmlContent = (this.form.htmlContent || '') + '\n' + html
    this.successMessage = 'Image HTML inserted into page content.'
  }

  isImage(item: any) {
    return item.fileType === 'image' || item.mimeType?.startsWith('image/')
  }

  save() {
    if (!this.pageId) return

    this.saving = true
    this.successMessage = ''
    this.errorMessage = ''

    this.http.put(`${this.pagesApi}/${this.pageId}`, this.form).subscribe({
      next: () => {
        this.saving = false
        this.successMessage = 'Page updated successfully.'
      },
      error: (err) => {
        this.saving = false
        this.errorMessage = err?.error?.message || 'Unable to update page.'
      },
    })
  }

  goBack() {
    this.router.navigateByUrl('/sites/pages')
  }
}