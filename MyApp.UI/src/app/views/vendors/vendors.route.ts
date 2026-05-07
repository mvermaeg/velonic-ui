import { Route } from '@angular/router'
import { ApikeyComponent } from './apikey/apikey.component'
import { LeadsComponent } from './leads/leads.component'
import { CommissionComponent } from './commission/commission.component'
import { BlacklistComponent } from './blacklist/blacklist.component'
import { LeadqualityComponent } from './leadquality/leadquality.component'

export const VENDORS_ROUTES: Route[] = [

 { path: 'api-keys', component: ApikeyComponent, data: { title: 'Leads-API Keys' }},
  { path: 'leads-io', component: LeadsComponent, data: { title: 'Leads-Leads I/O' }},
  { path: 'commissions', component: CommissionComponent, data: { title: 'Leads-Commissions' }},
  { path: 'blacklist', component: BlacklistComponent, data: { title: 'Leads-Black List' }},
  { path: 'lead-quality', component: LeadqualityComponent, data: { title: 'Sites-Lead Quality' }},

]
