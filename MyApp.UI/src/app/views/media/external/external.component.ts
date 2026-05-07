import { Component, Directive, EventEmitter, inject, Input, OnInit, Output, PipeTransform, QueryList, ViewChildren } from '@angular/core'
import { PageTitleComponent } from '@common/page-title.component'
import { TableService } from '@core/service/table.service'
import { NgbHighlight, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap'
import { NgbdSortableHeader } from '@core/directive/sortable.directive'
import { DataTableItems, type DataTableItemsType } from './externaldata'
import { CommonModule, DecimalPipe } from '@angular/common'
import { FormsModule } from '@angular/forms'
import { Observable } from 'rxjs'
import { FlatpickrDirective } from '@core/directive/flatpickr.directive'

export type SortColumn = keyof DataTableItemsType | ''
export type SortDirection = 'asc' | 'desc' | ''
const rotate: { [key: string]: SortDirection } = {
  asc: 'desc', desc: '', '': 'asc',
}

export type CustomSortEvent = {
  column: SortColumn
  direction: SortDirection
}

@Directive({
  selector: 'th[sortable]',
  standalone: true,
  host: {
    '[class.asc]': 'direction === "asc"',
    '[class.desc]': 'direction === "desc"',
    '(click)': 'rotate()',
  },
})

export class NgbdCustomSortableHeader {
  @Input() sortable: SortColumn = ''
  @Input() direction: SortDirection = ''
  @Output() sort = new EventEmitter<CustomSortEvent>()

  rotate() {
    this.direction = rotate[this.direction]
    this.sort.emit({ column: this.sortable, direction: this.direction })
  }
}

@Component({
  selector: 'app-external',
  standalone: true,
  imports: [
    PageTitleComponent,
    NgbPaginationModule,
    CommonModule,
    FormsModule,
    NgbHighlight,
    NgbdSortableHeader,
    FlatpickrDirective,
  ],
  templateUrl: './external.component.html',
  styleUrl: './external.component.scss'
})

export class ExternalComponent implements OnInit {
  filter!: string
  page = 1
  pageSize = 4
  collectionSize = DataTableItems.length
  records$: Observable<DataTableItemsType[]>
  total$: Observable<number>
  @ViewChildren(NgbdSortableHeader) headers!: QueryList<NgbdSortableHeader<DataTableItemsType>>
  public tableService = inject(TableService<DataTableItemsType>)
  recordsLength = 0;

  constructor(
    public pipe: DecimalPipe
  ) {
    this.records$ = this.tableService.items$
    this.total$ = this.tableService.total$
  }

  ngOnInit(): void {
    this.tableService.setItems(DataTableItems, 4);

    this.records$.subscribe(data => {
      this.recordsLength = data.length;
    });
  }

  onCompleteSort({ column, direction }: CustomSortEvent) {
    for (const header of this.headers) {
      if (header.sortable !== column) {
        header.direction = ''
      }
    }

    this.tableService.sortColumn = column
    this.tableService.sortDirection = direction
  }

 exportToCSV() {
  // this.records$.subscribe((data) => {

  //   const headers = ['ID', 'Country', 'Area', 'Population'];

  //   const rows = data.map(item => [
  //     item.id,
  //     item.name,
  //     item.area,
  //     item.population
  //   ]);

  //   let csvContent =
  //     'data:text/csv;charset=utf-8,' +
  //     [headers, ...rows].map(e => e.join(',')).join('\n');

  //   const encodedUri = encodeURI(csvContent);

  //   const link = document.createElement('a');
  //   link.setAttribute('href', encodedUri);
  //   link.setAttribute('download', 'countries-data.csv');

  //   document.body.appendChild(link);
  //   link.click();
  //   document.body.removeChild(link);

  // });
}
  


}
