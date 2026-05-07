import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BiddingService } from './bidding.service';

@Component({
  selector: 'app-bidding',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bidding.component.html'
})
export class BiddingComponent implements OnInit {

  records: any[] = [];
  total = 0;

  filters: any = {
    clientId: null,
    search: '',
    fromDate: null,
    toDate: null,
    excludeAfterSale: true,
    page: 1,
    pageSize: 20
  };

  constructor(private service: BiddingService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.service.getResults(this.filters).subscribe(res => {
      this.records = res.data;
      this.total = res.total;
    });
  }


  deliver(id: number) {
  if (!confirm('Are you sure you want to sell/deliver this lead?')) {
    return
  }

  this.service.deliverResult(id).subscribe({
    next: () => {
      alert('Lead delivered successfully.')
      this.load()
    },
    error: (err) => {
      alert(err?.error?.message || 'Unable to deliver lead.')
    },
  })
}
}