import { inject } from '@angular/core'
import { RedirectCommand, Router, Routes, type UrlTree } from '@angular/router'
import { AuthenticationService } from '@core/service/auth.service'
import { LayoutComponent } from '@layouts/layout/layout.component'


export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [
      (url: any) => {
        const router = inject(Router)
        const authService = inject(AuthenticationService)
        if (!authService.session) {
          return router.createUrlTree(['/auth/login'], {
            queryParams: { returnUrl: url._routerState.url },
          })
        }
        return true
      },
    ],
    // canActivate: [
    //   () => {
    //     const currentUser = inject(AuthenticationService).session
    //     const router: Router = inject(Router)
    //     if (currentUser) return true
    //     const urlTree: UrlTree = router.parseUrl('/auth/signin')
    //     return new RedirectCommand(urlTree, { skipLocationChange: true })
    //   },
    // ],
    
    loadChildren: () =>
      import('./views/views.route').then((mod) => mod.VIEW_ROUTES),
  },

  {
    path: 'auth',
    loadChildren: () =>
      import('./views/auth/auth.route').then((mod) => mod.AUTH_ROUTES),
  },
  {
  path: 'website-templates',
  loadComponent: () =>
    import('./views/website-templates/website-templates.component')
      .then(m => m.WebsiteTemplatesComponent)
},
{
  path: 'w/:slug/:pageSlug',
  loadComponent: () =>
    import('./website-engine/public-site-renderer/public-site-renderer.component')
      .then(m => m.PublicSiteRendererComponent),
},
{
  path: 'w/:slug',
  loadComponent: () =>
    import('./website-engine/public-site-renderer/public-site-renderer.component')
      .then(m => m.PublicSiteRendererComponent),
},
{
  path: 'w/:slug',
  loadComponent: () =>
    import('./website-engine/public-site-renderer/public-site-renderer.component')
      .then(m => m.PublicSiteRendererComponent),
},
{
  path: 'w/:slug/:pageSlug',
  loadComponent: () =>
    import('./website-engine/public-site-renderer/public-site-renderer.component')
      .then(m => m.PublicSiteRendererComponent),
},
{
  path: 'sites/sections',
  loadComponent: () =>
    import('./views/site-page-sections/site-page-sections.component').then(
      (m) => m.SitePageSectionsComponent
    ),
},
{
  path: 'sites/settings',
  loadComponent: () =>
    import('./views/site-settings/site-settings.component').then(
      (m) => m.SiteSettingsComponent
    ),
},
{
  path: 'site-preview/:domain',
  loadComponent: () =>
    import('./website-engine/public-site-renderer/public-site-renderer.component')
      .then(m => m.PublicSiteRendererComponent),
},
 

  // {
  //   path: 'error',
  //   loadChildren: () =>
  //     import('./views/other-pages/other-page.route').then(
  //       (mod) => mod.OTHER_PAGES_ROUTES
  //     ),
  // },
  // {
  //   path: 'maintenance',
  //   component: MaintenanceComponent,
  //   data: { title: 'Maintenance' },
  // },
]
