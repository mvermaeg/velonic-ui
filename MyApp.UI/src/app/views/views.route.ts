import { Route } from '@angular/router'
import { DashboardComponent } from './dashboard/dashboard.component'

export const VIEW_ROUTES: Route[] = [

    {
    path: 'dashboard',
    component: DashboardComponent,
    data: { title: 'Dashboard' },
  },

    {
        path: 'leads',
        loadChildren: () =>
            import('./leads/leads.route').then((mod) => mod.LEADS_ROUTES),
    },
    {
        path: 'sites',
        loadChildren: () =>
            import('./sites/sites.route').then((mod) => mod.SITES_ROUTES),
    },
    {
  path: 'website-templates',
  loadComponent: () =>
    import('./website-templates/website-templates.component')
      .then(m => m.WebsiteTemplatesComponent),
},
    {
        path: 'media-buying',
        loadChildren: () =>
            import('./media/media.route').then((mod) => mod.MEDIA_ROUTES),
    },

     {
        path: 'affiliates',
        loadChildren: () =>
            import('./affiliates/affiliates.route').then((mod) => mod.AFFILIATES_ROUTES),
    },
     {
        path: 'vendors',
        loadChildren: () =>
            import('./vendors/vendors.route').then((mod) => mod.VENDORS_ROUTES),
    },
    {
        path: 'reports',
        loadChildren: () =>
            import('./reports/reports.route').then((mod) => mod.REPORTS_ROUTES),
    },
{
  path: 'system/users',
  loadComponent: () =>
    import('./system/users/users.component').then((m) => m.UsersComponent),
  data: { title: 'User Management' },
},
{
  path: 'clients',
  loadChildren: () =>
    import('./clients/clients.route').then((mod) => mod.CLIENTS_ROUTES),
},
{
  path: 'sites/themes',
  loadComponent: () =>
    import('./website-themes/website-themes.component')
      .then(m => m.WebsiteThemesComponent),
},
{
  path: 'sites/pages',
  loadComponent: () =>
    import('./site-pages/site-pages.component').then(
      (m) => m.SitePagesComponent
    ),
},
{
  path: 'sites/settings',
  loadComponent: () =>
    import('./site-settings/site-settings.component').then(
      (m) => m.SiteSettingsComponent
    ),
},
{
  path: 'sites/domains',
  loadComponent: () =>
    import('./site-domains/site-domains.component').then(
      (m) => m.SiteDomainsComponent
    ),
},
{
  path: 'sites/pixels',
  loadComponent: () =>
    import('./site-pixels/site-pixels.component').then(
      (m) => m.SitePixelsComponent
    ),
},
{
  path: 'sites/forms',
  loadComponent: () =>
    import('./site-forms/site-forms.component').then(
      (m) => m.SiteFormsComponent
    ),
},
{
  path: 'sites/edit/:id',
  loadComponent: () =>
    import('./site-edit/site-edit.component').then(
      (m) => m.SiteEditComponent
    ),
},
]
