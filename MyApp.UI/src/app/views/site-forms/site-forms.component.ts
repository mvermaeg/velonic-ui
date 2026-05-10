import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { HttpClient } from '@angular/common/http'
import { PageTitleComponent } from '@common/page-title.component'
import { environment } from '../../../environments/environment'

@Component({
  selector: 'app-site-forms',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './site-forms.component.html',
  styleUrl: './site-forms.component.scss',
})
export class SiteFormsComponent implements OnInit {
  private http = inject(HttpClient)

  private sitesApi = `${environment.apiUrl}/sites`
  private pagesApi = `${environment.apiUrl}/sitepages`
  private formsApi = `${environment.apiUrl}/siteforms`

  sites: any[] = []
  pages: any[] = []
  forms: any[] = []

  loading = false
  saving = false
  showForm = false
  editingId: number | null = null

  successMessage = ''
  errorMessage = ''

  form: any = {
    siteId: null,
    sitePageId: null,
    formKey: 'quote-form',
    displayName: '',
    submitButtonText: 'Submit Enquiry',
    successMessage: 'Thank you. Your enquiry has been submitted.',
    campaignName: '',
    sourceName: 'Website',
    isMultiStep: false,
    settingsJson: '',
    isActive: true,
  }

  ngOnInit(): void {
    this.loadSites()
    this.loadPages()
    this.loadForms()
  }

  get filteredPages() {
    if (!this.form.siteId) return this.pages
    return this.pages.filter((x) => Number(x.siteId) === Number(this.form.siteId))
  }

  loadSites() {
    this.http.get<any[]>(this.sitesApi).subscribe({
      next: (res) => (this.sites = res || []),
    })
  }

  loadPages() {
    this.http.get<any[]>(this.pagesApi).subscribe({
      next: (res) => (this.pages = res || []),
    })
  }

  loadForms() {
    this.loading = true

    this.http.get<any[]>(this.formsApi).subscribe({
      next: (res) => {
        this.forms = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Unable to load forms.'
      },
    })
  }

  startAdd() {
    this.showForm = true
    this.editingId = null
    this.successMessage = ''
    this.errorMessage = ''

    this.form = {
      siteId: null,
      sitePageId: null,
      formKey: 'quote-form',
      displayName: '',
      submitButtonText: 'Submit Enquiry',
      successMessage: 'Thank you. Your enquiry has been submitted.',
      campaignName: '',
      sourceName: 'Website',
      isMultiStep: false,
      settingsJson: '',
      isActive: true,
    }
  }

  edit(item: any) {
    this.showForm = true
    this.editingId = item.id
    this.successMessage = ''
    this.errorMessage = ''

    this.form = {
      siteId: item.siteId,
      sitePageId: item.sitePageId,
      formKey: item.formKey,
      displayName: item.displayName,
      submitButtonText: item.submitButtonText,
      successMessage: item.successMessage,
      campaignName: item.campaignName,
      sourceName: item.sourceName,
      isMultiStep: item.isMultiStep,
      settingsJson: item.settingsJson,
      isActive: item.isActive,
    }
  }

  cancel() {
    this.showForm = false
    this.editingId = null
  }

  save() {
    this.successMessage = ''
    this.errorMessage = ''

    if (!this.form.siteId) {
      this.errorMessage = 'Please select site.'
      return
    }

    if (!this.form.formKey) {
      this.errorMessage = 'Form key is required.'
      return
    }

    this.saving = true

    const request = this.editingId
      ? this.http.put(`${this.formsApi}/${this.editingId}`, this.form)
      : this.http.post(this.formsApi, this.form)

    request.subscribe({
      next: () => {
        this.saving = false
        this.showForm = false
        this.loadForms()
        this.successMessage = this.editingId
          ? 'Form updated successfully.'
          : 'Form created successfully.'
      },
      error: (err) => {
        this.saving = false
        this.errorMessage = err?.error?.message || 'Unable to save form.'
      },
    })
  }

  toggleStatus(item: any) {
    this.http
      .put(`${this.formsApi}/${item.id}/status?isActive=${!item.isActive}`, {})
      .subscribe({
        next: () => this.loadForms(),
        error: () => (this.errorMessage = 'Unable to update form status.'),
      })
  }
}