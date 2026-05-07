import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { PageTitleComponent } from '@common/page-title.component'
import { LeadSalesService } from '@core/service/lead-sales.service'

@Component({
  selector: 'app-lead-sales',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './sales.component.html',
  styleUrl: './sales.component.scss',
})
export class SalesComponent implements OnInit {
  private service = inject(LeadSalesService)

  records: any[] = []
  loading = false
  errorMessage = ''

  total = 0
  totalRevenue = 0

  filters: any = {
    search: '',
    campaignName: '',
    clientId: '',
    page: 1,
    pageSize: 20,
  }

  selectedSale: any = null
  showReturnBox = false
  returnReason = ''
  returnNotes = ''

  ngOnInit(): void {
    this.load()
  }

  load() {
    this.loading = true
    this.errorMessage = ''

    this.service.getSales(this.filters).subscribe({
      next: (res) => {
        this.records = res.data || []
        this.total = res.total || 0
        this.totalRevenue = res.totalRevenue || 0
        this.loading = false
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Unable to load lead sales.'
      },
    })
  }

  clearFilters() {
    this.filters.search = ''
    this.filters.campaignName = ''
    this.filters.clientId = ''
    this.filters.page = 1
    this.load()
  }

  openReturn(row: any) {
    this.selectedSale = row
    this.returnReason = ''
    this.returnNotes = ''
    this.showReturnBox = true
  }

  submitReturn() {
    if (!this.selectedSale) return

    if (!this.returnReason.trim()) {
      alert('Return reason is required.')
      return
    }

    const payload = {
      leadId: this.selectedSale.leadId,
      clientId: this.selectedSale.clientId,
      leadDeliveryId: this.selectedSale.id,
      returnReason: this.returnReason,
      notes: this.returnNotes,
    }

    this.service.createReturn(payload).subscribe({
      next: () => {
        alert('Return created successfully.')
        this.showReturnBox = false
        this.selectedSale = null
        this.load()
      },
      error: (err) => {
        alert(err?.error?.message || 'Unable to create return.')
      },
    })
  }
}