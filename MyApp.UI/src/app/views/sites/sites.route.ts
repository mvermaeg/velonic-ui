import { Route } from '@angular/router'

export const SITES_ROUTES: Route[] = [
  {
    path: '',
    loadComponent: () =>
      import('./sites.component').then((m) => m.SitesComponent),
    data: { title: 'Sites' },
  },
  {
    path: 'pages',
    loadComponent: () =>
      import('./pages/pages.component').then((m) => m.PagesComponent),
    data: { title: 'Sites Pages' },
  },
]