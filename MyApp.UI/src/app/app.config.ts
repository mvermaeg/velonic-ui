import {
  ApplicationConfig,
  importProvidersFrom,
  isDevMode,
  provideZoneChangeDetection,
} from '@angular/core'
import { BrowserModule } from '@angular/platform-browser'
import { provideRouter } from '@angular/router'
import { BrowserAnimationsModule } from '@angular/platform-browser/animations'

import { routes } from './app.routes'
import { provideStore } from '@ngrx/store'
import { rootReducer } from './store'
import { AuthenticationEffects } from '@store/authentication/authentication.effects'
import { provideEffects } from '@ngrx/effects'
import { provideStoreDevtools } from '@ngrx/store-devtools'
import { HTTP_INTERCEPTORS, provideHttpClient, withFetch, withInterceptorsFromDi, } from '@angular/common/http'
import { provideToastr } from 'ngx-toastr'
import { DecimalPipe } from '@angular/common'
 
import { JwtInterceptor } from '@core/helpers/jwt.interceptor'
import { ErrorInterceptor } from '@core/helpers/error.interceptor'

export const appConfig: ApplicationConfig = {
  providers: [
    DecimalPipe,
     
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideStore(rootReducer),
    provideStoreDevtools({ maxAge: 25, logOnly: !isDevMode() }),
    provideEffects(AuthenticationEffects),
    provideHttpClient(withFetch(), withInterceptorsFromDi()),
    importProvidersFrom(BrowserAnimationsModule, BrowserModule),
    provideToastr({}),
  ],
}
