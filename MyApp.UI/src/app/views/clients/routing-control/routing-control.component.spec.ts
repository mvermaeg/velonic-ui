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
  it('loads capabilities, rules, runs, coverage and mapping readiness independently', () => {
    component.refresh()
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
