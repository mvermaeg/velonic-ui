import { Injectable } from '@angular/core';
import { PreloadingStrategy, Route } from '@angular/router';
import { Observable, of, timer } from 'rxjs';
import { mergeMap } from 'rxjs/operators';

/**
 * Preloads the lazy routes, but only once the first page has settled.
 *
 * PreloadAllModules starts downloading every feature module the moment the app
 * boots, which competes with the images and fonts of the page the visitor is
 * actually looking at. Waiting a few seconds keeps navigation instant without
 * paying for it during the initial render.
 */
@Injectable({ providedIn: 'root' })
export class IdlePreloadStrategy implements PreloadingStrategy {

  private static readonly DELAY_MS = 5000;

  preload(route: Route, load: () => Observable<any>): Observable<any> {

    // Respect the visitor's data saver settings when the browser exposes them
    const connection = (navigator as any).connection;

    if (connection && (connection.saveData || /2g/.test(connection.effectiveType || ''))) {
      return of(null);
    }

    return timer(IdlePreloadStrategy.DELAY_MS).pipe(mergeMap(() => load()));
  }
}
