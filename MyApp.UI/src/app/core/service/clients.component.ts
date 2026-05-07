import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { PageTitleComponent } from '@common/page-title.component'
import { ClientsService, Client } from '@core/service/clients.service'

@Component({
  selector: 'app-clients',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './clients.component.html',
})
export class ClientsComponent implements OnInit {
  private clientsService = inject(ClientsService)

  clients: Client[] = []
  search = ''
  loading = false
  saving = false
  showCreate = false
  errorMessage = ''
  successMessage = ''

  newClient: any = {
    clientName: '',
    tier: 'Tier 1',
    accountStatus: 'Active',
    timezone: 'UTC-05:00 - Eastern Standard Time (EST)',
    acceptsWebLeads: true,
    acceptsInboundCalls: false,
    isBuyer: true,
    isVendor: false,
    accountManagerUserId: null,
    subAccountManagerUserId: null,
    returnAgreement: 'Uncapped',
  }

  ngOnInit(): void {
    this.loadClients()
  }

  get filteredClients() {
    if (!this.search) return this.clients
    const s = this.search.toLowerCase()
    return this.clients.filter((x) =>
      [x.clientName, x.tier, x.accountStatus, x.timezone]
        .filter(Boolean)
        .some((v) => String(v).toLowerCase().includes(s))
    )
  }

  loadClients() {
    this.loading = true
    this.clientsService.getClients().subscribe({
      next: (res) => {
        this.clients = res.data || []
        this.loading = false
      },
      error: () => {
        this.errorMessage = 'Unable to load clients.'
        this.loading = false
      },
    })
  }

  createClient() {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.newClient.clientName) {
      this.errorMessage = 'Client name is required.'
      return
    }

    this.saving = true

    this.clientsService.createClient(this.newClient).subscribe({
      next: () => {
        this.saving = false
        this.successMessage = 'Client created successfully.'
        this.showCreate = false
        this.newClient.clientName = ''
        this.loadClients()
      },
      error: (err) => {
        this.saving = false
        this.errorMessage = err?.error?.message || 'Unable to create client.'
      },
    })
  }
}