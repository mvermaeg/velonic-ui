import { CommonModule } from '@angular/common'
import { Component, OnInit, OnDestroy, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { HttpClient } from '@angular/common/http'
import { environment } from '../../../../environments/environment'
import { RoutingRun, RunDetail, NetworxReadiness } from './routing-control.models'

@Component({
  selector: 'app-routing-control',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './routing-control.component.html',
  styles: [':host { display: block; min-width: 0; } pre, td { overflow-wrap: anywhere; } .card, .card-body { min-width: 0; }'],
})
export class RoutingControlComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient)
  private url = `${environment.apiUrl}/lead-routing`
  loading = false
  saving = false
  bidSettings: { minimumBidAmount: number; revision: number } | null = null
  minimumBidAmount: number | null = null
  bidLoading = false
  bidSaving = false
  bidError = ''
  bidMessage = ''
  message = ''
  rules: any[] = []
  runs: RoutingRun[] = []
  runsPage = 1
  runsPageSize = 25
  runsTotal = 0
  private runsRequest = 0
  private requestedRunsPage = 1
  get runsTotalPages() { return Math.max(1, Math.ceil(this.runsTotal / this.runsPageSize)) }
  get runsFirst() { return this.runs.length ? (this.runsPage - 1) * this.runsPageSize + 1 : 0 }
  get runsLast() { return this.runs.length ? this.runsFirst + this.runs.length - 1 : 0 }
  get runsPages() {
    const start = Math.max(1, Math.min(this.runsPage - 2, this.runsTotalPages - 4))
    return Array.from({ length: Math.min(5, this.runsTotalPages) }, (_, i) => start + i)
  }
  capabilities: any = null
  detail: RunDetail | null = null
  readiness: NetworxReadiness | null = null
  detailLoading = false
  runsLoading = false
  coverageLoading = false
  detailError = ''
  runsError = ''
  coverageError = ''
  reconciliationReference = ''
  reconciliationAccepted = false
  reconciling = false
  now = Date.now()
  private timer = setInterval(() => this.now = Date.now(), 1000)
  ngOnDestroy() { clearInterval(this.timer); this.runsRequest++ }
  utc(value?: string): number { return value ? Date.parse(/Z$|[+-]\d\d:\d\d$/.test(value) ? value : value + 'Z') : this.now }
  get secondsRemaining() { return Math.max(0, Math.ceil((this.utc(this.detail?.run.auctionClosesOn) - this.now) / 1000)) }
  get durationSeconds() { const r = this.detail?.run; return r?.auctionStartedOn ? Math.max(0, Math.floor((this.utc(r.winnerLockedOn || r.completedOn) - this.utc(r.auctionStartedOn)) / 1000)) : 0 }
  selectedRunId: number | null = null
  coverage: any = null
  coverageService = ''
  coverageZip = ''
  coveragePage = 1
  importService = 'Roofing'
  importText = ''
  replaceCoverage = false
  importResult: any = null
  importBusy = false
  form = this.emptyForm()

  ngOnInit() { this.refresh() }
  emptyForm() {
    return { id: 0, destinationType: 'ExternalPlatform', clientId: null,
      platformCode: 'BLUEINK', verticalCode: 'Roofing', state: '', postcode: '',
      bidAmount: 0, priority: 100, dailyCap: null, monthlyCap: null, isActive: false,
      effectiveFrom: null as string | null, effectiveTo: null as string | null }
  }
  refresh() {
    this.loadBidSettings()
    this.http.get<NetworxReadiness>(`${this.url}/networx-readiness`).subscribe({ next: x => this.readiness = x, error: () => { this.readiness = null; this.message = 'Networx mapping readiness could not be verified.' } })
    this.http.get<any>(`${this.url}/auction-capabilities`).subscribe({ next: x => this.capabilities = x, error: () => this.message = 'Could not load auction capabilities.' })
    this.loadCoverage()
    if (this.selectedRunId) this.selectRun()
    this.loading = true
    this.http.get<any[]>(`${this.url}/rules`).subscribe({
      next: x => { this.rules = x || []; this.loading = false },
      error: e => { this.message = e?.error?.message || 'Could not load routing rules.'; this.loading = false },
    })
    this.loadRuns()
  }
  loadRuns(page = this.runsPage) {
    const target = Math.max(1, Math.trunc(page))
    if (!Number.isFinite(target)) return
    this.requestedRunsPage = target
    const request = ++this.runsRequest
    const pageSize = this.runsPageSize
    this.runsLoading = true; this.runsError = ''
    this.http.get<{items: RoutingRun[]; total: number}>(`${this.url}/runs?page=${target}&pageSize=${pageSize}`).subscribe({
      next: x => {
        if (request !== this.runsRequest) return
        const total = Math.max(0, x.total || 0)
        const lastPage = Math.max(1, Math.ceil(total / pageSize))
        this.runsTotal = total
        if (target > lastPage) { this.loadRuns(lastPage); return }
        this.runs = x.items || []; this.runsPage = target; this.runsLoading = false
      },
      error: () => {
        if (request !== this.runsRequest) return
        this.runsError = 'Could not load routing history. Retry this page.'; this.runsLoading = false
      },
    })
  }
  retryRuns() { this.loadRuns(this.requestedRunsPage) }
  goRunsPage(page: number) {
    if (this.runsLoading || page < 1 || page > this.runsTotalPages || page === this.runsPage) return
    this.loadRuns(page)
  }
  changeRunsPageSize(size: number) {
    if (this.runsLoading || ![10, 25, 50, 100].includes(Number(size))) return
    this.runsPageSize = Number(size); this.runsPage = 1; this.runs = []
    this.loadRuns(1)
  }
  openRun(id: number) { this.selectedRunId = id; this.selectRun() }
  loadBidSettings() {
    this.bidLoading = true; this.bidError = ''; this.bidSettings = null; this.minimumBidAmount = null
    this.http.get<{ minimumBidAmount: number; revision: number }>(`${this.url}/bid-settings`).subscribe({
      next: x => { this.bidSettings = x; this.minimumBidAmount = x.minimumBidAmount; this.bidLoading = false },
      error: () => { this.bidError = 'Could not load the auction minimum. Refresh before editing.'; this.bidLoading = false },
    })
  }
  get validMinimumBid() {
    const n = this.minimumBidAmount
    return n != null && Number.isFinite(n) && n >= 0 && n <= 1000000 && Math.abs(n * 10000 - Math.round(n * 10000)) < 0.000001
  }
  saveBidSettings() {
    if (!this.bidSettings || !this.validMinimumBid || this.bidSaving || this.bidLoading || this.bidError) return
    this.bidSaving = true; this.bidError = ''; this.bidMessage = ''
    this.http.put<{ minimumBidAmount: number; revision: number }>(`${this.url}/bid-settings`, {
      minimumBidAmount: this.minimumBidAmount, expectedRevision: this.bidSettings.revision,
    }).subscribe({
      next: x => { this.bidSettings = x; this.minimumBidAmount = x.minimumBidAmount; this.bidSaving = false
        this.bidMessage = 'Minimum saved for new auctions. Existing auctions and deliveries are unchanged.'
        if (this.capabilities?.options) this.capabilities.options.minimumBidAmount = x.minimumBidAmount
      },
      error: e => { this.bidError = e.status === 409 ? 'Another administrator changed this setting. Reload the minimum before saving again.' : 'Save could not be confirmed. Reload the minimum before trying again.'; this.bidSaving = false },
    })
  }
  save() {
    this.saving = true; this.message = ''
    const model: any = { ...this.form }
    for (const key of ['effectiveFrom', 'effectiveTo']) model[key] = model[key] ? `${model[key].slice(0, 16)}:00Z` : null
    if (model.destinationType === 'InternalClient') model.platformCode = null
    else model.clientId = null
    this.http.post<any>(`${this.url}/rules`, model).subscribe({
      next: x => { this.message = x.message; this.saving = false; this.form = this.emptyForm(); this.refresh() },
      error: e => { this.message = e?.error?.message || 'Could not save routing rule.'; this.saving = false },
    })
  }
  edit(x: any) { this.form = { ...x, effectiveFrom: x.effectiveFrom?.slice(0, 16) || null, effectiveTo: x.effectiveTo?.slice(0, 16) || null } }
  toggle(x: any) {
    this.http.put<any>(`${this.url}/rules/${x.id}/status?isActive=${!x.isActive}`, {}).subscribe({
      next: () => x.isActive = !x.isActive,
      error: e => this.message = e?.error?.message || 'Could not change rule status.',
    })
  }
  selectRun() {
    if (!this.selectedRunId) { this.detail = null; return }
    const id = this.selectedRunId
    this.detail = null; this.detailLoading = true; this.detailError = ''; this.reconciliationReference = ''
    this.http.get<RunDetail>(`${this.url}/runs/${id}`).subscribe({ next: x => { if (this.selectedRunId === id) { this.detail = x; this.detailLoading = false } }, error: () => { if (this.selectedRunId === id) { this.detailError = 'Could not load auction details. Refresh to retry.'; this.detailLoading = false } } })
  }
  reconcile() {
    if (!this.detail || this.detail.run.status !== 'ReconciliationRequired' || !/^[A-Za-z0-9_-]{3,100}$/.test(this.reconciliationReference)) return
    this.reconciling = true
    this.http.post<{message: string}>(`${this.url}/runs/${this.detail.run.id}/reconcile`, { accepted: this.reconciliationAccepted, evidenceReference: this.reconciliationReference })
      .subscribe({ next: x => { this.message = x.message; this.reconciling = false; this.refresh() }, error: e => { this.message = e?.error?.message || 'Reconciliation failed. Refresh before retrying.'; this.reconciling = false } })
  }
  loadCoverage() {
    this.coverageLoading = true; this.coverageError = ''
    this.http.get<any>(`${this.url}/coverage`, { params: { service: this.coverageService, zip: this.coverageZip, page: this.coveragePage } })
      .subscribe({ next: x => { this.coverage = x; this.coverageLoading = false }, error: () => { this.coverageError = 'Could not load coverage. Refresh to retry.'; this.coverageLoading = false } })
  }
  toggleCoverage(service: string, zip: string | null, enabled: boolean) {
    this.http.put(`${this.url}/coverage/status`, { service, zip, enabled }).subscribe({ next: () => this.loadCoverage(), error: () => this.message = 'Could not update coverage.' })
  }
  async readCoverageFile(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0]
    this.importResult = null
    this.importText = ''
    if (!file) return
    if (file.size > 1500000) { this.message = 'Coverage file must be smaller than 1.5 MB.'; return }
    try { this.importText = await file.text() } catch { this.message = 'Could not read this coverage file.' }
  }
  importCoverage(confirm: boolean) {
    this.importBusy = true
    this.http.post<any>(`${this.url}/coverage/import`, { text: this.importText, service: this.importService, replace: this.replaceCoverage, confirm })
      .subscribe({ next: x => { this.importResult = x; this.importBusy = false; if (x.applied) this.loadCoverage() },
        error: () => { this.message = 'Coverage import failed; refresh to check its result before retrying.'; this.importBusy = false } })
  }
}
