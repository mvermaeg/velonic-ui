import { Component, inject, OnInit } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { CommonModule, DecimalPipe } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { SearchComponent } from '@/app/tables/search/search.component'
import { Observable } from 'rxjs'
import { NgbPagination } from '@ng-bootstrap/ng-bootstrap';
import { DataTableItems, DataTableItemsType } from './leadqualitydata'
import { TableService } from '@/app/tables/table.service'

@Component({
  selector: 'app-commissions',
  standalone: true,
  imports: [
    PageTitleComponent,
    CommonModule,
    FormsModule,
    SearchComponent,
    NgbPagination,
  ],
  templateUrl: './leadquality.component.html',
  styleUrl: './leadquality.component.scss'
})

export class LeadqualityComponent implements OnInit {
  selectedFields: (keyof DataTableItemsType)[] = ["Source_Name", "Campaign_Name", "Invalid_Reason", "UUID", "Tier", "Trusted_From_Token", "Phone", "Landing_Page", "Domain"]
     
       table_data = inject(TableService<DataTableItemsType>);
       records$: Observable<DataTableItemsType[]> = this.table_data.items$;
       total$!: Observable<number>;
       recordsLength = 0;
       pageSize = 8
     
       constructor(public pipe: DecimalPipe) {
       }
     
       ngOnInit(): void {
         this.table_data.setItems(DataTableItems, 8);
         this.total$ = this.table_data.total$;
     
         this.records$ = this.table_data.items$;
         this.records$.subscribe(data => { this.recordsLength = data.length; });
       }
     
       getValue(item: DataTableItemsType, field: keyof DataTableItemsType): any {
         return item[field];
       }
     
       formatHeader(key: string): string {
         return key.replace(/_/g, ' ');
       }
     
       exportToCSV() {
     
       }
     
     }