import { Component } from '@angular/core'
import { CustomCardPortletComponent } from '@component/custom-card-portlet/custom-card-portlet.component'
import { lineChartOpts } from '@views/dashboard/data'
import { NgApexchartsModule } from 'ng-apexcharts'

@Component({
  selector: 'dashboard-weekly-report',
  standalone: true,
  imports: [NgApexchartsModule, CustomCardPortletComponent],
  templateUrl: './weekly-report.component.html',
  styles: ``,
})
export class WeeklyReportComponent {
lineChart = lineChartOpts


}
