import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { PageTitleComponent } from '@common/page-title.component'
import { AffiliateClicksService } from '@core/service/affiliate-clicks.service'

@Component({
  selector: 'app-clicks',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './clicks.component.html',
})
export class ClicksComponent implements OnInit {
  private service = inject(AffiliateClicksService)

  records: any[] = []
  total = 0
  loading = false
  errorMessage = ''

  filters: any = {
    search: '',
    campaignName: '',
    affiliateId: '',
    page: 1,
    pageSize: 20,
  }

  ngOnInit(): void {
    this.load()
  }

  load() {
    this.loading = true
    this.errorMessage = ''

    this.service.getClicks(this.filters).subscribe({
      next: (res) => {
        this.records = res.data || []
        this.total = res.total || 0
        this.loading = false
      },
      error: () => {
        this.errorMessage = 'Unable to load affiliate clicks.'
        this.loading = false
      },
    })
  }

  clearFilters() {
    this.filters.search = ''
    this.filters.campaignName = ''
    this.filters.affiliateId = ''
    this.filters.page = 1
    this.load()
  }
}