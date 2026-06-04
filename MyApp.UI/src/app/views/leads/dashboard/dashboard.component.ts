import { Component, OnInit, inject } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { CommonModule } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { LeadsService } from '@core/service/leads.service'

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [PageTitleComponent, CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private leadsService = inject(LeadsService)

  leads: any[] = []
  loading = false

  totalLeads = 0
  totalSales = 0
  totalProfit = 0
  avgSaleValue = 0

  search = ''

  ngOnInit(): void {
    this.loadLeads()
  }

  loadLeads() {
    this.loading = true

    this.leadsService.getLeads(1, 20).subscribe({
      next: (res) => {
        this.leads = res.data || []
        this.totalLeads = res.total || this.leads.length

        this.totalSales = this.leads.reduce((sum, x) => sum + Number(x.sales || 0), 0)
        this.totalProfit = this.leads.reduce((sum, x) => sum + Number(x.profit || 0), 0)
        this.avgSaleValue = this.totalLeads > 0 ? this.totalSales / this.totalLeads : 0

        this.loading = false
      },
      error: () => {
        this.loading = false
      },
    })
  }

  get filteredLeads() {
    if (!this.search) return this.leads

    const s = this.search.toLowerCase()

  return this.leads.filter((x) =>
  [
    x.fullName,
    x.email,
    x.phone,
    x.campaignName,
    x.affiliateName,
    x.pageName,
    x.postcode,
    x.state,
    x.city,
    x.country,
    x.visitorCountry,
    x.ipAddress,
    x.fraudLevel,
    x.fraudReasons,
    x.leadStatus,
  ]
    .filter(Boolean)
    .some((v) => String(v).toLowerCase().includes(s))
)

  }
}