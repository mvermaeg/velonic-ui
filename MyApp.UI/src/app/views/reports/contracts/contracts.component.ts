
import { Component, inject, OnInit } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { CommonModule, DecimalPipe } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { SearchComponent } from '@/app/tables/search/search.component'
import { Observable } from 'rxjs'
import { NgbPagination } from '@ng-bootstrap/ng-bootstrap';
import { DataTableItems, DataTableItemsType } from './contractsdata'
import { TableService } from '@/app/tables/table.service'
@Component({
  selector: 'app-report-contracts',
  standalone: true,
  imports: [
    PageTitleComponent,
    CommonModule,
    FormsModule,
    SearchComponent,
    NgbPagination
  ],
  templateUrl: './contracts.component.html',
  styleUrl: './contracts.component.scss'
})

export class ContractsComponent implements OnInit {
  selectedFields: (keyof DataTableItemsType)[] = ["Date", "Campaign_Name", "Contract_Name", "Pings_Sent", "Pings_Accepted", "Pings_AR", "Posts_Sent", "Posts_Accepted", "Posts_Rejected", "Posts_AR", "Posts_RR", "RPL", "Adj_RPL", "ABL", "Revenue", "Adj_Revenue"]

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
