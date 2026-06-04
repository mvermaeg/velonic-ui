import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { HttpClient } from '@angular/common/http'
import { PageTitleComponent } from '@common/page-title.component'
import { environment } from '../../../environments/environment'

@Component({
  selector: 'app-site-page-sections',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './site-page-sections.component.html',
  styleUrl: './site-page-sections.component.scss',
})
export class SitePageSectionsComponent implements OnInit {
  private http = inject(HttpClient)
private mediaApi = `${environment.apiUrl}/sitemediafiles`
  private sitesApi = `${environment.apiUrl}/sites`
  private pagesApi = `${environment.apiUrl}/sitepages`
  private sectionsApi = `${environment.apiUrl}/sitepagesections`

  sites: any[] = []
  pages: any[] = []
  sections: any[] = []

  mediaFiles: any[] = []
mediaLoading = false

  selectedSiteId: number | null = null
  selectedPageId: number | null = null

  loading = false
  saving = false
  showForm = false
  editingId: number | null = null

  errorMessage = ''
  successMessage = ''

  form: any = {
    sitePageId: null,
    sectionKey: '',
    sectionTitle: '',
    sectionSubtitle: '',
    htmlContent: '',
    imageUrl: '',
    buttonText: '',
    buttonUrl: '',
    sortOrder: 0,
    isActive: true,
  }

  ngOnInit(): void {
    this.loadSites()
    this.loadPages()
  }

  get filteredPages() {
    if (!this.selectedSiteId) return this.pages
    return this.pages.filter((x) => Number(x.siteId) === Number(this.selectedSiteId))
  }

  loadSites() {
    this.http.get<any[]>(this.sitesApi).subscribe({
      next: (res) => (this.sites = res || []),
      error: () => (this.errorMessage = 'Unable to load sites.'),
    })
  }

  loadPages() {
    this.http.get<any[]>(this.pagesApi).subscribe({
      next: (res) => (this.pages = res || []),
      error: () => (this.errorMessage = 'Unable to load pages.'),
    })
  }
loadMediaForPage() {
  this.mediaFiles = []

  const page = this.pages.find((x) => Number(x.id) === Number(this.selectedPageId))
  if (!page?.siteId) return

  this.mediaLoading = true

  this.http.get<any[]>(`${this.mediaApi}/by-site/${page.siteId}`).subscribe({
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

selectSectionImage(fileUrl: string) {
  this.form.imageUrl = this.getFullUrl(fileUrl)
  this.successMessage = 'Image selected for section.'
}

isImage(item: any) {
  return item.fileType === 'image' || item.mimeType?.startsWith('image/')
}
  onSiteChange() {
    this.selectedPageId = null
    this.sections = []
  }

  onPageChange() {
    if (!this.selectedPageId) {
      this.sections = []
      return
    }

    this.loadSections()
  }

  loadSections() {
    if (!this.selectedPageId) return

    this.loading = true
    this.errorMessage = ''

    this.http.get<any[]>(`${this.sectionsApi}/by-page/${this.selectedPageId}`).subscribe({
      next: (res) => {
        this.sections = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Unable to load sections.'
      },
    })
  }

  startAdd() {
    if (!this.selectedPageId) {
      this.errorMessage = 'Please select a page first.'
      return
    }

    this.showForm = true
    this.editingId = null
    this.successMessage = ''
    this.errorMessage = ''

    this.form = {
      sitePageId: this.selectedPageId,
      sectionKey: '',
      sectionTitle: '',
      sectionSubtitle: '',
      htmlContent: '',
      imageUrl: '',
      buttonText: '',
      buttonUrl: '',
      sortOrder: this.sections.length + 1,
      isActive: true,
    }
  }

  editSection(section: any) {
    this.showForm = true
    this.editingId = section.id
    this.successMessage = ''
    this.errorMessage = ''

    this.form = {
      sitePageId: section.sitePageId,
      sectionKey: section.sectionKey,
      sectionTitle: section.sectionTitle,
      sectionSubtitle: section.sectionSubtitle,
      htmlContent: section.htmlContent,
      imageUrl: section.imageUrl,
      buttonText: section.buttonText,
      buttonUrl: section.buttonUrl,
      sortOrder: section.sortOrder,
      isActive: section.isActive,
    }
  }

  cancelForm() {
    this.showForm = false
    this.editingId = null
  }

  saveSection() {
    this.successMessage = ''
    this.errorMessage = ''

    if (!this.form.sitePageId) {
      this.errorMessage = 'Page is required.'
      return
    }

    if (!this.form.sectionKey) {
      this.errorMessage = 'Section key is required.'
      return
    }

    this.saving = true

    const request = this.editingId
      ? this.http.put(`${this.sectionsApi}/${this.editingId}`, this.form)
      : this.http.post(this.sectionsApi, this.form)

    request.subscribe({
      next: () => {
        this.saving = false
        this.successMessage = this.editingId
          ? 'Section updated successfully.'
          : 'Section created successfully.'

        this.showForm = false
        this.editingId = null
        this.loadSections()
      },
      error: (err) => {
        this.saving = false
        this.errorMessage = err?.error?.message || 'Unable to save section.'
      },
    })
  }

  toggleStatus(section: any) {
    this.http
      .put(`${this.sectionsApi}/${section.id}/status?isActive=${!section.isActive}`, {})
      .subscribe({
        next: () => this.loadSections(),
        error: () => (this.errorMessage = 'Unable to update section status.'),
      })
  }
}