import { Component, inject, OnInit, TemplateRef } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { CommonModule, DecimalPipe } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { SearchComponent } from '@/app/tables/search/search.component'
import { TableService } from '@/app/tables/table.service'
import { Observable } from 'rxjs'
import { NgbPagination,NgbNavModule} from '@ng-bootstrap/ng-bootstrap';
import { DataTableItems, DataTableItemsType } from './subiddata'
import { NgbModal, type NgbModalOptions,} from '@ng-bootstrap/ng-bootstrap'
import { LeadsFormComponent } from '@/app/pages/leads-form/leads-form.component'

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [
    PageTitleComponent,
    CommonModule,
    FormsModule,
    SearchComponent,
    NgbPagination,
    NgbNavModule,
    LeadsFormComponent
  ],
  templateUrl: './subid.component.html',
  styleUrl: './subid.component.scss'
})

export class SubidComponent implements OnInit {
  selectedFields: (keyof DataTableItemsType)[] = ["Sub_ID", "Affiliate_Name", "S1", "Source", "UTM_Source", "UTM_Campaign"]
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

  get startIndex(): number {
  return (this.table_data.page - 1) * this.table_data.pageSize + 1;
}

get endIndex(): number {
  return (this.table_data.page - 1) * this.table_data.pageSize + (this.recordsLength || 0);
}

  exportToCSV() {

  }


}
