import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { BlogDetailsRoutingModule } from './blog-details-routing.module';
import { BlogDetailsComponent } from './blog-details.component';
import { ReactiveFormsModule } from '@angular/forms';
import { FooterModule } from 'src/app/module/footer/footer.module';

@NgModule({
  declarations: [
    BlogDetailsComponent
  ],
  imports: [
    CommonModule,
    BlogDetailsRoutingModule,
    ReactiveFormsModule,
    FooterModule
  ],
  providers: [],
})
export class BlogDetailsModule { }
