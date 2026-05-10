import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { HttpClient } from '@angular/common/http'
import { PageTitleComponent } from '@common/page-title.component'
import { environment } from '../../../environments/environment'
import { Router } from '@angular/router'

@Component({
  selector: 'app-site-domains',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './site-domains.component.html',
  styleUrl: './site-domains.component.scss',
})
export class SiteDomainsComponent implements OnInit {
  private http = inject(HttpClient)
private router = inject(Router)
  private sitesApi = `${environment.apiUrl}/sites`
  private domainsApi = `${environment.apiUrl}/sitedomains`

  sites: any[] = []
  domains: any[] = []

  loading = false
  saving = false
  showForm = false

  successMessage = ''
  errorMessage = ''

  editingId: number | null = null

  form: any = {
    siteId: null,
    domainName: '',
    isPrimary: false,
    sslEnabled: false,
    cloudflareEnabled: false,
    isActive: true,
  }

  ngOnInit(): void {
    this.loadSites()
    this.loadDomains()
  }

  loadSites() {
    this.http.get<any[]>(this.sitesApi).subscribe({
      next: (res) => (this.sites = res || []),
    })
  }

  loadDomains() {
    this.loading = true

    this.http.get<any[]>(this.domainsApi).subscribe({
      next: (res) => {
        this.domains = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
      },
    })
  }
openPreview(item: any) {
  this.router.navigateByUrl(`/site-preview/${item.domainName}`)
}
  startAdd() {
    this.showForm = true
    this.editingId = null

    this.form = {
      siteId: null,
      domainName: '',
      isPrimary: false,
      sslEnabled: false,
      cloudflareEnabled: false,
      isActive: true,
    }
  }

  edit(item: any) {
    this.showForm = true
    this.editingId = item.id

    this.form = {
      siteId: item.siteId,
      domainName: item.domainName,
      isPrimary: item.isPrimary,
      sslEnabled: item.sslEnabled,
      cloudflareEnabled: item.cloudflareEnabled,
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

    this.saving = true

    const request = this.editingId
      ? this.http.put(`${this.domainsApi}/${this.editingId}`, this.form)
      : this.http.post(this.domainsApi, this.form)

    request.subscribe({
      next: () => {
        this.saving = false
        this.showForm = false
        this.loadDomains()

        this.successMessage = this.editingId
          ? 'Domain updated successfully.'
          : 'Domain added successfully.'
      },
      error: (err) => {
        this.saving = false
        this.errorMessage = err?.error?.message || 'Unable to save domain.'
      },
    })
  }

  toggleStatus(item: any) {
    this.http
      .put(`${this.domainsApi}/${item.id}/status?isActive=${!item.isActive}`, {})
      .subscribe({
        next: () => this.loadDomains(),
      })
  }
}