import { Component, inject, TemplateRef } from '@angular/core'
import { credits } from '@common/constants'
import { CustomCardPortletComponent } from '@component/custom-card-portlet/custom-card-portlet.component'
import { Campagin, Clients, Contracts, Leads } from '@views/dashboard/data'
import { NgbModal, NgbModalModule, NgbModalConfig, type NgbModalOptions, } from '@ng-bootstrap/ng-bootstrap'
import { LeadsFormComponent } from '@/app/pages/leads-form/leads-form.component'
@Component({
  selector: 'dashboard-projects',
  standalone: true,
  imports: [
    CustomCardPortletComponent, 
    NgbModalModule,
    LeadsFormComponent
  ],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss'
})

export class ProjectsComponent {
  credits = credits
  Leads = Leads
  Campagin = Campagin
  Clients = Clients
  Contracts = Contracts
  private modalService = inject(NgbModal)

  openModal(content: TemplateRef<HTMLElement>,
    options: NgbModalOptions) {
    this.modalService.open(content, options)
  }
}