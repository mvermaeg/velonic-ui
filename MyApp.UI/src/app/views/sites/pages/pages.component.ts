import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { LandingPagesService } from '@core/service/landing-pages.service'
import { FormsModule } from '@angular/forms'

@Component({
  selector: 'app-sites-pages',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './pages.component.html',
})
export class PagesComponent implements OnInit {
  private landingPagesService = inject(LandingPagesService)

  pages: any[] = []
  search = ''
  loading = false

  ngOnInit(): void {
    this.loadPages()
  }

  loadPages() {
    this.loading = true

    this.landingPagesService.getPages().subscribe({
      next: (res) => {
        this.pages = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
      },
    })
  }

  get filteredPages() {
    if (!this.search) return this.pages

    const s = this.search.toLowerCase()

    return this.pages.filter((x) =>
      [
        x.pageName,
        x.pageSlug,
        x.campaignName,
        x.affiliateName,
        x.pageTitle,
        x.heroTitle,
      ]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(s))
    )
  }

  exportToCSV() {
    const rows = this.filteredPages.map((x) => ({
      Id: x.id,
      PageName: x.pageName,
      Path: '/' + x.pageSlug,
      Campaign: x.campaignName,
      Affiliate: x.affiliateName,
      Title: x.pageTitle,
      Created: x.createdOn,
      Status: x.isActive ? 'Active' : 'Inactive',
    }))

    const csv =
      Object.keys(rows[0] || {}).join(',') +
      '\n' +
      rows.map((r: any) => Object.values(r).join(',')).join('\n')

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
    const link = document.createElement('a')
    link.href = URL.createObjectURL(blob)
    link.download = 'landing-pages.csv'
    link.click()
  }
}