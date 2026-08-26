import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { StatsFormComponent } from './stats-form.component';
import { ReactiveFormsModule } from '@angular/forms';


@NgModule({
  declarations: [
    StatsFormComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
  ],
  providers: [],
  exports:[StatsFormComponent]
})
export class StatsFormModule { }
