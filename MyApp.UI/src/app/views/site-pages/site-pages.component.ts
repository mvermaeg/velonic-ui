import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { HttpClient } from '@angular/common/http'
import { RouterModule } from '@angular/router'
import { PageTitleComponent } from '@common/page-title.component'
import { environment } from '../../../environments/environment'

@Component({
  selector: 'app-site-pages',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent, RouterModule],
  templateUrl: './site-pages.component.html',
  styleUrl: './site-pages.component.scss',
})
export class SitePagesComponent implements OnInit {
  private http = inject(HttpClient)

  private pagesApi = `${environment.apiUrl}/sitepages`
  private sitesApi = `${environment.apiUrl}/sites`

  sites: any[] = []
  pages: any[] = []

  selectedSiteId: number | null = null
  search = ''
  loading = false
  saving = false
  showForm = false
  editingId: number | null = null

  errorMessage = ''
  successMessage = ''

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
    isHomePage: false,
    sortOrder: 0,
    isActive: true,
  }

  ngOnInit(): void {
    this.loadSites()
    this.loadPages()
  }

  get filteredPages() {
    let data = this.pages

    if (this.selectedSiteId) {
      data = data.filter((x) => Number(x.siteId) === Number(this.selectedSiteId))
    }

    if (!this.search) return data

    const s = this.search.toLowerCase()

    return data.filter((x) =>
      [
        x.siteName,
        x.pageName,
        x.pageSlug,
        x.pageTitle,
        x.heroTitle,
        x.metaTitle,
      ]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(s))
    )
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

  loadPages() {
    this.loading = true
    this.errorMessage = ''

    this.http.get<any[]>(this.pagesApi).subscribe({
      next: (res) => {
        this.pages = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Unable to load site pages.'
      },
    })
  }

  startAdd() {
    this.showForm = true
    this.editingId = null

    this.form = {
      siteId: this.selectedSiteId,
      pageName: '',
      pageSlug: '',
      pageTitle: '',
      metaTitle: '',
      metaDescription: '',
      heroTitle: '',
      heroSubtitle: '',
      htmlContent: '',
      jsonContent: '',
      isHomePage: false,
      sortOrder: 0,
      isActive: true,
    }
  }

  editPage(page: any) {
    this.showForm = true
    this.editingId = page.id

    this.form = {
      siteId: page.siteId,
      pageName: page.pageName,
      pageSlug: page.pageSlug,
      pageTitle: page.pageTitle,
      metaTitle: page.metaTitle,
      metaDescription: page.metaDescription,
      heroTitle: page.heroTitle,
      heroSubtitle: page.heroSubtitle,
      htmlContent: page.htmlContent,
      jsonContent: page.jsonContent,
      isHomePage: page.isHomePage,
      sortOrder: page.sortOrder,
      isActive: page.isActive,
    }
  }

  cancelForm() {
    this.showForm = false
    this.editingId = null
  }

  openLivePage(page: any) {
    const site = this.sites.find((x) => Number(x.id) === Number(page.siteId))

    if (!site) {
      this.errorMessage = 'Site not found for this page.'
      return
    }

    const url =
      page.pageSlug === 'home'
        ? `/w/${site.slug}`
        : `/w/${site.slug}/${page.pageSlug}`

    window.open(url, '_blank')
  }

  savePage() {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.form.siteId) {
      this.errorMessage = 'Please select site.'
      return
    }

    if (!this.form.pageName || !this.form.pageSlug) {
      this.errorMessage = 'Page name and slug are required.'
      return
    }

    this.saving = true

    const request = this.editingId
      ? this.http.put(`${this.pagesApi}/${this.editingId}`, this.form)
      : this.http.post(this.pagesApi, this.form)

    request.subscribe({
      next: () => {
        this.saving = false
        this.successMessage = this.editingId
          ? 'Page updated successfully.'
          : 'Page created successfully.'

        this.showForm = false
        this.editingId = null
        this.loadPages()
      },
      error: (err) => {
        this.saving = false
        this.errorMessage = err?.error?.message || 'Unable to save page.'
      },
    })
  }

  toggleStatus(page: any) {
    this.http
      .put(`${this.pagesApi}/${page.id}/status?isActive=${!page.isActive}`, {})
      .subscribe({
        next: () => this.loadPages(),
        error: () => {
          this.errorMessage = 'Unable to update page status.'
        },
      })
  }
}