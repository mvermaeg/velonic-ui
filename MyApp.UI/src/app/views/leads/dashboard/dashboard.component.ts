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
  totalSales = 0
  totalProfit = 0
  avgSaleValue = 0

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

  get filteredLeads() {
    if (!this.search) return this.leads

    const s = this.search.toLowerCase()

    return this.leads.filter((x) =>
      [
        x.id,
        x.fullName,
        x.email,
        x.emailVerificationStatus,
        x.emailVerificationReason,
        x.isEmailDeliverable,
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
        x.leadQualityScore,
        x.leadStatus,
      ]
        .filter((v) => v !== null && v !== undefined && v !== '')
        .some((v) => String(v).toLowerCase().includes(s))
    )
  }
}


// import { Component, OnInit, inject } from '@angular/core'
// import { PageTitleComponent } from '@common/page-title.component'
// import { CommonModule } from '@angular/common'
// import { FormsModule } from '@angular/forms'
// import { LeadsService } from '@core/service/leads.service'

// @Component({
//   selector: 'app-dashboard',
//   standalone: true,
//   imports: [PageTitleComponent, CommonModule, FormsModule],
//   templateUrl: './dashboard.component.html',
//   styleUrl: './dashboard.component.scss',
// })
// export class DashboardComponent implements OnInit {
//   private leadsService = inject(LeadsService)

//   leads: any[] = []
//   loading = false

//   totalLeads = 0
//   totalSales = 0
//   totalProfit = 0
//   avgSaleValue = 0

//   search = ''

//   ngOnInit(): void {
//     this.loadLeads()
//   }

//   loadLeads() {
//     this.loading = true

//     this.leadsService.getLeads(1, 20).subscribe({
//       next: (res) => {
//         this.leads = res.data || []

//         this.totalLeads = res.total || this.leads.length
//         this.totalSales = this.leads.reduce((sum, x) => sum + Number(x.sales || 0), 0)
//         this.totalProfit = this.leads.reduce((sum, x) => sum + Number(x.profit || 0), 0)
//         this.avgSaleValue = this.totalLeads > 0 ? this.totalSales / this.totalLeads : 0

//         this.loading = false
//       },
//       error: () => {
//         this.loading = false
//       },
//     })
//   }

//   get filteredLeads() {
//     if (!this.search) return this.leads

//     const s = this.search.toLowerCase()

//     return this.leads.filter((x) =>
//       [
//         x.id,
//         x.fullName,
//         x.email,
//         x.emailVerificationStatus,
//         x.emailVerificationReason,
//         x.isEmailDeliverable,
//         x.phone,
//         x.campaignName,
//         x.affiliateName,
//         x.pageName,
//         x.postcode,
//         x.state,
//         x.city,
//         x.country,
//         x.visitorCountry,
//         x.ipAddress,
//         x.fraudLevel,
//         x.fraudReasons,
//         x.leadQualityScore,
//         x.leadStatus,
//       ]
//         .filter((v) => v !== null && v !== undefined && v !== '')
//         .some((v) => String(v).toLowerCase().includes(s))
//     )
//   }
// }