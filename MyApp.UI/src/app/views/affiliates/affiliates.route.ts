import { Route } from '@angular/router'
import { ClicksComponent } from './clicks/clicks.component'
import { CommissionsComponent } from './commissions/commissions.component'
import { EmailComponent } from './email/email.component'
import { FraudsComponent } from './frauds/frauds.component'
import { OffersComponent } from './offers/offers.component'
import { PixelslogComponent } from './pixellog/pixellog.component'


export const AFFILIATES_ROUTES: Route[] = [

 { path: 'clicks', component: ClicksComponent, data: { title: 'Leads-Clicks' }},
  { path: 'commissions', component: CommissionsComponent, data: { title: 'Leads-Commissions' }},
  { path: 'email-subscriptions', component: EmailComponent, data: { title: 'Leads-Email Subscriptions' }},
  { path: 'frauds', component: FraudsComponent, data: { title: 'Leads-Frauds' }},
  { path: 'offers', component: OffersComponent, data: { title: 'Sites-Offers' }},
  { path: 'pixel-log', component: PixelslogComponent, data: { title: 'Leads-Pixel Log' }}, 

]
