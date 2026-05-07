import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { PageTitleComponent } from '@common/page-title.component'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../environments/environment'

@Component({
  selector: 'app-sites',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './sites.component.html',
  styleUrl: './sites.component.scss',
})
export class SitesComponent implements OnInit {
  private http = inject(HttpClient)

  private apiUrl = `${environment.apiUrl}/sites`
  private templatesApiUrl = `${environment.apiUrl}/WebsiteTemplates`
  private themesApiUrl = `${environment.apiUrl}/WebsiteThemes`

  sites: any[] = []
  templates: any[] = []
  themes: any[] = []

  search = ''
  loading = false
  saving = false
  errorMessage = ''
  successMessage = ''

  showAddForm = false

  newSite: any = {
    siteName: '',
    domainName: '',
    slug: '',
    themeKey: 'theme-1',
    websiteTemplateId: null,
    websiteThemeId: null,
    isActive: true,
  }

  ngOnInit(): void {
    this.loadSites()
    this.loadTemplates()
    this.loadThemes()
  }

  get filteredSites() {
    if (!this.search) return this.sites

    const s = this.search.toLowerCase()

    return this.sites.filter((x) =>
      [
        x.siteName,
        x.domainName,
        x.slug,
        x.themeKey,
        x.templateName,
        x.templateKey,
        x.themeName,
        x.themeKeyName,
      ]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(s))
    )
  }

  loadSites() {
    this.loading = true
    this.errorMessage = ''

    this.http.get<any[]>(this.apiUrl).subscribe({
      next: (res) => {
        this.sites = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Unable to load sites.'
      },
    })
  }

  loadTemplates() {
    this.http.get<any[]>(this.templatesApiUrl).subscribe({
      next: (res) => {
        this.templates = (res || []).filter((x) => x.isActive)
      },
      error: () => {
        this.errorMessage = 'Unable to load website templates.'
      },
    })
  }

  loadThemes() {
    this.http.get<any[]>(this.themesApiUrl).subscribe({
      next: (res) => {
        this.themes = (res || []).filter((x) => x.isActive)
      },
      error: () => {
        this.errorMessage = 'Unable to load website themes.'
      },
    })
  }

  createSite() {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.newSite.siteName || !this.newSite.slug) {
      this.errorMessage = 'Site name and slug are required.'
      return
    }

    if (!this.newSite.websiteTemplateId) {
      this.errorMessage = 'Please select website template.'
      return
    }

    if (!this.newSite.websiteThemeId) {
      this.errorMessage = 'Please select website theme.'
      return
    }

    this.saving = true

    this.http.post<any>(this.apiUrl, this.newSite).subscribe({
      next: () => {
        this.saving = false
        this.successMessage = 'Site created successfully.'
        this.showAddForm = false

        this.newSite = {
          siteName: '',
          domainName: '',
          slug: '',
          themeKey: 'theme-1',
          websiteTemplateId: null,
          websiteThemeId: null,
          isActive: true,
        }

        this.loadSites()
      },
      error: (err) => {
        this.saving = false
        this.errorMessage = err?.error?.message || 'Unable to create site.'
      },
    })
  }

  toggleStatus(site: any) {
    this.http
      .put<any>(`${this.apiUrl}/${site.id}/status?isActive=${!site.isActive}`, {})
      .subscribe({
        next: () => this.loadSites(),
        error: () => {
          this.errorMessage = 'Unable to update site status.'
        },
      })
  }

  exportToCSV() {
    const rows = this.filteredSites.map((x) => ({
      Id: x.id,
      Name: x.siteName,
      Domain: x.domainName,
      Slug: x.slug,
      Template: x.templateName,
      Theme: x.themeName,
      ThemeKey: x.themeKey,
      Created: x.createdOn,
      Status: x.isActive ? 'Active' : 'Inactive',
    }))

    if (rows.length === 0) return

    const csv =
      Object.keys(rows[0]).join(',') +
      '\n' +
      rows.map((r: any) => Object.values(r).join(',')).join('\n')

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
    const link = document.createElement('a')
    link.href = URL.createObjectURL(blob)
    link.download = 'sites.csv'
    link.click()
  }
}



// import { CommonModule } from '@angular/common'
// import { Component, OnInit, inject } from '@angular/core'
// import { FormsModule } from '@angular/forms'
// import { PageTitleComponent } from '@common/page-title.component'
// import { HttpClient } from '@angular/common/http'
// import { environment } from '../../../environments/environment'

// @Component({
//   selector: 'app-sites',
//   standalone: true,
//   imports: [CommonModule, FormsModule, PageTitleComponent],
//   templateUrl: './sites.component.html',
//   styleUrl: './sites.component.scss',
// })
// export class SitesComponent implements OnInit {
//   private http = inject(HttpClient)
//   private apiUrl = `${environment.apiUrl}/sites`

//   sites: any[] = []
//   search = ''
//   loading = false
//   saving = false
//   errorMessage = ''
//   successMessage = ''

//   showAddForm = false

//   newSite = {
//     siteName: '',
//     domainName: '',
//     slug: '',
//     themeKey: 'theme-1',
//     isActive: true,
//   }

//   ngOnInit(): void {
//     this.loadSites()
//   }

//   get filteredSites() {
//     if (!this.search) return this.sites

//     const s = this.search.toLowerCase()

//     return this.sites.filter((x) =>
//       [x.siteName, x.domainName, x.slug, x.themeKey]
//         .filter(Boolean)
//         .some((v) => String(v).toLowerCase().includes(s))
//     )
//   }

//   loadSites() {
//     this.loading = true
//     this.errorMessage = ''

//     this.http.get<any[]>(this.apiUrl).subscribe({
//       next: (res) => {
//         this.sites = res || []
//         this.loading = false
//       },
//       error: () => {
//         this.loading = false
//         this.errorMessage = 'Unable to load sites.'
//       },
//     })
//   }

//   createSite() {
//     this.errorMessage = ''
//     this.successMessage = ''

//     if (!this.newSite.siteName || !this.newSite.slug) {
//       this.errorMessage = 'Site name and slug are required.'
//       return
//     }

//     this.saving = true

//     this.http.post<any>(this.apiUrl, this.newSite).subscribe({
//       next: () => {
//         this.saving = false
//         this.successMessage = 'Site created successfully.'
//         this.showAddForm = false
//         this.newSite = {
//           siteName: '',
//           domainName: '',
//           slug: '',
//           themeKey: 'theme-1',
//           isActive: true,
//         }
//         this.loadSites()
//       },
//       error: (err) => {
//         this.saving = false
//         this.errorMessage = err?.error?.message || 'Unable to create site.'
//       },
//     })
//   }

//   toggleStatus(site: any) {
//     this.http
//       .put<any>(`${this.apiUrl}/${site.id}/status?isActive=${!site.isActive}`, {})
//       .subscribe({
//         next: () => this.loadSites(),
//         error: () => {
//           this.errorMessage = 'Unable to update site status.'
//         },
//       })
//   }

//   exportToCSV() {
//     const rows = this.filteredSites.map((x) => ({
//       Id: x.id,
//       Name: x.siteName,
//       Domain: x.domainName,
//       Slug: x.slug,
//       Theme: x.themeKey,
//       Created: x.createdOn,
//       Status: x.isActive ? 'Active' : 'Inactive',
//     }))

//     if (rows.length === 0) return

//     const csv =
//       Object.keys(rows[0]).join(',') +
//       '\n' +
//       rows.map((r: any) => Object.values(r).join(',')).join('\n')

//     const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
//     const link = document.createElement('a')
//     link.href = URL.createObjectURL(blob)
//     link.download = 'sites.csv'
//     link.click()
//   }
// }