import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import {
  RoofingRoutingModule
} from './roofing-routing.module';

import {
  RoofingComponent
} from './roofing.component';

import {
  StatsFormModule
} from '../../common/stats-form/stats-form.module';

import {
  ServicesFormModule
} from '../../common/services-form/services-form.module';

@NgModule({
  declarations: [
    RoofingComponent
  ],

  imports: [
    CommonModule,
    RoofingRoutingModule,
    StatsFormModule,
    ServicesFormModule
  ],

  providers: []
})
export class RoofingModule {}


// import { NgModule } from '@angular/core';
// import { CommonModule } from '@angular/common';

// import { RoofingRoutingModule } from './roofing-routing.module';
// import { RoofingComponent } from './roofing.component';
// import { StatsFormModule } from '../../common/stats-form/stats-form.module';
// import { ServicesFormModule } from '../../common/services-form/services-form.module';

// @NgModule({
//   declarations: [
//     RoofingComponent
//   ],
//   imports: [
//     CommonModule,
//     RoofingRoutingModule,
//     StatsFormModule,
//     ServicesFormModule
//   ],
//   providers: [],
// })
// export class RoofingModule { }
