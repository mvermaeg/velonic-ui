import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { HttpClient } from '@angular/common/http'
import { PageTitleComponent } from '@common/page-title.component'
import { environment } from '../../../environments/environment'

@Component({
  selector: 'app-site-pixels',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './site-pixels.component.html',
  styleUrl: './site-pixels.component.scss',
})
export class SitePixelsComponent implements OnInit {
  private http = inject(HttpClient)

  private sitesApi = `${environment.apiUrl}/sites`
  private pixelsApi = `${environment.apiUrl}/sitepixels`

  sites: any[] = []
  pixels: any[] = []

  loading = false
  saving = false
  showForm = false

  successMessage = ''
  errorMessage = ''

  editingId: number | null = null

  form: any = {
    siteId: null,
    pixelType: 'Meta Pixel',
    pixelName: '',
    pixelCode: '',
    placement: 'head',
    campaignName: '',
    sourceName: '',
    fireOnPageSlug: '',
    isActive: true,
  }

  ngOnInit(): void {
    this.loadSites()
    this.loadPixels()
  }

  loadSites() {
    this.http.get<any[]>(this.sitesApi).subscribe({
      next: (res) => {
        this.sites = res || []
      },
    })
  }

  loadPixels() {
    this.loading = true

    this.http.get<any[]>(this.pixelsApi).subscribe({
      next: (res) => {
        this.pixels = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
      },
    })
  }

  startAdd() {
    this.showForm = true
    this.editingId = null

    this.form = {
      siteId: null,
      pixelType: 'Meta Pixel',
      pixelName: '',
      pixelCode: '',
      placement: 'head',
      campaignName: '',
      sourceName: '',
      fireOnPageSlug: '',
      isActive: true,
    }
  }

  edit(item: any) {
    this.showForm = true
    this.editingId = item.id

    this.form = {
      siteId: item.siteId,
      pixelType: item.pixelType,
      pixelName: item.pixelName,
      pixelCode: item.pixelCode,
      placement: item.placement,
      campaignName: item.campaignName,
      sourceName: item.sourceName,
      fireOnPageSlug: item.fireOnPageSlug,
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
      ? this.http.put(`${this.pixelsApi}/${this.editingId}`, this.form)
      : this.http.post(this.pixelsApi, this.form)

    request.subscribe({
      next: () => {
        this.saving = false
        this.showForm = false
        this.loadPixels()

        this.successMessage = this.editingId
          ? 'Pixel updated successfully.'
          : 'Pixel created successfully.'
      },
      error: (err) => {
        this.saving = false
        this.errorMessage = err?.error?.message || 'Unable to save pixel.'
      },
    })
  }

  toggleStatus(item: any) {
    this.http
      .put(`${this.pixelsApi}/${item.id}/status?isActive=${!item.isActive}`, {})
      .subscribe({
        next: () => this.loadPixels(),
      })
  }
}