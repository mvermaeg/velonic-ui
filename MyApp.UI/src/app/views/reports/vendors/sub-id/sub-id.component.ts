import { Component, OnInit, inject } from '@angular/core'
import { CommonModule } from '@angular/common'
import { PageTitleComponent } from '@common/page-title.component'
import { ReportsService } from '@core/service/reports.service'

@Component({
  selector: 'app-reports-vendors-sub-id',
  standalone: true,
  imports: [CommonModule, PageTitleComponent],
  templateUrl: './sub-id.component.html',
  styleUrl: './sub-id.component.scss',
})
export class SubIdComponent implements OnInit {
  private reportsService = inject(ReportsService)

  loading = false
  rows: any[] = []

  ngOnInit(): void {
    this.loadReport()
  }

  loadReport() {
    this.loading = true

    this.reportsService.getVendorSubId().subscribe({
      next: (res) => {
        this.rows = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
      },
    })
  }
}