import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { debounceTime, switchMap } from 'rxjs/operators';

export interface SearchResult<T> {
  items: T[];
  total: number;
}

export type SortDirection = 'asc' | 'desc' | '';
export type SortColumn<T> = keyof T | '';

interface State<T> {
  page: number;
  pageSize: number;
  searchTerm: string;
  sortColumn: SortColumn<T>;
  sortDirection: SortDirection;
  source: string;
  tier: string;
  campaign:string;
}

function compare(v1: any, v2: any) {
  return v1 < v2 ? -1 : v1 > v2 ? 1 : 0;
}

@Injectable({ providedIn: 'root' })
export class TableService<T> {

  private _data: T[] = [];

  private _items$ = new BehaviorSubject<T[]>([]);
  private _total$ = new BehaviorSubject<number>(0);
  private _loading$ = new BehaviorSubject<boolean>(false);
  private _search$ = new BehaviorSubject<void>(undefined);

  items$ = this._items$.asObservable();
  total$ = this._total$.asObservable();
  loading$ = this._loading$.asObservable();

  private _state: State<T> = {
    page: 1,
    pageSize: 4,
    searchTerm: '',
    sortColumn: '',
    sortDirection: '',
    source: '',
    tier: '',
    campaign:''
  };

  constructor() {
    this._search$
      .pipe(
        debounceTime(200),
        switchMap(() => this._search())
      )
      .subscribe(result => {
        this._items$.next(result.items);
        this._total$.next(result.total);
      });

    this._search$.next();
  }

  setItems(data: T[], pageSize: number) {
    this._data = data;
    this._set({ pageSize });
  }

  get page() { return this._state.page; }
  set page(page: number) { this._set({ page }); }

  get pageSize() { return this._state.pageSize; }
  set pageSize(pageSize: number) { this._set({ pageSize }); }

  get searchTerm() { return this._state.searchTerm; }
  set searchTerm(searchTerm: string) { this._set({ searchTerm, page: 1 }); }

  get sortColumn() { return this._state.sortColumn; }
  set sortColumn(sortColumn: SortColumn<T>) { this._set({ sortColumn }); }

  get sortDirection() { return this._state.sortDirection; }
  set sortDirection(sortDirection: SortDirection) { this._set({ sortDirection }); }

  get source() { return this._state.source; }
  set source(source: string) { this._set({ source, page: 1 }); }

  get tier() { return this._state.tier; }
  set tier(tier: string) { this._set({ tier, page: 1 }); }

   get campaign() { return this._state.campaign; }
  set campaign(campaign: string) { this._set({ campaign, page: 1 }); }

  private _set(patch: Partial<State<T>>) {
    Object.assign(this._state, patch);
    this._search$.next();
  }

  private _search(): Observable<SearchResult<T>> {
    this._loading$.next(true);

    return new Observable(observer => {

      let data = [...this._data];

      // Search
      if (this._state.searchTerm) {
        const term = this._state.searchTerm.toLowerCase();
        data = data.filter((item: any) =>
          Object.values(item).some(val =>
            String(val).toLowerCase().includes(term)
          )
        );
      }

      // Filter
      if (this._state.source) {
        data = data.filter((x: any) => x.source === this._state.source);
      }

      if (this._state.tier) {
        data = data.filter((x: any) => x.tier === this._state.tier);
      }

       if (this._state.campaign) {
        data = data.filter((x: any) => x.campaign === this._state.campaign);
      }

      // Sort
      if (this._state.sortColumn && this._state.sortDirection) {
        data = data.sort((a: any, b: any) => {
          const res = compare(a[this._state.sortColumn], b[this._state.sortColumn]);
          return this._state.sortDirection === 'asc' ? res : -res;
        });
      }

      const total = data.length;

      // Pagination
      const start = (this._state.page - 1) * this._state.pageSize;
      data = data.slice(start, start + this._state.pageSize);

      this._loading$.next(false);
      observer.next({ items: data, total });
      observer.complete();
    });
  }
}