import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ImageCompareComponent } from './image-compare.component';
import { ReactiveFormsModule } from '@angular/forms';


@NgModule({
  declarations: [
    ImageCompareComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
  ],
  providers: [],
  exports:[ImageCompareComponent]
})
export class ImageCompareModule { }
