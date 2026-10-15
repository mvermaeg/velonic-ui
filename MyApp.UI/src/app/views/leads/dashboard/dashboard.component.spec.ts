import { TestBed } from '@angular/core/testing'
import { of, throwError } from 'rxjs'
import { DashboardComponent } from './dashboard.component'
import { LeadsService } from '@core/service/leads.service'
import { ClientBiddingService } from '@core/service/client-bidding.service'

describe('Lead dashboard live data', () => {
  const response = {total: 55, data: [{id: 155}], campaigns: ['Roofing'], statuses: ['New'],
    summary: {deliveredLeads: 12, confirmedRevenue: null, netProfit: null, averageSaleValue: null, financialNote: 'Not reconciled'}}
  let service: jasmine.SpyObj<LeadsService>
  beforeEach(() => {
    service = jasmine.createSpyObj('LeadsService', ['getLeads'])
    service.getLeads.and.returnValue(of(response))
    TestBed.configureTestingModule({imports: [DashboardComponent], providers: [
      {provide: LeadsService, useValue: service}, {provide: ClientBiddingService, useValue: {}}]})
  })
  it('uses full filtered totals, preserves gradients and never invents revenue', () => {
    const f = TestBed.createComponent(DashboardComponent); f.detectChanges()
    expect(f.componentInstance.totalLeads).toBe(55)
    expect(f.componentInstance.deliveredLeads).toBe(12)
    expect(f.nativeElement.querySelectorAll('.gradient-blue,.gradient-purple,.gradient-green,.gradient-orange').length).toBe(4)
    expect(f.nativeElement.textContent).toContain('Unavailable')
    expect(f.componentInstance.totalSales).toBeNull()
    f.destroy()
  })
  it('passes filters and pagination to the API instead of filtering one page', () => {
    const f = TestBed.createComponent(DashboardComponent); const c = f.componentInstance
    c.search = 'customer'; c.campaignName = 'Roofing'; c.status = 'New'; c.loadLeads(true); c.changePage(1)
    expect(service.getLeads).toHaveBeenCalledWith(2, 20, 'customer', 'Roofing', 'New')
    f.destroy()
  })
  it('clears stale display on failure and recovers on retry and empty results', () => {
    const f = TestBed.createComponent(DashboardComponent); const c = f.componentInstance; c.loadLeads()
    service.getLeads.and.returnValue(throwError(() => new Error('offline'))); c.loadLeads()
    expect(c.hasData).toBeFalse(); expect(c.error).toContain('could not be loaded')
    service.getLeads.and.returnValue(of({...response, total: 0, data: []})); c.loadLeads(true)
    expect(c.hasData).toBeTrue(); expect(c.totalLeads).toBe(0); expect(c.error).toBe('')
    f.destroy()
  })
})
