import { Route } from '@angular/router'
import { DashboardComponent } from './dashboard/dashboard.component'
import { ExportComponent } from './export/export.component'
import { SalesComponent } from './sales/sales.component'
import { SubidComponent } from './subid/subid.component'
import { LeadformComponent } from './leadform/leadform.component'
import { AppointmentComponent } from './appointment/appointment.component'


export const LEADS_ROUTES: Route[] = [

  { path: 'dashboard', component: DashboardComponent, data: { title: 'Leads-Dashboard' }},
  { path: 'export', component: ExportComponent, data: { title: 'Leads-Export' }},
  { path: 'sales', component: SalesComponent, data: { title: 'Leads-Sales' }},
  { path: 'appointment-sales', component: AppointmentComponent, data: { title: 'Leads-Appoinment-Sales' }},
  { path: 'subids', component: SubidComponent, data: { title: 'Leads-Sub IDs' }},
  { path: 'leadform', component: LeadformComponent, data: { title: 'Leads-Leadforms' }},
{
  path: 'sales',
  loadComponent: () =>
    import('./sales/sales.component').then((m) => m.SalesComponent),
  data: { title: 'Lead Sales' },
},
{
  path: 'returns',
  loadComponent: () =>
    import('./returns/returns.component').then((m) => m.ReturnsComponent),
  data: { title: 'Lead Returns' },
},
{
  path: 'rejections',
  loadComponent: () =>
    import('./rejections/rejections.component').then((m) => m.RejectionsComponent),
  data: { title: 'Lead Rejections' },
},
]
