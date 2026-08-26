import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { QoutesRoutingModule } from './qoutes-routing.module';
import { QoutesComponent } from './qoutes.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    QoutesComponent
  ],
  imports: [
    CommonModule,
    QoutesRoutingModule,
    ReactiveFormsModule,
  ],
  providers: [],
})
export class QoutesModule { }
