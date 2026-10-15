import { Component, OnInit, inject } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { CommonModule } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { LeadsService } from '@core/service/leads.service'
import { ClientBiddingService } from '@core/service/client-bidding.service'

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [PageTitleComponent, CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit {
  private leadsService = inject(LeadsService)
  private clientBiddingService = inject(ClientBiddingService)

  leads: any[] = []
  loading = false

  totalLeads = 0
  totalSales: number | null = null
  totalProfit: number | null = null
  avgSaleValue: number | null = null

  search = ''

  selectedLead: any = null
  manualClientId: number | null = null
  manualBidAmount = 0
  manualNote = ''
  manualDeliveryMessage = ''
  manualDelivering = false

  ngOnInit(): void {
    this.loadLeads()
  }

  error = ''
  hasData = false
  financialNote = ''
  deliveredLeads = 0
  campaignName = ''
  status = ''
  campaigns: string[] = []
  statuses: string[] = []
  page = 1
  pageSize = 20
  loadLeads(resetPage = false) {
    if (this.loading) return
    if (resetPage) this.page = 1
    this.loading = true
    this.error = ''
    this.leadsService.getLeads(this.page, this.pageSize, this.search, this.campaignName, this.status).subscribe({
      next: res => {
        this.leads = res?.data || []
        this.totalLeads = res?.total ?? 0
        this.totalSales = res?.summary?.confirmedRevenue ?? null
        this.totalProfit = res?.summary?.netProfit ?? null
        this.avgSaleValue = res?.summary?.averageSaleValue ?? null
        this.deliveredLeads = res?.summary?.deliveredLeads ?? 0
        this.financialNote = res?.summary?.financialNote || 'Financial reconciliation unavailable.'
        this.campaigns = res?.campaigns || []
        this.statuses = res?.statuses || []
        this.hasData = true
        this.loading = false
      },
      error: () => {
        this.hasData = false
        this.leads = []
        this.error = 'Lead dashboard could not be loaded. Please retry.'
        this.loading = false
      }
    })
  }
  changePage(delta: number) {
    if (this.loading) return
    this.page += delta
    this.loadLeads()
  }
  exportLeads() {
    const fields = ['id','createdAt','fullName','email','phone','campaignName','postcode','state','city','leadStatus']
    const cell = (value: unknown) => '"' + String(value ?? '').replace(/^[=+@-]/, "'$&").replace(/"/g, '""') + '"'
    const csv = [fields.join(','), ...this.leads.map(row => fields.map(key => cell(row[key])).join(','))].join('\r\n')
    const url = URL.createObjectURL(new Blob([csv], {type:'text/csv;charset=utf-8;'}))
    const link = document.createElement('a'); link.href = url; link.download = 'leads-current-page.csv'; link.click()
    URL.revokeObjectURL(url)
  }
  openManualDelivery(lead: any) {
    this.selectedLead = lead
    this.manualClientId = null
    this.manualBidAmount = 0
    this.manualNote = ''
    this.manualDeliveryMessage = ''
  }

  closeManualDelivery() {
    this.selectedLead = null
    this.manualClientId = null
    this.manualBidAmount = 0
    this.manualNote = ''
    this.manualDeliveryMessage = ''
  }

  manualDeliverLead() {
    if (!this.selectedLead) return

    if (!this.manualClientId || this.manualClientId <= 0) {
      this.manualDeliveryMessage = 'Please enter valid Client ID.'
      return
    }

    this.manualDelivering = true
    this.manualDeliveryMessage = ''

    this.clientBiddingService.manualDeliver({
      leadId: this.selectedLead.id,
      clientId: this.manualClientId,
      bidAmount: this.manualBidAmount || 0,
      note: this.manualNote,
    }).subscribe({
      next: (res) => {
        this.manualDelivering = false
        this.manualDeliveryMessage = res.message || 'Lead delivered manually.'
        this.loadLeads()
      },
      error: (err) => {
        this.manualDelivering = false
        this.manualDeliveryMessage = err?.error?.message || 'Manual delivery failed.'
      },
    })
  }

  get filteredLeads() { return this.leads }
}
