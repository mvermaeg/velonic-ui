import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { PageTitleComponent } from '@common/page-title.component'
import { LeadRejectionsService } from '@core/service/lead-rejections.service'

@Component({
  selector: 'app-lead-rejections',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './rejections.component.html',
})
export class RejectionsComponent implements OnInit {
  private service = inject(LeadRejectionsService)

  records: any[] = []
  total = 0
  loading = false
  errorMessage = ''
  successMessage = ''

  filters: any = {
    search: '',
    source: '',
    page: 1,
    pageSize: 20,
  }

  newRejection: any = {
    leadId: '',
    rejectionReason: '',
    rejectionSource: 'Admin',
    notes: '',
    createdByUserId: null,
  }

  ngOnInit(): void {
    this.load()
  }

  load() {
    this.loading = true
    this.errorMessage = ''

    this.service.getRejections(this.filters).subscribe({
      next: (res) => {
        this.records = res.data || []
        this.total = res.total || 0
        this.loading = false
      },
      error: () => {
        this.errorMessage = 'Unable to load rejections.'
        this.loading = false
      },
    })
  }

  clearFilters() {
    this.filters.search = ''
    this.filters.source = ''
    this.filters.page = 1
    this.load()
  }

  createRejection() {
    this.errorMessage = ''
    this.successMessage = ''

    if (!this.newRejection.leadId) {
      this.errorMessage = 'Lead ID is required.'
      return
    }

    if (!this.newRejection.rejectionReason.trim()) {
      this.errorMessage = 'Rejection reason is required.'
      return
    }

    const payload = {
      leadId: Number(this.newRejection.leadId),
      rejectionReason: this.newRejection.rejectionReason,
      rejectionSource: this.newRejection.rejectionSource,
      notes: this.newRejection.notes,
      createdByUserId: null,
    }

    this.service.createRejection(payload).subscribe({
      next: () => {
        this.successMessage = 'Lead rejection created successfully.'
        this.newRejection = {
          leadId: '',
          rejectionReason: '',
          rejectionSource: 'Admin',
          notes: '',
          createdByUserId: null,
        }
        this.load()
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Unable to create rejection.'
      },
    })
  }
}