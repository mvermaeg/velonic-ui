import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AboutusRoutingModule } from './aboutus-routing.module';
import { AboutusComponent } from './aboutus.component';
import { ContactFormModule } from '../common/contact-form/contact-form.module';
import { StatsFormModule } from '../common/stats-form/stats-form.module';

@NgModule({
  declarations: [
    AboutusComponent
  ],
  imports: [
    CommonModule,
    AboutusRoutingModule,
    ContactFormModule,
    StatsFormModule

  ],
  providers: [],
})
export class AboutusModule { }
