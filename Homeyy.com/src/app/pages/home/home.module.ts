import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { HomeRoutingModule } from './home-routing.module';
import { HomeComponent } from './home.component';
import { StatsFormModule } from '../common/stats-form/stats-form.module';
import { ImageCompareModule } from '../common/image-compare/image-compare.module';
import { ContactFormModule } from '../common/contact-form/contact-form.module';

@NgModule({
  declarations: [
    HomeComponent
  ],
  imports: [
    CommonModule,
    HomeRoutingModule,
    StatsFormModule,
    ImageCompareModule,
    ContactFormModule
  ],
  providers: [],
})
export class HomeModule { }
