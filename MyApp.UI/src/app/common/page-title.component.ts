import { Component, input, Input } from '@angular/core'

@Component({
  selector: 'app-page-title',
  standalone: true,
  imports: [],
  template: `
    <div class="row">
      <div class="col-12">
        <div class="page-title-box">
          <div class="page-title-right">
            <ol class="breadcrumb m-0">
              <li class="breadcrumb-item">
                <a href="javascript: void(0);">Velonic</a>
              </li>
              <li class="breadcrumb-item">
                <a href="javascript: void(0);">{{ subtitle }}</a>
              </li>
              <li class="breadcrumb-item active">{{ title }}</li>
              
            </ol>
          </div>
          <h4 class="page-title">{{ title }}</h4>
          <p style="margin-top: -1rem;">{{header}}</p>
        </div>
      </div>
    </div>
  `,
})
export class PageTitleComponent {
  @Input() title: string = ''
  @Input() subtitle: string = ''
  @Input() header: string = ''
}
