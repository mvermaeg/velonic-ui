import { Component, OnInit, inject } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { CommonModule } from '@angular/common'
import { ReportsService } from '@core/service/reports.service'

@Component({
  selector: 'app-reports-campaign',
  standalone: true,
  imports: [PageTitleComponent, CommonModule],
  templateUrl: './campaign.component.html',
  styleUrl: './campaign.component.scss',
})
export class CampaignComponent implements OnInit {
  private reportsService = inject(ReportsService)

  loading = false
  rows: any[] = []

  ngOnInit(): void {
    this.loadReport()
  }

  loadReport() {
    this.loading = true

    this.reportsService.getCampaign().subscribe({
      next: (res) => {
        this.rows = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
      },
    })
  }

  getConversion(row: any): number {
    const leads = Number(row.leads || 0)
    const sold = Number(row.sold || 0)
    return leads > 0 ? (sold * 100) / leads : 0
  }
}