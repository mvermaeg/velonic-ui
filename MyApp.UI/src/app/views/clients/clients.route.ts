import { Route } from '@angular/router'

export const CLIENTS_ROUTES: Route[] = [
  {
    path: '',
    loadComponent: () =>
      import('./clients.component').then((m) => m.ClientsComponent),
    data: { title: 'Clients' },
  },
  {
    path: 'bidding',
    loadComponent: () =>
      import('./bidding/bidding.component').then((m) => m.BiddingComponent),
    data: { title: 'Contract Bidding' },
  },
  {
    path: 'deliveries',
    loadComponent: () =>
      import('./deliveries/deliveries.component').then((m) => m.DeliveriesComponent),
    data: { title: 'Deliveries' },
  },
]