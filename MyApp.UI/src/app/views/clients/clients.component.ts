import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { PageTitleComponent } from '@common/page-title.component'
import { ClientsService, Client } from '@core/service/clients.service'
import { ClientBiddingService } from '@core/service/client-bidding.service'

@Component({
  selector: 'app-clients',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './clients.component.html',
 styleUrl: './clients.component.scss',
})
export class ClientsComponent implements OnInit {
  private clientsService = inject(ClientsService)
  private biddingService = inject(ClientBiddingService)

  clients: Client[] = []
  search = ''
  loading = false
  saving = false
  errorMessage = ''
  successMessage = ''

  selectedClient: Client | null = null
  biddingSettings: any[] = []
  showBiddingPanel = false
  savingBid = false

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

  newBid: any = {
    leadType: 'Roofing',
    state: '',
    postcode: '',
    bidAmount: 45,
    dailyCap: 10,
    monthlyCap: 200,
    isExclusive: false,
    isActive: true,
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
    this.errorMessage = ''

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

    if (!this.newClient.clientName?.trim()) {
      this.errorMessage = 'Client name is required.'
      return
    }

    this.saving = true

    this.clientsService.createClient(this.newClient).subscribe({
      next: () => {
        this.saving = false
        this.successMessage = 'Client created successfully.'
        this.newClient.clientName = ''
        this.loadClients()
      },
      error: (err) => {
        this.saving = false
        this.errorMessage = err?.error?.message || 'Unable to create client.'
      },
    })
  }

  openBidding(client: Client) {
    this.selectedClient = client
    this.showBiddingPanel = true
    this.loadBidding(client.id)
  }

  closeBidding() {
    this.showBiddingPanel = false
    this.selectedClient = null
    this.biddingSettings = []
  }

  loadBidding(clientId: number) {
    this.biddingService.getByClient(clientId).subscribe({
      next: (res) => {
        this.biddingSettings = res || []
      },
      error: () => {
        this.errorMessage = 'Unable to load bidding settings.'
      },
    })
  }

  saveBid() {
    if (!this.selectedClient) return

    if (!this.newBid.bidAmount || Number(this.newBid.bidAmount) <= 0) {
      this.errorMessage = 'Bid amount must be greater than zero.'
      return
    }

    this.savingBid = true
    this.errorMessage = ''
    this.successMessage = ''

    const payload = {
      clientId: this.selectedClient.id,
      leadType: this.newBid.leadType,
      state: this.newBid.state,
      postcode: this.newBid.postcode,
      bidAmount: Number(this.newBid.bidAmount),
      dailyCap: this.newBid.dailyCap ? Number(this.newBid.dailyCap) : null,
      monthlyCap: this.newBid.monthlyCap ? Number(this.newBid.monthlyCap) : null,
      isExclusive: this.newBid.isExclusive,
      isActive: this.newBid.isActive,
    }

    this.biddingService.create(payload).subscribe({
      next: () => {
        this.savingBid = false
        this.successMessage = 'Bidding setting saved.'
        this.loadBidding(this.selectedClient!.id)
      },
      error: (err) => {
        this.savingBid = false
        this.errorMessage = err?.error?.message || 'Unable to save bidding setting.'
      },
    })
  }

  toggleBidStatus(row: any) {
    if (!this.selectedClient) return

    this.biddingService.updateStatus(row.id, !row.isActive).subscribe({
      next: () => this.loadBidding(this.selectedClient!.id),
      error: () => {
        this.errorMessage = 'Unable to update bidding status.'
      },
    })
  }
}