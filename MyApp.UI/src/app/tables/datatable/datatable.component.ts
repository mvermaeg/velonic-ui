import { Component, Directive, EventEmitter, Input, OnInit, Output, QueryList, ViewChildren, inject } from '@angular/core'
import { CommonModule } from '@angular/common'
import { NgbHighlight, NgbPagination } from '@ng-bootstrap/ng-bootstrap';
import { Observable } from 'rxjs';

import { NgbdSortableHeader } from '@core/directive/sortable.directive';
import { SourceData, SourceDataType } from '../sourcedata';
import { TableService } from '../table.service';
import { LeadsFormComponent } from '@/app/pages/leads-form/leads-form.component';

export type SortColumn = keyof SourceDataType | ''
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
  selector: 'app-datatable',
  standalone: true,
  imports: [CommonModule,
    NgbPagination,
    // NgbdSortableHeader,
    LeadsFormComponent
  ],
  templateUrl: './datatable.component.html',
  styleUrl: './datatable.component.scss'
})

export class DatatableComponent implements OnInit {

  @Input() selectedFields: (keyof SourceDataType)[] = [];

  table_data = inject(TableService<SourceDataType>);
  records$: Observable<SourceDataType[]> = this.table_data.items$;
  total$!: Observable<number>;
  recordsLength = 0;
  pageSize = 8

  @ViewChildren(NgbdSortableHeader) headers!: QueryList<NgbdSortableHeader<SourceDataType>>

  ngOnInit(): void {
    this.table_data.setItems(SourceData, 8);
    this.total$ = this.table_data.total$;

    this.records$ = this.table_data.items$;
    this.records$.subscribe(data => { this.recordsLength = data.length; });

  }

  getValue(item: SourceDataType, field: keyof SourceDataType): any {
    return item[field];
  }

  onSort({ column, direction }: CustomSortEvent) {
    for (const header of this.headers) {
      if (header.sortable !== column) {
        header.direction = ''
      }
    }

    this.table_data.sortColumn = column
    this.table_data.sortDirection = direction
  }


  formatHeader(key: string): string {
    return key.replace(/_/g, ' ');
  }

  exportToCSV() {

  }

  get startIndex(): number {
    return (this.table_data.page - 1) * this.table_data.pageSize + 1;
  }

  get endIndex(): number {
    return (this.table_data.page - 1) * this.table_data.pageSize + (this.recordsLength || 0);
  }

}