import { Route } from '@angular/router'
import { ClicksComponent } from './clicks/clicks.component'
import { LinksComponent } from './links/links.component'
import { SplitsComponent } from './splits/splits.component'
import { FacebookComponent } from './facebook/facebook.component'
import { ExternalComponent } from './external/external.component'
import { RepositoryComponent } from './repository/repository.component'


export const MEDIA_ROUTES: Route[] = [

 { path: 'clicks', component: ClicksComponent, data: { title: 'Leads-Clicks' }},
  { path: 'links', component: LinksComponent, data: { title: 'Leads-Links' }},
  { path: 'split-test', component: SplitsComponent, data: { title: 'Leads-Splits' }},
  { path: 'facebook-ads-review', component: FacebookComponent, data: { title: 'Leads-Facebook Ads Review' }},
  { path: 'external', component: ExternalComponent, data: { title: 'Leads-External' }},
  { path: 'zip-repository', component: RepositoryComponent, data: { title: 'Leads-Zip Repository' }},

]
