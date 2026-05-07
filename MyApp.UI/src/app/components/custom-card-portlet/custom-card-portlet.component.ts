import { Component, Input, type OnInit } from '@angular/core'
import { NgbCollapseModule } from '@ng-bootstrap/ng-bootstrap'

@Component({
  selector: 'custom-card-portlet',
  standalone: true,
  imports: [NgbCollapseModule],
  templateUrl: './custom-card-portlet.component.html',
  styles: ``,
})
export class CustomCardPortletComponent implements OnInit {
  @Input() cardTitle: string = ''
  @Input() cardTitleClass?: string
  @Input() headerClass?: string
  isCollapsed: boolean = false
  isClosed: boolean = false
  refreshRequsted: boolean = false

  constructor() {}

  ngOnInit(): void {}

  /**
   * close portlet card
   */
  removeCard(): void {
    this.isClosed = true
  }

  /**
   * refresh card content
   */
  reloadContent(): void {
    this.refreshRequsted = true
    setTimeout(() => {
      this.refreshRequsted = false
    }, 1000)
  }
}
