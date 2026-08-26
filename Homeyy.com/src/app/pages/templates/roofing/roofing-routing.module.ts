import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RoofingComponent } from './roofing.component';

const routes: Routes = [{ path: '', component: RoofingComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class RoofingRoutingModule { }
