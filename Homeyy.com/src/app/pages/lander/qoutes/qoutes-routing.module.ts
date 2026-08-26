import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { QoutesComponent } from './qoutes.component';

const routes: Routes = [{ path: '', component: QoutesComponent }];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class QoutesRoutingModule { }
