import { Component, OnInit, inject } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { CommonModule } from '@angular/common'
import { ReportsService } from '@core/service/reports.service'

@Component({
  selector: 'app-report-contracts',
  standalone: true,
  imports: [PageTitleComponent, CommonModule],
  templateUrl: './contracts.component.html',
  styleUrl: './contracts.component.scss',
})
export class ContractsComponent implements OnInit {
  private reportsService = inject(ReportsService)

  loading = false
  rows: any[] = []

  ngOnInit(): void {
    this.loadReport()
  }

  loadReport() {
    this.loading = true

    this.reportsService.getContracts().subscribe({
      next: (res) => {
        this.rows = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
      },
    })
  }
}