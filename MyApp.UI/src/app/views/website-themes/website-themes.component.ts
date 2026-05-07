import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { PageTitleComponent } from '@common/page-title.component'
import {
  WebsiteTheme,
  WebsiteThemeService,
} from '@core/service/website-theme.service'

@Component({
  selector: 'app-website-themes',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './website-themes.component.html',
})
export class WebsiteThemesComponent implements OnInit {
  private themeService = inject(WebsiteThemeService)

  themes: WebsiteTheme[] = []
  search = ''
  statusFilter = ''
  loading = false

  ngOnInit(): void {
    this.loadThemes()
  }

  loadThemes() {
    this.loading = true

    this.themeService.getAll().subscribe({
      next: (res: WebsiteTheme[]) => {
        this.themes = res || []
        this.loading = false
      },
      error: (err: unknown) => {
        console.error('Theme load failed', err)
        this.loading = false
      },
    })
  }

  get filteredThemes() {
    let data = this.themes

    if (this.statusFilter === 'active') data = data.filter((x) => x.isActive)
    if (this.statusFilter === 'inactive') data = data.filter((x) => !x.isActive)

    if (!this.search) return data

    const s = this.search.toLowerCase()

    return data.filter((x) =>
      [
        x.themeName,
        x.themeKey,
        x.primaryColor,
        x.secondaryColor,
        x.fontFamily,
      ]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(s))
    )
  }
}