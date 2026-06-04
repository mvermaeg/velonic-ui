import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { HttpClient } from '@angular/common/http'
import { PageTitleComponent } from '@common/page-title.component'
import { environment } from '../../../environments/environment'

@Component({
  selector: 'app-site-media',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './site-media.component.html',
  styleUrl: './site-media.component.scss',
})
export class SiteMediaComponent implements OnInit {
  private http = inject(HttpClient)

  private sitesApi = `${environment.apiUrl}/sites`
  private mediaApi = `${environment.apiUrl}/sitemediafiles`

  sites: any[] = []
  mediaFiles: any[] = []

  selectedSiteId: number | null = null
  selectedFile: File | null = null

  altText = ''
  caption = ''

  loading = false
  uploading = false

  successMessage = ''
  errorMessage = ''

  copiedUrl = ''

  ngOnInit(): void {
    this.loadSites()
  }

  loadSites() {
    this.http.get<any[]>(this.sitesApi).subscribe({
      next: (res) => {
        this.sites = res || []

        if (this.sites.length > 0) {
          this.selectedSiteId = this.sites[0].id
          this.loadMedia()
        }
      },
      error: () => {
        this.errorMessage = 'Unable to load sites.'
      },
    })
  }

  onSiteChange() {
    this.mediaFiles = []
    this.successMessage = ''
    this.errorMessage = ''

    if (this.selectedSiteId) {
      this.loadMedia()
    }
  }

  loadMedia() {
    if (!this.selectedSiteId) return

    this.loading = true

    this.http.get<any[]>(`${this.mediaApi}/by-site/${this.selectedSiteId}`).subscribe({
      next: (res) => {
        this.mediaFiles = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Unable to load media files.'
      },
    })
  }

  onFileSelected(event: any) {
    const file = event?.target?.files?.[0]

    if (!file) {
      this.selectedFile = null
      return
    }

    this.selectedFile = file
  }

  uploadFile() {
    this.successMessage = ''
    this.errorMessage = ''

    if (!this.selectedSiteId) {
      this.errorMessage = 'Please select site.'
      return
    }

    if (!this.selectedFile) {
      this.errorMessage = 'Please select file.'
      return
    }

    const formData = new FormData()
    formData.append('SiteId', String(this.selectedSiteId))
    formData.append('File', this.selectedFile)
    formData.append('AltText', this.altText || '')
    formData.append('Caption', this.caption || '')

    this.uploading = true

    this.http.post<any>(`${this.mediaApi}/upload`, formData).subscribe({
      next: () => {
        this.uploading = false
        this.successMessage = 'File uploaded successfully.'
        this.selectedFile = null
        this.altText = ''
        this.caption = ''

        const input = document.getElementById('mediaFileInput') as HTMLInputElement
        if (input) input.value = ''

        this.loadMedia()
      },
      error: (err) => {
        this.uploading = false
        this.errorMessage = err?.error?.message || 'Unable to upload file.'
      },
    })
  }

  deactivate(item: any) {
    this.http.put(`${this.mediaApi}/${item.id}/status?isActive=false`, {}).subscribe({
      next: () => {
        this.successMessage = 'Media removed successfully.'
        this.loadMedia()
      },
      error: () => {
        this.errorMessage = 'Unable to remove media.'
      },
    })
  }

  getFullUrl(fileUrl: string) {
    if (!fileUrl) return ''

    if (fileUrl.startsWith('http')) return fileUrl

    const apiRoot = environment.apiUrl.replace('/api', '')
    return `${apiRoot}${fileUrl}`
  }

  copyUrl(fileUrl: string) {
    const fullUrl = this.getFullUrl(fileUrl)

    navigator.clipboard.writeText(fullUrl).then(() => {
      this.copiedUrl = fullUrl
      this.successMessage = 'File URL copied.'
    })
  }

  isImage(item: any) {
    return item.fileType === 'image' || item.mimeType?.startsWith('image/')
  }

  isVideo(item: any) {
    return item.fileType === 'video' || item.mimeType?.startsWith('video/')
  }
}