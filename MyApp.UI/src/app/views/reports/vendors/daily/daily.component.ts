import { Component, OnInit, inject } from '@angular/core'
import { CommonModule } from '@angular/common'
import { PageTitleComponent } from '@common/page-title.component'
import { ReportsService } from '@/app/core/service/reports.service'

@Component({
  selector: 'app-report-vendors-daily',
  standalone: true,
  imports: [
    CommonModule,
    PageTitleComponent
  ],
  templateUrl: './daily.component.html',
  styleUrl: './daily.component.scss'
})
export class DailyComponent implements OnInit {

  private reportsService = inject(ReportsService)

  loading = false

  rows: any[] = []

  ngOnInit(): void {
    this.loadReport()
  }

  loadReport() {
    this.loading = true

    this.reportsService.getVendorDaily()
      .subscribe({
        next: (res: any) => {
          this.rows = res || []
          this.loading = false
        },
        error: () => {
          this.loading = false
        }
      })
  }
}