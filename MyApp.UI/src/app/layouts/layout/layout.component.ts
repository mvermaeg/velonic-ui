import { Component, Renderer2, inject, type OnInit } from '@angular/core'
// import { HorizontalLayoutComponent } from '@layouts/horizontal-layout/horizontal-layout.component'
import { VerticalLayoutComponent } from '@layouts/vertical-layout/vertical-layout.component'
import { Store } from '@ngrx/store'

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [VerticalLayoutComponent],
  templateUrl: './layout.component.html',
  styles: ``,
})
export class LayoutComponent implements OnInit {
  layoutType: string = ''

  private store = inject(Store)
  private render = inject(Renderer2)

  ngOnInit(): void {
    this.store.select('layout').subscribe((data) => {
      this.layoutType = data.LAYOUT
      this.render.setAttribute(
        document.documentElement,
        'data-bs-theme',
        data.LAYOUT_THEME
      )
      this.render.setAttribute(
        document.documentElement,
        'data-layout-mode',
        data.LAYOUT_MODE
      )
      this.render.setAttribute(
        document.documentElement,
        'data-menu-color',
        data.LAYOUT_MENU_COLOR
      )
      this.render.setAttribute(
        document.documentElement,
        'data-topbar-color',
        data.LAYOUT_TOPBAR_COLOR
      )
      this.render.setAttribute(
        document.documentElement,
        'data-layout-position',
        data.LAYOUT_POSITION
      )
      this.render.setAttribute(
        document.documentElement,
        'data-sidenav-size',
        data.LAYOUT_MENU_SIZE
      )
    })
    this.render.addClass(document.documentElement, 'menuitem-active')
  }

  /**
   * Check if the vertical layout is requested
   */
  isVerticalLayoutRequested() {
    return this.layoutType === 'vertical'
  }
}
