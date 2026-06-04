import { CommonModule } from '@angular/common'
import { Component, OnInit, inject } from '@angular/core'
import { FormsModule } from '@angular/forms'
import { HttpClient, HttpParams } from '@angular/common/http'
import { environment } from '../../../../environments/environment'

@Component({
  selector: 'app-bidding-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bidding-settings.component.html',
})
export class BiddingSettingsComponent implements OnInit {
  private http = inject(HttpClient)
  private apiUrl = environment.apiUrl

  loading = false
  saving = false
  message = ''

  clientId: number | null = 1
  settings: any[] = []

  form = {
    clientId: 1,
    leadType: 'Roofing',
    state: '',
    postcode: '',
    bidAmount: 0,
    dailyCap: 0,
    monthlyCap: 0,
    isExclusive: false,
    isActive: true,
  }

  ngOnInit(): void {
    this.loadSettings()
  }

  loadSettings() {
    if (!this.clientId) return

    this.loading = true

    this.http.get<any[]>(`${this.apiUrl}/client-bidding/${this.clientId}`).subscribe({
      next: (res) => {
        this.settings = res || []
        this.loading = false
      },
      error: () => {
        this.loading = false
        this.message = 'Failed to load bidding settings.'
      },
    })
  }

  saveSetting() {
    this.saving = true
    this.message = ''

    this.form.clientId = Number(this.clientId || this.form.clientId)

    this.http.post<any>(`${this.apiUrl}/client-bidding`, this.form).subscribe({
      next: (res) => {
        this.message = res.message || 'Bidding setting saved.'
        this.saving = false
        this.resetForm()
        this.loadSettings()
      },
      error: (err) => {
        this.message = err?.error?.message || 'Failed to save bidding setting.'
        this.saving = false
      },
    })
  }

  toggleStatus(setting: any) {
    const newStatus = !setting.isActive

    const params = new HttpParams().set('isActive', newStatus)

    this.http.put<any>(`${this.apiUrl}/client-bidding/${setting.id}/status`, null, { params }).subscribe({
      next: () => {
        setting.isActive = newStatus
      },
      error: () => {
        this.message = 'Failed to update status.'
      },
    })
  }

  resetForm() {
    this.form = {
      clientId: Number(this.clientId || 1),
      leadType: 'Roofing',
      state: '',
      postcode: '',
      bidAmount: 0,
      dailyCap: 0,
      monthlyCap: 0,
      isExclusive: false,
      isActive: true,
    }
  }
}