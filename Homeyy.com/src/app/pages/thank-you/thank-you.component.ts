import { Component, OnInit } from '@angular/core';
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
export class ThankYouComponent implements OnInit {

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

  private loadThumbtackBusinesses(): void {
    const pending = this.websiteLeadService.getPendingThumbtackLead();

    if (
      !pending ||
      pending.serviceCode !== String(this.path_name || '').toLowerCase()
    ) {
      return;
    }

    this.thumbtackLoading = true;

    this.websiteLeadService
      .getThumbtackBusinesses(pending.leadId, pending.leadUuid)
      .subscribe({
        next: response => {
          this.thumbtackLoading = false;
          this.thumbtackBusinesses = response.available
            ? (response.businesses || []).filter(x => !!x.requestFlowUrl)
            : [];

          if (!response.available) {
            this.thumbtackMessage =
              response.message ||
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
