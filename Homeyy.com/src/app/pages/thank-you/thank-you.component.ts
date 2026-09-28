import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subject, timer } from 'rxjs';
import { exhaustMap, finalize, takeUntil, takeWhile, timeout } from 'rxjs/operators';
import { ActivatedRoute } from '@angular/router';
import { FormsService } from 'src/app/services/forms.service';
import {
  ThumbtackBusiness,
  WebsiteLeadService
} from 'src/app/services/website-lead.service';

@Component({
  selector: 'app-thank-you',
  templateUrl: './thank-you.component.html',
  styleUrls: ['./thank-you.component.scss']
})
export class ThankYouComponent implements OnInit, OnDestroy {
  private readonly destroyed = new Subject<void>();

  path_name = '';
  imageUrl = '';
  thumbtackLoading = false;
  thumbtackMessage = '';
  thumbtackBusinesses: ThumbtackBusiness[] = [];

  constructor(
    private activatedRoute: ActivatedRoute,
    private formsService: FormsService,
    private websiteLeadService: WebsiteLeadService
  ) { }

  ngOnInit(): void {
    this.path_name = this.activatedRoute.snapshot.paramMap.get('qoute-name') ?? '';
    this.imageUrl = this.formsService.qoutes.find(x => x.path === this.path_name)?.icon ?? '';
    this.loadThumbtackBusinesses();
  }

  ngOnDestroy(): void {
    this.destroyed.next();
    this.destroyed.complete();
  }

  private loadThumbtackBusinesses(): void {
    const pending = this.websiteLeadService.getPendingThumbtackLead();

    if (
      !pending ||
      pending.serviceCode !== String(this.path_name || '').toLowerCase()
    ) {
      return;
    }

    this.thumbtackLoading = true;

    // Observe the API's decision; never start another lead or select a provider here.
    timer(0, 3000).pipe(
      exhaustMap(() => this.websiteLeadService
        .getThumbtackBusinesses(pending.leadId, pending.leadUuid)
        .pipe(timeout(10000))),
      takeWhile(response => response.routingPending === true, true),
      takeUntil(timer(120000)),
      takeUntil(this.destroyed),
      finalize(() => {
        this.thumbtackLoading = false;
        if (!this.thumbtackBusinesses.length && !this.thumbtackMessage) {
          this.thumbtackMessage = 'Your request was submitted successfully. Matching is still in progress.';
        }
      })
    )
      .subscribe({
        next: response => {
          if (response.success === false) {
            this.thumbtackMessage = 'Your request was submitted successfully. Thumbtack recommendations are temporarily unavailable.';
            return;
          }
          if (response.routingPending) return;
          this.thumbtackLoading = false;
          this.thumbtackBusinesses = response.available
            ? (response.businesses || []).filter(x => !!x.requestFlowUrl)
            : [];

          if (!response.available || !this.thumbtackBusinesses.length) {
            this.thumbtackMessage =
              'No additional Thumbtack professionals are available for this request.';
          }
        },
        error: () => {
          this.thumbtackLoading = false;
          this.thumbtackMessage =
            'Your request was submitted successfully. Additional Thumbtack professionals are temporarily unavailable.';
        }
      });
  }

}



// import { Component, OnInit } from '@angular/core';
// import { ActivatedRoute } from '@angular/router';
// import { FormsService } from 'src/app/services/forms.service';

// @Component({
//   selector: 'app-thank-you',
//   templateUrl: './thank-you.component.html',
//   styleUrls: ['./thank-you.component.scss']
// })
// export class ThankYouComponent implements OnInit {

//   path_name = '';
//   imageUrl = '';

//   constructor(
//     private activatedRoute: ActivatedRoute,
//     private formsService: FormsService
//   ) { }

//   ngOnInit(): void {
//     this.path_name = this.activatedRoute.snapshot.paramMap.get('qoute-name') ?? '';
//     this.imageUrl = this.formsService.qoutes.find(x => x.path === this.path_name)?.icon ?? '';
//   }

// }
