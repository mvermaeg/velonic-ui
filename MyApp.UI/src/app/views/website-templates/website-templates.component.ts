import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { PageTitleComponent } from '@common/page-title.component'
import {
  WebsiteTemplate,
  WebsiteTemplateService,
} from '../../core/service/website-template.service'
@Component({
  selector: 'app-website-templates',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './website-templates.component.html',
})
export class WebsiteTemplatesComponent implements OnInit {
  private templateService = inject(WebsiteTemplateService)

  templates: WebsiteTemplate[] = []
  search = ''
  statusFilter = ''
  loading = false

  ngOnInit(): void {
    this.loadTemplates()
  }

  loadTemplates() {
    this.loading = true

    this.templateService.getAll().subscribe({
      next: (res) => {
        this.templates = res || []
        this.loading = false
      },
      error: (err) => {
        console.error('Template load failed', err)
        this.loading = false
      },
    })
  }

  get filteredTemplates() {
    let data = this.templates

    if (this.statusFilter === 'active') {
      data = data.filter((x) => x.isActive)
    }

    if (this.statusFilter === 'inactive') {
      data = data.filter((x) => !x.isActive)
    }

    if (!this.search) return data

    const s = this.search.toLowerCase()

    return data.filter((x) =>
      [
        x.templateName,
        x.templateKey,
        x.sourceFolder,
        x.buildFolder,
        x.version,
        x.description,
      ]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(s))
    )
  }

  exportToCSV() {
    const rows = this.filteredTemplates.map((x) => ({
      Id: x.id,
      TemplateName: x.templateName,
      TemplateKey: x.templateKey,
      SourceFolder: x.sourceFolder,
      BuildFolder: x.buildFolder,
      Version: x.version,
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
    link.download = 'website-templates.csv'
    link.click()
  }
}