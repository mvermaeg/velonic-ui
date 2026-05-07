import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { PageTitleComponent } from '@common/page-title.component'
import { LeadReturnsService } from '@core/service/lead-returns.service'

@Component({
  selector: 'app-lead-returns',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './returns.component.html',
})
export class ReturnsComponent implements OnInit {
  private service = inject(LeadReturnsService)

  records: any[] = []
  total = 0
  loading = false
  errorMessage = ''

  filters: any = {
    search: '',
    status: '',
    clientId: '',
    page: 1,
    pageSize: 20,
  }

  ngOnInit(): void {
    this.load()
  }

  load() {
    this.loading = true
    this.errorMessage = ''

    this.service.getReturns(this.filters).subscribe({
      next: (res) => {
        this.records = res.data || []
        this.total = res.total || 0
        this.loading = false
      },
      error: () => {
        this.errorMessage = 'Unable to load returns.'
        this.loading = false
      },
    })
  }

  review(id: number, status: string) {
    if (!confirm(`Are you sure you want to ${status} this return?`)) return

    this.service.reviewReturn(id, status).subscribe({
      next: () => this.load(),
      error: (err) => alert(err?.error?.message || 'Unable to update return.'),
    })
  }
}