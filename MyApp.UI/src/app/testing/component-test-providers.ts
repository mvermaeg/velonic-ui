import { provideHttpClient } from '@angular/common/http'
import { provideHttpClientTesting } from '@angular/common/http/testing'
import { provideRouter } from '@angular/router'
import { provideStore } from '@ngrx/store'
import { rootReducer } from '../store'
import { provideNoopAnimations } from '@angular/platform-browser/animations'
import { provideToastr } from 'ngx-toastr'

// Test-only dependency graph. No auth effects, production interceptors or real HTTP.
export function componentTestProviders() {
  return [provideRouter([]), provideStore(rootReducer), provideHttpClient(),
    provideHttpClientTesting(), provideNoopAnimations(), provideToastr()]
}
