import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { PageTitleComponent } from '@common/page-title.component'
import { LeadDeliveriesService } from '@core/service/lead-deliveries.service'

@Component({
  selector: 'app-client-deliveries',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './deliveries.component.html',
  styleUrl: './deliveries.component.scss',
})
export class DeliveriesComponent implements OnInit {
  private service = inject(LeadDeliveriesService)

  records: any[] = []
  total = 0
  loading = false
  errorMessage = ''

  filters: any = {
    search: '',
    status: '',
    clientId: '',
    page: 1,
    pageSize: 20,
  }

  ngOnInit(): void {
    this.load()
  }

  load() {
    this.loading = true
    this.errorMessage = ''

    this.service.getDeliveries(this.filters).subscribe({
      next: (res) => {
        this.records = res.data || []
        this.total = res.total || 0
        this.loading = false
      },
      error: () => {
        this.loading = false
        this.errorMessage = 'Unable to load deliveries.'
      },
    })
  }

  clearFilters() {
    this.filters.search = ''
    this.filters.status = ''
    this.filters.clientId = ''
    this.filters.page = 1
    this.load()
  }

  exportToCSV() {
    const rows = this.records.map((x) => ({
      Id: x.id,
      LeadUuid: x.leadUuid,
      Source: x.sourceName,
      Client: x.clientName,
      Email: x.email,
      Campaign: x.campaignName,
      DeliveryType: x.deliveryType,
      DeliveryStatus: x.deliveryStatus,
      DeliveredOn: x.deliveredOn,
      Response: x.responseMessage,
    }))

    if (rows.length === 0) return

    const csv =
      Object.keys(rows[0]).join(',') +
      '\n' +
      rows.map((r: any) => Object.values(r).map((v) => `"${v ?? ''}"`).join(',')).join('\n')

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
    const link = document.createElement('a')
    link.href = URL.createObjectURL(blob)
    link.download = 'lead-deliveries.csv'
    link.click()
  }
}