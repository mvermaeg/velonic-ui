import { inject } from '@angular/core'
import { Router, Routes } from '@angular/router'
import { AuthenticationService } from '@core/service/auth.service'
import { LayoutComponent } from '@layouts/layout/layout.component'

export const routes: Routes = [
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
    path: 'site-preview/:domain',
    loadComponent: () =>
      import('./website-engine/public-site-renderer/public-site-renderer.component')
        .then(m => m.PublicSiteRendererComponent),
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
        .then(m => m.WebsiteTemplatesComponent),
  },

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
    loadChildren: () =>
      import('./views/views.route').then((mod) => mod.VIEW_ROUTES),
  },

  {
    path: '**',
    redirectTo: 'dashboard',
  },
]


// import { inject } from '@angular/core'
// import { Router, Routes } from '@angular/router'
// import { AuthenticationService } from '@core/service/auth.service'
// import { LayoutComponent } from '@layouts/layout/layout.component'

// export const routes: Routes = [
//   {
//     path: '',
//     redirectTo: 'dashboard',
//     pathMatch: 'full',
//   },

//   {
//     path: 'auth',
//     loadChildren: () =>
//       import('./views/auth/auth.route').then((mod) => mod.AUTH_ROUTES),
//   },

//   {
//     path: 'w/:slug/:pageSlug',
//     loadComponent: () =>
//       import('./website-engine/public-site-renderer/public-site-renderer.component')
//         .then(m => m.PublicSiteRendererComponent),
//   },

//   {
//     path: 'w/:slug',
//     loadComponent: () =>
//       import('./website-engine/public-site-renderer/public-site-renderer.component')
//         .then(m => m.PublicSiteRendererComponent),
//   },

//   {
//     path: 'site-preview/:domain',
//     loadComponent: () =>
//       import('./website-engine/public-site-renderer/public-site-renderer.component')
//         .then(m => m.PublicSiteRendererComponent),
//   },

//   {
//     path: 'website-templates',
//     loadComponent: () =>
//       import('./views/website-templates/website-templates.component')
//         .then(m => m.WebsiteTemplatesComponent),
//   },

//   {
//     path: '',
//     component: LayoutComponent,
//     canActivate: [
//       (url: any) => {
//         const router = inject(Router)
//         const authService = inject(AuthenticationService)

//         if (!authService.session) {
//           return router.createUrlTree(['/auth/login'], {
//             queryParams: { returnUrl: url._routerState.url },
//           })
//         }

//         return true
//       },
//     ],
//     loadChildren: () =>
//       import('./views/views.route').then((mod) => mod.VIEW_ROUTES),
//   },
// ]

// // import { inject } from '@angular/core'
// // import { Router, Routes } from '@angular/router'
// // import { AuthenticationService } from '@core/service/auth.service'
// // import { LayoutComponent } from '@layouts/layout/layout.component'

// // export const routes: Routes = [
// //   {
// //     path: '',
// //     pathMatch: 'full',
// //     loadComponent: () =>
// //       import('./website-engine/public-site-renderer/public-site-renderer.component')
// //         .then(m => m.PublicSiteRendererComponent),
// //   },

// //   {
// //     path: '',
// //     component: LayoutComponent,
// //     canActivate: [
// //       (url: any) => {
// //         const router = inject(Router)
// //         const authService = inject(AuthenticationService)

// //         if (!authService.session) {
// //           return router.createUrlTree(['/auth/login'], {
// //             queryParams: { returnUrl: url._routerState.url },
// //           })
// //         }

// //         return true
// //       },
// //     ],
// //     loadChildren: () =>
// //       import('./views/views.route').then((mod) => mod.VIEW_ROUTES),
// //   },

// //   {
// //     path: 'auth',
// //     loadChildren: () =>
// //       import('./views/auth/auth.route').then((mod) => mod.AUTH_ROUTES),
// //   },

// //   {
// //     path: 'website-templates',
// //     loadComponent: () =>
// //       import('./views/website-templates/website-templates.component')
// //         .then(m => m.WebsiteTemplatesComponent),
// //   },

// //   {
// //     path: 'w/:slug/:pageSlug',
// //     loadComponent: () =>
// //       import('./website-engine/public-site-renderer/public-site-renderer.component')
// //         .then(m => m.PublicSiteRendererComponent),
// //   },

// //   {
// //     path: 'w/:slug',
// //     loadComponent: () =>
// //       import('./website-engine/public-site-renderer/public-site-renderer.component')
// //         .then(m => m.PublicSiteRendererComponent),
// //   },

// //   {
// //     path: 'site-preview/:domain',
// //     loadComponent: () =>
// //       import('./website-engine/public-site-renderer/public-site-renderer.component')
// //         .then(m => m.PublicSiteRendererComponent),
// //   },
// // ]