import { TestBed, ComponentFixture } from '@angular/core/testing'
import { provideHttpClient } from '@angular/common/http'
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing'
import { RoutingControlComponent } from './routing-control.component'

describe('RoutingControlComponent', () => {
  let fixture: ComponentFixture<RoutingControlComponent>
  let component: RoutingControlComponent
  let http: HttpTestingController
  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [RoutingControlComponent], providers: [provideHttpClient(), provideHttpClientTesting()] })
    fixture = TestBed.createComponent(RoutingControlComponent)
    component = fixture.componentInstance
    http = TestBed.inject(HttpTestingController)
  })
  afterEach(() => { http.verify(); fixture.destroy() })
  it('shows equal external bidding without editable priority or a configured ranking', () => {
    spyOn(component, 'refresh')
    component.rules = [{ verticalCode: 'Roofing', destinationType: 'ExternalPlatform', platformCode: 'NETWORX', bidAmount: 25, priority: 70, isActive: true }]
    fixture.detectChanges()
    const el: HTMLElement = fixture.nativeElement
    expect(el.textContent).toContain('Configured destinations')
    expect(el.textContent).toContain('compete concurrently with equal standing')
    expect(el.textContent).toContain('Not used in auction')
    expect(el.textContent).not.toContain('Configured ranking')
    expect(el.textContent).not.toContain('Legacy client priority')
    const headers = Array.from(el.querySelectorAll('th')).map(x => x.textContent?.trim())
    expect(headers).not.toContain('Priority')
  })
  it('loads and saves zero without changing auction settings until save is clicked', () => {
    component.loadBidSettings()
    http.expectOne(r => r.url.endsWith('/bid-settings')).flush({ minimumBidAmount: 10, revision: 4 })
    component.minimumBidAmount = 0
    http.expectNone(r => r.method === 'PUT')
    component.saveBidSettings()
    const save = http.expectOne(r => r.url.endsWith('/bid-settings') && r.method === 'PUT')
    expect(save.request.body).toEqual({ minimumBidAmount: 0, expectedRevision: 4 })
    save.flush({ minimumBidAmount: 0, revision: 5 })
    expect(component.bidSettings?.revision).toBe(5)
    expect(component.bidMessage).toContain('new auctions')
  })
  it('blocks invalid minimums and requires reload after a conflicting edit', () => {
    component.bidSettings = { minimumBidAmount: 0, revision: 1 }
    for (const amount of [-1, NaN, 1000001, 0.00001, null]) {
      component.minimumBidAmount = amount
      component.saveBidSettings()
      http.expectNone(r => r.method === 'PUT')
    }
    component.minimumBidAmount = 10; component.saveBidSettings()
    http.expectOne(r => r.method === 'PUT').flush({}, { status: 409, statusText: 'Conflict' })
    component.saveBidSettings()
    http.expectNone(r => r.method === 'PUT')
    expect(component.bidError).toContain('Reload')
  })
  it('does not substitute zero if settings cannot be loaded', () => {
    component.loadBidSettings()
    http.expectOne(r => r.url.endsWith('/bid-settings')).flush({}, { status: 500, statusText: 'Unavailable' })
    expect(component.minimumBidAmount).toBeNull()
    expect(component.bidSettings).toBeNull()
    component.saveBidSettings()
    http.expectNone(r => r.method === 'PUT')
  })
  it('loads capabilities, rules, runs, coverage and mapping readiness independently', () => {
    component.refresh()
    http.expectOne(r => r.url.endsWith('/bid-settings')).flush({ minimumBidAmount: 0, revision: 1 })
    http.expectOne(r => r.url.endsWith('/networx-readiness')).flush({ items: [], note: 'Verified' })
    http.expectOne(r => r.url.endsWith('/auction-capabilities')).flush({ options: { enabled: false }, providers: [] })
    http.expectOne(r => r.url.endsWith('/coverage')).flush({ total: 0, items: [], services: [] })
    http.expectOne(r => r.url.endsWith('/rules')).flush([])
    http.expectOne(r => r.url.includes('/runs?page')).flush({ items: [] })
    expect(component.loading || component.runsLoading || component.coverageLoading).toBeFalse()
    expect(component.runs.length).toBe(0)
  })
  it('displays a detail error instead of stale winner data', () => {
    component.selectedRunId = 7; component.selectRun()
    http.expectOne(r => r.url.endsWith('/runs/7')).flush({}, { status: 500, statusText: 'Unavailable' })
    expect(component.detail).toBeNull(); expect(component.detailLoading).toBeFalse()
    expect(component.detailError).toContain('Could not load')
  })
  it('interprets persisted UTC timestamps for countdown and duration', () => {
    component.now = Date.parse('2026-09-29T12:00:30Z')
    component.detail = { run: { id: 1, leadId: 1, verticalCode: 'Roofing', status: 'CollectingBids', auctionStartedOn: '2026-09-29T12:00:00', auctionClosesOn: '2026-09-29T12:01:30' }, attempts: [] }
    expect(component.secondsRemaining).toBe(60); expect(component.durationSeconds).toBe(30)
  })
  it('does not send reconciliation without a valid provider evidence reference', () => {
    component.detail = { run: { id: 1, leadId: 1, verticalCode: 'Roofing', status: 'ReconciliationRequired' }, attempts: [] }
    component.reconciliationReference = 'private@example.test'; component.reconcile()
    http.expectNone(r => r.url.includes('/reconcile'))
    expect(component.reconciling).toBeFalse()
  })
  it('clears a previous import when an oversized file is chosen', async () => {
    component.importText = '01007'; component.importResult = { rows: 1 }
    await component.readCoverageFile({ target: { files: [{ size: 1500001 }] } } as unknown as Event)
    expect(component.importText).toBe(''); expect(component.importResult).toBeNull()
  })
  it('ignores a stale response after another run is selected', () => {
    component.selectedRunId = 1; component.selectRun()
    component.selectedRunId = 2; component.selectRun()
    http.expectOne(r => r.url.endsWith('/runs/1')).flush({ run: { id: 1 }, attempts: [] })
    expect(component.detail).toBeNull()
    http.expectOne(r => r.url.endsWith('/runs/2')).flush({ run: { id: 2 }, attempts: [] })
    expect(component.detail?.run.id).toBe(2)
  })
})
