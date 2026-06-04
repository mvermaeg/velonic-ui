import { Component, inject } from '@angular/core'
import { CommonModule } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { HttpClient } from '@angular/common/http'
import { PageTitleComponent } from '@common/page-title.component'
import { environment } from '../../../../environments/environment'

@Component({
  selector: 'app-leadform',
  standalone: true,
  imports: [CommonModule, FormsModule, PageTitleComponent],
  templateUrl: './leadform.component.html',
  styleUrl: './leadform.component.scss',
})
export class LeadformComponent {
  private http = inject(HttpClient)

  submitting = false
  successMessage = ''
  errorMessage = ''

  postcodeError = ''
  postcodeVerified = false
  expectedLocation: any = null

  form: any = {
    fullName: '',
    email: '',
    phone: '',
    postcode: '',
    city: '',
    state: '',
    country: '',
    campaignName: 'Test Campaign',
    affiliateName: 'Test Affiliate',
    pageName: 'Lead Form',
    affiliateSubId: '',
    address: '',
    step: 1,
    isCompleted: true,
    isTest: false,
  }

  checkPostcode() {
    this.postcodeError = ''
    this.postcodeVerified = false
    this.expectedLocation = null

    const postcode = this.form.postcode?.trim()

    if (!postcode) {
      this.postcodeError = 'Postcode is required.'
      return
    }

    this.http.get<any>(`${environment.apiUrl}/location/postcode/${encodeURIComponent(postcode)}`).subscribe({
      next: (res) => {
        this.expectedLocation = res
        this.postcodeVerified = true

        this.form.city = res.city
        this.form.state = res.stateCode || res.stateName
        this.form.country = res.country
      },
      error: () => {
        this.postcodeError = 'Invalid postcode or postcode not found.'
        this.postcodeVerified = false
        this.form.city = ''
        this.form.state = ''
        this.form.country = ''
      },
    })
  }

  submitLead() {
    this.successMessage = ''
    this.errorMessage = ''

    if (!this.postcodeVerified) {
      this.errorMessage = 'Please verify postcode before submitting.'
      return
    }

    if (!this.form.email && !this.form.phone) {
      this.errorMessage = 'Email or phone is required.'
      return
    }

    this.submitting = true

    this.http.post<any>(`${environment.apiUrl}/public/leads/website`, this.form).subscribe({
      next: (res) => {
        this.successMessage = res?.message || 'Lead submitted successfully.'
        this.submitting = false
        this.resetForm()
      },
      error: (err) => {
        this.errorMessage = err?.error?.message || 'Failed to submit lead.'
        this.submitting = false
      },
    })
  }

  resetForm() {
    this.form = {
      fullName: '',
      email: '',
      phone: '',
      postcode: '',
      city: '',
      state: '',
      country: '',
      campaignName: 'Test Campaign',
      affiliateName: 'Test Affiliate',
      pageName: 'Lead Form',
      affiliateSubId: '',
      address: '',
      step: 1,
      isCompleted: true,
      isTest: false,
    }

    this.postcodeError = ''
    this.postcodeVerified = false
    this.expectedLocation = null
  }
}

// import { Component, inject, OnInit } from '@angular/core'
// import { PageTitleComponent } from '@common/page-title.component'
// import { CommonModule, DecimalPipe } from '@angular/common'
// import { FormsModule } from '@angular/forms'
// import { SearchComponent } from '@/app/tables/search/search.component'
// import { TableService } from '@/app/tables/table.service'
// import { Observable } from 'rxjs'
// import { NgbPagination } from '@ng-bootstrap/ng-bootstrap';
// import { DataTableItems, DataTableItemsType } from '../leadform/leadformdata'
// import { HttpClient } from '@angular/common/http'
// import { environment } from '../../../../environments/environment'


// @Component({
//   selector: 'app-sales',
//   standalone: true,
//   imports: [
//    CommonModule,
//        FormsModule,
//        PageTitleComponent,
//        SearchComponent,
//        NgbPagination,
//        CommonModule
//   ],
//   templateUrl: './leadform.component.html',
//   styleUrl: './leadform.component.scss'
// })



// export class LeadformComponent implements OnInit {
  
//   private http = inject(HttpClient)

// postcodeError = ''
// postcodeVerified = false
// expectedLocation: any = null

// selectedFields: (keyof DataTableItemsType)[] = [
//   "Created",
//   "Lead_ID",
//   "Campaign_ID",
//   "Facebook_Lead_ID",
//   "Facebook_Ad_ID",
//   "Name",
//   "Post_code",
//   "IP_Address",
//   "Phone",
//   "Updated",
//   "Attempts",
//   "Status"
// ]

  
//     table_data = inject(TableService<DataTableItemsType>);
//     records$: Observable<DataTableItemsType[]> = this.table_data.items$;
//     total$!: Observable<number>;
//     recordsLength = 0;
//     pageSize = 8
  
//     constructor(public pipe: DecimalPipe) {
//     }
  
//     ngOnInit(): void {
//       this.table_data.setItems(DataTableItems, 8);
//       this.total$ = this.table_data.total$;
  
//       this.records$ = this.table_data.items$;
//       this.records$.subscribe(data => { this.recordsLength = data.length; });
//     }
  
//     getValue(item: DataTableItemsType, field: keyof DataTableItemsType): any {
//       return item[field];
//     }
  
//     formatHeader(key: string): string {
//       return key.replace(/_/g, ' ');
//     }
  
//     exportToCSV() {
  
//     }


//     checkPostcode() {
//   this.postcodeError = ''
//   this.postcodeVerified = false
//   this.expectedLocation = null

//   const postcode = this.form.postcode?.trim()

//   if (!postcode) {
//     this.postcodeError = 'Postcode is required.'
//     return
//   }

//   this.http.get<any>(`${environment.apiUrl}/location/postcode/${postcode}`).subscribe({
//     next: (res) => {
//       this.expectedLocation = res
//       this.postcodeVerified = true

//       this.form.city = res.city
//       this.form.state = res.stateCode || res.stateName
//       this.form.country = res.country
//     },
//     error: () => {
//       this.postcodeError = 'Invalid postcode or postcode not found.'
//       this.postcodeVerified = false
//     },
//   })
// }
  
//   }