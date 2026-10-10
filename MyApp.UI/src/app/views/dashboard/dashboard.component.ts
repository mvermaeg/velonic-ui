import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { HttpClient } from '@angular/common/http'
import { PageTitleComponent } from '@common/page-title.component'
import { NgApexchartsModule } from 'ng-apexcharts'
import { environment } from '../../../environments/environment'

interface DashboardData {
  asOfUtc: string; timezone: string; totalLeads: number; leadsToday: number;
  deliveredToday: number; pending: number; thumbtackOnly: number; noBid: number; needsAttention: number;
  confirmedEarnings: number | null; cost: number | null; profit: number | null; financialNote: string;
  hourly: {hourUtc: string; leads: number; delivered: number}[];
  providers: {platform: string; delivered: number}[];
  recentRuns: {leadId: number; vertical: string; mode: string; status: string; winner: string | null; createdOn: string}[];
}

@Component({
  selector: 'app-dashboard', standalone: true,
  imports: [CommonModule, PageTitleComponent, NgApexchartsModule],
  templateUrl: './dashboard.component.html',
  styles: `.metrics {display:grid;grid-template-columns:repeat(auto-fit,minmax(230px,1fr));gap:20px;margin-bottom:24px}
    .metric {padding:24px;border-radius:14px;background:var(--bs-secondary-bg);border:1px solid var(--bs-border-color)}
    .metric strong {display:block;font-size:28px;margin:10px 0}.metric small {color:var(--bs-secondary-color)}`,
})
export class DashboardComponent implements OnInit {
  private http = inject(HttpClient)
  data: DashboardData | null = null
  loading = false
  error = ''
  chart: any = null
  ngOnInit(): void { this.refresh() }
  refresh(): void {
    if (this.loading) return
    this.loading = true; this.error = ''; this.data = null; this.chart = null
    this.http.get<DashboardData>(`${environment.apiUrl}/reports/dashboard`).subscribe({
      next: data => {
        this.data = data; this.loading = false
        this.chart = {
          series: [{name: 'Leads received', data: data.hourly.map(x => x.leads)},
            {name: 'Leads delivered', data: data.hourly.map(x => x.delivered)}],
          chart: {type: 'line', height: 320, toolbar: {show: false}, animations: {enabled: false}},
          xaxis: {categories: data.hourly.map(x => x.hourUtc.slice(5, 16).replace('T', ' ')), title: {text: 'UTC hour'}},
          yaxis: {min: 0, forceNiceScale: true, decimalsInFloat: 0},
          stroke: {width: 3}, colors: ['#5b8cff', '#45b778'], noData: {text: 'No activity'}
        }
      },
      error: error => {
        this.loading = false
        this.error = error.status === 403 ? 'Administrator access is required to view this dashboard.' :
          error.status === 401 ? 'Your session expired. Please sign in again.' : 'Dashboard data could not be loaded. Please try again.'
      }
    })
  }
  get metrics() {
    const d = this.data
    if (!d) return []
    return [
      {title: 'Leads today', value: d.leadsToday, note: 'UTC calendar day'},
      {title: 'Delivered today', value: d.deliveredToday, note: 'Distinct leads accepted; earnings unconfirmed'},
      {title: 'Routing in progress', value: d.pending, note: 'Current open routing runs'},
      {title: 'Thumbtack-only', value: d.thumbtackOnly, note: 'Last 30 UTC calendar days; includes lookup failures'},
      {title: 'No valid bid', value: d.noBid, note: 'Last 30 UTC calendar days'},
      {title: 'Needs attention', value: d.needsAttention, note: 'Failed or reconciliation-required runs'}
    ]
  }
}