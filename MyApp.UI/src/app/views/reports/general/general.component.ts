import { Component, OnInit, inject } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { CommonModule } from '@angular/common'
import { ReportsService } from '@core/service/reports.service'

@Component({
  selector: 'app-general',
  standalone: true,
  imports: [PageTitleComponent, CommonModule],
  templateUrl: './general.component.html',
  styleUrl: './general.component.scss',
})
export class GeneralComponent implements OnInit {
  private reportsService = inject(ReportsService)

  loading = false
  report: any = {}

  ngOnInit(): void {
    this.loadReport()
  }

  loadReport() {
    this.loading = true

    this.reportsService.getGeneral().subscribe({
      next: (res) => {
        this.report = res || {}
        this.loading = false
      },
      error: () => {
        this.loading = false
      },
    })
  }
}