import { Route } from '@angular/router'
import { GeneralComponent } from './general/general.component'
import { CampaignComponent } from './campaign/campaign.component'
import { ContractsComponent } from './contracts/contracts.component'
import { DailyComponent } from './vendors/daily/daily.component'
import { SubIdComponent } from './vendors/sub-id/sub-id.component'
import { DailyReportComponent } from './Affiliates/daily-report/daily-report.component'
import { ConversionsComponent } from './Affiliates/conversions/conversions.component'

export const REPORTS_ROUTES: Route[] = [

  { path: 'general', component: GeneralComponent, data: { title: 'Reports-General' } },
  { path: 'campaign', component: CampaignComponent, data: { title: 'Reports-Campaign' } },
  { path: 'contracts', component: ContractsComponent, data: { title: 'Reports-Contracts' } },
  { path: 'vendors/daily', component: DailyComponent, data: { title: 'Reports-Vendors-Daily' } },
  { path: 'vendors/sub-id', component: SubIdComponent, data: { title: 'Reports-Vendors-Sub-ID' } },
  { path: 'affiliates/daily-report', component: DailyReportComponent, data: { title: 'Reports-Affiliates-Daily-Report' } },
  { path: 'affiliates/conversions', component: ConversionsComponent, data: { title: 'Reports-Affiliates-Conversions' } },

]
