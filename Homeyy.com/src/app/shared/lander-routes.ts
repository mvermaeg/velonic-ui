import { NavigationEnd, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { distinctUntilChanged, filter, map, startWith } from 'rxjs/operators';

// Landing pages render a stripped down chrome: logo only header, copyright only footer.
export const LANDER_ROUTES: readonly string[] = [
  '/hvac-contractors',
  '/bathroom-remodelling-contractors',
  '/kitchen-modeling-contractors',
  '/plumbing-contractors',
  '/window-installation-contractors',
  '/door-installation-contractors',
  '/flooring-contractors',
  '/gutter-installation-contractors',
  '/fencing-installation-contractors',
  '/solar-installation-contractors',
  '/roofing-installation-contractors',
  '/home-security-contractors',
  '/siding-installation-contractors',
];

// Emits on subscribe with the current route, then on every completed navigation.
export function isLanderRoute(router: Router): Observable<boolean> {
  return router.events.pipe(
    filter((event): event is NavigationEnd => event instanceof NavigationEnd),
    map(event => event.urlAfterRedirects),
    startWith(router.url),
    map(url => LANDER_ROUTES.includes(url)),
    distinctUntilChanged()
  );
}
