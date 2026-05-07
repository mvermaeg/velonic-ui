import { Component } from '@angular/core'
import { ProjectsComponent } from './components/projects/projects.component'
import { WeeklyReportComponent } from './components/weekly-report/weekly-report.component'
import { PageTitleComponent } from '@common/page-title.component'
import { StateComponent } from './components/state/state.component'

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    PageTitleComponent,
    ProjectsComponent,
    StateComponent,
    WeeklyReportComponent,
  ],
  templateUrl: './dashboard.component.html',
  styles: ``,
})
export class DashboardComponent { }
