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
  it('requests older history pages and uses the API total rather than the first 25 rows', () => {
    component.runsTotal = 76
    component.goRunsPage(2)
    http.expectOne(r => r.url.endsWith('/runs?page=2&pageSize=25')).flush({ total: 76, items: Array.from({length:25},(_,i)=>({id:26+i,leadId:126+i})) })
    expect(component.runsPage).toBe(2)
    expect(component.runsTotalPages).toBe(4)
    expect(component.runsFirst).toBe(26)
    expect(component.runsLast).toBe(50)
    component.goRunsPage(4)
    http.expectOne(r => r.url.endsWith('/runs?page=4&pageSize=25')).flush({ total:76,items:[{id:76,leadId:100}] })
    expect(component.runsFirst).toBe(76)
    expect(component.runsLast).toBe(76)
    component.goRunsPage(5)
    component.goRunsPage(0)
    component.goRunsPage(4)
    http.expectNone(r => r.url.includes('/runs?'))
  })
  it('resets to the first page when changing the page size and rejects unsupported sizes', () => {
    component.runsPage=3; component.runsTotal=90
    component.changeRunsPageSize(50)
    http.expectOne(r=>r.url.endsWith('/runs?page=1&pageSize=50')).flush({total:90,items:[]})
    expect(component.runsPage).toBe(1)
    expect(component.runsPageSize).toBe(50)
    expect(component.runsTotalPages).toBe(2)
    component.changeRunsPageSize(500)
    http.expectNone(r=>r.url.includes('/runs?'))
  })
  it('ignores stale history replies after a newer refresh and holds navigation while loading', () => {
    component.runsTotal=80
    component.loadRuns(2)
    const old=http.expectOne(r=>r.url.endsWith('/runs?page=2&pageSize=25'))
    component.goRunsPage(3)
    http.expectNone(r=>r.url.endsWith('/runs?page=3&pageSize=25'))
    component.loadRuns(1)
    http.expectOne(r=>r.url.endsWith('/runs?page=1&pageSize=25')).flush({total:80,items:[{id:9,leadId:99}]})
    old.flush({total:80,items:[{id:1,leadId:1}]})
    expect(component.runs[0].leadId).toBe(99)
    expect(component.runsPage).toBe(1)
  })
  it('preserves the displayed page on failure and retries the failed target page', () => {
    component.runsTotal=80
    component.loadRuns(2)
    http.expectOne(r=>r.url.endsWith('/runs?page=2&pageSize=25')).flush({}, {status:500,statusText:'Unavailable'})
    expect(component.runsPage).toBe(1)
    expect(component.runsLoading).toBeFalse()
    expect(component.runsError).toContain('Retry')
    component.retryRuns()
    http.expectOne(r=>r.url.endsWith('/runs?page=2&pageSize=25')).flush({total:80,items:[]})
    expect(component.runsPage).toBe(2)
    expect(component.runsError).toBe('')
  })
  it('returns to the last available page if records were removed before refresh', () => {
    component.runsPage=4; component.runsTotal=76
    component.loadRuns()
    http.expectOne(r=>r.url.endsWith('/runs?page=4&pageSize=25')).flush({total:40,items:[]})
    http.expectOne(r=>r.url.endsWith('/runs?page=2&pageSize=25')).flush({total:40,items:[{id:40,leadId:40}]})
    expect(component.runsPage).toBe(2)
    expect(component.runsLoading).toBeFalse()
  })
  it('renders empty history safely and opens details from a history lead link', () => {
    spyOn(component,'refresh')
    fixture.detectChanges()
    const el:HTMLElement=fixture.nativeElement
    expect(el.textContent).toContain('Showing 0–0 of 0 routing records')
    const next=Array.from(el.querySelectorAll('button')).find(x=>x.textContent?.trim()==='Next')!
    expect(next.disabled).toBeTrue()
    component.runs=[{id:7,leadId:192,verticalCode:'Roofing',status:'NoBid'}]
    component.runsTotal=76
    fixture.detectChanges()
    const lead=el.querySelector('button[aria-label="View routing details for lead 192"]') as HTMLButtonElement
    lead.click()
    http.expectOne(r=>r.url.endsWith('/runs/7')).flush({run:component.runs[0],attempts:[]})
    expect(component.detail?.run.leadId).toBe(192)
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
