import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BodyComponent } from './body/body.component';
import { IdlePreloadStrategy } from './shared/idle-preload.strategy';

const routes: Routes = [
  {
    path: '',
    component: BodyComponent,
    children: [
      { path: '', loadChildren: () => import('./pages/home/home.module').then(m => m.HomeModule) },
      { path: 'blogs', loadChildren: () => import('./pages/blogs/blogs.module').then(m => m.BlogsModule) },
      { path: 'contact-us', loadChildren: () => import('./pages/contactus/contactus.module').then(m => m.ContactusModule) },
      { path: 'about-us', loadChildren: () => import('./pages/aboutus/aboutus.module').then(m => m.AboutusModule) },
      { path: 'services', loadChildren: () => import('./pages/services/services.module').then(m => m.ServicesModule) },
      { path: 'services/:qoute-name', loadChildren: () => import('./pages/lander/qoutes/qoutes.module').then(m => m.QoutesModule) },
      { path: 'terms-conditions', loadChildren: () => import('./pages/terms-conditions/terms-conditions.module').then(m => m.TermsConditionsModule) },
      { path: 'privacy-policy', loadChildren: () => import('./pages/privacy-policy/privacy-policy.module').then(m => m.PrivacyPolicyModule) },
      { path: ':qoute-name/thank-you', loadChildren: () => import('./pages/thank-you/thank-you.module').then(m => m.ThankYouModule) },
      { path: ':page-name', loadChildren: () => import('./pages/templates/roofing/roofing.module').then(m => m.RoofingModule) },
    ]
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {
    scrollPositionRestoration: 'enabled',
    anchorScrolling: 'enabled',
    preloadingStrategy: IdlePreloadStrategy
  })],
  exports: [RouterModule]
})

export class AppRoutingModule { }
