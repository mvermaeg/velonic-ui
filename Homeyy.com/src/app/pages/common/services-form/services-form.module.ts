import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ServicesFormComponent } from './services-form.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';


@NgModule({
  declarations: [
    ServicesFormComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule
  ],
  providers: [],
  exports:[ServicesFormComponent]
})
export class ServicesFormModule { }
