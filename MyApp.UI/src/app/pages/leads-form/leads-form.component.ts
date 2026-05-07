
import { Component, Input, TemplateRef, ViewChild } from '@angular/core';
import { NgbModal, NgbModalOptions, NgbNavModule, NgbAccordionModule, NgbCollapseModule } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common'
import { FormsModule } from '@angular/forms'

@Component({
  selector: 'app-leads-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbNavModule,
    NgbAccordionModule,
    NgbCollapseModule
  ],
  templateUrl: './leads-form.component.html',
  styleUrl: './leads-form.component.scss',
  exportAs: 'appRightModal'
})

export class LeadsFormComponent {

  @Input() title: string = '';

  @ViewChild('modalTemplate', { static: true })
  modalTemplate!: TemplateRef<any>;


  General = [
    { label: 'Lead ID', value: '369-179-397' },
    { label: 'UUID', value: '92777bab-f519-49db-b878-4069401044e4' },
    { label: 'Parent Lead ID', value: 'N/A' },
    { label: 'Campaign', value: 'HVAC' },
    { label: 'First Name', value: 'Sarah' },
    { label: 'Last Name', value: 'Johnson' },
    { label: 'Email', value: 'sarah.jemail.com' },
    { label: 'Phone', value: '(555) 123-4567' },
    { label: 'Status', value: '1 - Done' },
    { label: 'Test Lead', value: 'No' },
    { label: 'Form Completed', value: 'No' },
    { label: 'IP Address', value: '2600:8803:5e52:4b00:bbb6:90ae:e16f:8d07' },
    { label: 'Revenue', value: '$0.00' },
    { label: 'Sales', value: '0' },
    { label: 'Created At', value: '16/04/2026 04:39' },
    { label: 'Updated At', value: '16/04/2026 04:40' }
  ];

  siding = [
    { label: 'Best Call Time', value: 'N/A' },
    { label: 'Homeowner', value: 'yes' },
    { label: 'Project Type', value: 'replace' },
    { label: 'Project Type', value: 'replace' },
    { label: 'Purchase Time', value: 'immediately' },
    { label: 'Siding Type', value: 'vinyl' },
    { label: 'TCPA Text', value: 'N/A' },
    { label: 'TCPA Text', value: 'N/A' }
  ]

  landing = [
    { label: 'Form Name', value: 'siding-simple' },
    { label: 'Form Step', value: '3' },
    { label: 'Landing Page URL', value: 'https://home-improvements.work/simple/siding?utm_source=RT%201%20APRIL%20%E2%80%93%20Copy&utm_medium=AD%20SET%201&utm_campaign=20%20%E2%80%93%20Copy%2010&fbclid=IwZXh0bgNhZW0BMABhZGlkAastJQeeicFzcnRjBmFwcF9pZAwzNTA2ODU1MzE3MjgAAR7dl-BuSkiXWx3rNVhCqb7YMRFKCRuf3cuU8yjP_vfQMB-CMXb5ifKG57g_aem_d_BHGRIz8SaOmHJ6tP0BBA&utm_id=120239450979160449&utm_content=120239450979180449&utm_term=120239450979190449&aid=1&clid=b07ed910-c522-487c-bc93-03e40ad1ac60&k=073d42fd-5cbd-48eb-a858-9e44cb94e9fa&t=f6&vr=2' },
    { label: 'Landing Page URL (Original)', value: 'N/A' },
    { label: 'Trusted Form Landing Page', value: 'N/A' }
  ]

  affiliate = [
    { label: 'Affiliate', value: 'MB-1 - Facebook - Ads' },
    { label: 'Sub ID 1', value: '120239450979190449' },
    { label: 'Sub ID 2', value: 'N/A' },
    { label: 'Sub ID 3', value: 'N/A' },
  ]
  certification = [
    { label: 'Trusted Form Token', value: 'https://cert.trustedform.com/ae94f0798249e519f6b72477c6b91f1c33b09c4a' },
    { label: 'Universal Lead ID', value: '58AAA698-21ED-4254-95A6-C9A4A100917E' },
  ]

  device = [
    { label: 'Device Type', value: 'N/A' },
    { label: 'Device Brand', value: 'N/A' },
    { label: 'Device OS Name', value: 'N/A' },
    { label: 'Device OS Version', value: 'N/A' },
    { label: 'Device Model', value: 'N/A' },
    { label: 'Browser Name', value: 'N/A' },
    { label: 'Browser Version', value: 'N/A' },
    {
      label: 'User Agent',
      value: 'Mozilla/5.0 (Linux; Android 16; SM-S928U Build/BP2A.250605.031.A3; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/147.0.7727.86 Mobile Safari/537.36 [FB_IAB/FB4A;FBAV/556.10.63.64;IABMV/1;]'
    }
  ]

  ipinfo = [
    { label: 'IP Address', value: '2600:8803:5e52:4b00:bbb6:90ae:e16f:8d07' },
    { label: 'IP Geo Code', value: 'US' },
    { label: 'IP Region Name', value: 'KS - Kansas' },
    { label: 'IP City', value: 'Kansas City' },
    { label: 'IP Postcode', value: 'N/A' },
    { label: 'IP Latitude', value: '0' },
    { label: 'IP Longitude', value: '0' },
    { label: 'IP Timezone', value: 'UTC0:00' },
    { label: 'ISP', value: 'Cox Communications' }
  ]

  location = [
    { label: 'Geo', value: 'US' },
    { label: 'State', value: 'KS' },
    { label: 'Postcode', value: '66604' },
    { label: 'Address', value: '4412 Southwest 17th Terrace' },
    { label: 'City', value: 'Topeka' }
  ]

  abtest = [
    { label: 'A/B Test Name', value: 'N/A' },
    { label: 'A/B Test Version', value: 'N/A' },
  ]

  utmparameters = [
    { label: 'UTM Campaign', value: '2 – Copy 10' },
    { label: 'UTM Source', value: 'RT 1 APRIL – Copy' },
    { label: 'UTM Medium', value: 'AD SET 1' },
    { label: 'UTM Content', value: '120239450979180449' },
    { label: 'UTM Term', value: '120239450979190449' },
    { label: 'FB Preview URL', value: 'N/A' },
  ]



  leadDetails = [
    { label: 'Fraud Score', value: 0 },
    { label: 'Recent Abuse', value: 'No' },
    { label: 'Risky', value: 'No' },
    { label: 'Valid', value: 'No' },
    { label: 'Active', value: 'No' },
    { label: 'VOIP', value: 'No' },
    { label: 'Prepaid', value: 'No' },
    { label: 'Name', value: 'N/A' },
    { label: 'Line Type', value: 'N/A' }
  ];


  sections = [
    {
      title: 'General Information',
      data: this.General
    },
    {
      title: 'Siding Fileds',
      data: this.siding
    },
    {
      title: 'Landing Page',
      data: this.landing
    },
    {
      title: 'Affiliate',
      data: this.affiliate
    },
    {
      title: 'Certification',
      data: this.certification
    },
    {
      title: 'Device & Browser',
      data: this.device
    },
    {
      title: 'IP Info',
      data: this.ipinfo
    },
    {
      title: 'Location',
      data: this.location
    },
    {
      title: 'A/B Test',
      data: this.abtest
    },
    {
      title: 'UTM Parameters',
      data: this.utmparameters
    }
  ];


  constructor(
    private modalService: NgbModal
  ) {

  }

  open(options?: NgbModalOptions) {
    this.modalService.open(this.modalTemplate, {
      size: 'xl',
      modalDialogClass: 'modal-right',
      backdrop: 'static',
      ...options
    });
  }

  activeIndex: number = 0;
  toggle(index: number) {
    this.activeIndex = this.activeIndex === index ? -1 : index;
  }


}
