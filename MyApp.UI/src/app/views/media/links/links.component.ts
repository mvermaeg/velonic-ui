import { Component, inject, OnInit } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { CommonModule, DecimalPipe } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { SearchComponent } from '@/app/tables/search/search.component'
import { Observable } from 'rxjs'
import { NgbPagination } from '@ng-bootstrap/ng-bootstrap';
import { DataTableItems, DataTableItemsType } from './linksdata'
import { TableService } from '@/app/tables/table.service'

@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [
    PageTitleComponent,
    CommonModule,
    FormsModule,
    SearchComponent,
    NgbPagination,
  ],
  templateUrl: './links.component.html',
  styleUrl: './links.component.scss'
})

export class LinksComponent implements OnInit {
  selectedFields: (keyof DataTableItemsType)[] =["Key", "Campaign_Name", "Source_Name", "Page_Name", "Next_Page_Name", "Domain_Name", "Created", "Updated"]
 
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