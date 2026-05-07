
import { Component, inject, OnInit } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { CommonModule, DecimalPipe } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { SearchComponent } from '@/app/tables/search/search.component'
import { Observable } from 'rxjs'
import { NgbPagination } from '@ng-bootstrap/ng-bootstrap';
import { DataTableItems, DataTableItemsType } from './generaldata'
import { TableService } from '@/app/tables/table.service'
@Component({
  selector: 'app-general',
  standalone: true,
  imports: [
    PageTitleComponent,
    CommonModule,
    FormsModule,
    SearchComponent,
    NgbPagination
  ],
  templateUrl: './general.component.html',
  styleUrl: './general.component.scss'
})

export class GeneralComponent implements OnInit {
  selectedFields: (keyof DataTableItemsType)[] = ["Date", "Revenue", "Ad_Revenue", "MB_Revenue", "AM_MB_Revenue", "VN_Revenue", "Ad_VN_Revenue", "AF_Revenue", "Ad_AF_Revenue", "Cost", "Profit", "Ad_Profit", "MB_Profit", "Ad_MB_Profit", "VN_Profit", "Ad_VN_Profit", "AF_Profit", "Ad_AF_Profit", "Sales", "Posts", "Ping"]

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
