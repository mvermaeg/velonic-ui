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
  message = ''
  rules: any[] = []
  runs: RoutingRun[] = []
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
  ngOnDestroy() { clearInterval(this.timer) }
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
    this.http.get<NetworxReadiness>(`${this.url}/networx-readiness`).subscribe({ next: x => this.readiness = x, error: () => { this.readiness = null; this.message = 'Networx mapping readiness could not be verified.' } })
    this.http.get<any>(`${this.url}/auction-capabilities`).subscribe({ next: x => this.capabilities = x, error: () => this.message = 'Could not load auction capabilities.' })
    this.loadCoverage()
    if (this.selectedRunId) this.selectRun()
    this.loading = true
    this.http.get<any[]>(`${this.url}/rules`).subscribe({
      next: x => { this.rules = x || []; this.loading = false },
      error: e => { this.message = e?.error?.message || 'Could not load routing rules.'; this.loading = false },
    })
    this.runsLoading = true; this.runsError = ''
    this.http.get<{items: RoutingRun[]}>(`${this.url}/runs?page=1&pageSize=25`).subscribe({
      next: x => { this.runs = x.items || []; this.runsLoading = false }, error: () => { this.runsError = 'Could not load routing runs. Refresh to retry.'; this.runsLoading = false },
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
