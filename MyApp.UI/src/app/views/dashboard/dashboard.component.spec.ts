import { ComponentFixture, TestBed } from '@angular/core/testing'
import { provideHttpClient } from '@angular/common/http'
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing'
import { DashboardComponent } from './dashboard.component'
import { environment } from '../../../environments/environment'

describe('Live dashboard', () => {
  let fixture: ComponentFixture<DashboardComponent>
  let http: HttpTestingController
  const data = {asOfUtc: '2026-10-10T12:00:00Z', timezone: 'UTC', totalLeads: 10, leadsToday: 2,
    deliveredToday: 1, pending: 1, thumbtackOnly: 1, noBid: 7, needsAttention: 0,
    confirmedEarnings: null, cost: null, profit: null, financialNote: 'Bids are not confirmed earnings.',
    hourly: [], providers: [{platform:'NETWORX', delivered: 9}], recentRuns: []}
  beforeEach(async () => {
    await TestBed.configureTestingModule({imports:[DashboardComponent], providers:[provideHttpClient(), provideHttpClientTesting()]}).compileComponents()
    fixture = TestBed.createComponent(DashboardComponent); http = TestBed.inject(HttpTestingController)
    fixture.detectChanges()
  })
  afterEach(() => { http.verify(); fixture.destroy() })
  it('loads live counts and never substitutes bids for earnings', () => {
    http.expectOne(`${environment.apiUrl}/reports/dashboard`).flush(data)
    // Chart library rendering is covered by the production compiler; test the dashboard data states.
    fixture.componentInstance.chart = null; fixture.detectChanges()
    expect(fixture.nativeElement.textContent).toContain('Unavailable')
    expect(fixture.nativeElement.textContent).toContain('NETWORX')
    expect(fixture.componentInstance.metrics[0].value).toBe(2)
    expect(fixture.nativeElement.textContent).not.toContain('35,807')
  })
  it('shows errors without fabricated zero counts and permits retry', () => {
    http.expectOne(`${environment.apiUrl}/reports/dashboard`).flush({}, {status:500,statusText:'Error'})
    fixture.detectChanges()
    expect(fixture.nativeElement.textContent).toContain('could not be loaded')
    expect(fixture.componentInstance.data).toBeNull()
    fixture.componentInstance.refresh()
    http.expectOne(`${environment.apiUrl}/reports/dashboard`).flush({...data, totalLeads:0, hourly:[], providers:[], recentRuns:[]})
    fixture.componentInstance.chart = null; fixture.detectChanges()
    expect(fixture.nativeElement.textContent).toContain('No production leads')
  })
  it('handles denied access and prevents simultaneous refreshes', () => {
    fixture.componentInstance.refresh()
    http.expectOne(`${environment.apiUrl}/reports/dashboard`).flush({}, {status:403,statusText:'Forbidden'})
    expect(fixture.componentInstance.error).toContain('Administrator access')
  })
})