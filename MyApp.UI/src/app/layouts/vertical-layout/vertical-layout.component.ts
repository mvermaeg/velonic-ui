import { CommonModule } from '@angular/common'
import {
  Component,
  HostListener,
  Renderer2,
  inject,
  type OnInit,
} from '@angular/core'
import { RouterModule } from '@angular/router'
import { FooterComponent } from '@layouts/footer/footer.component'
import { SidebarComponent } from '@layouts/sidebar/sidebar.component'
import { TopbarComponent } from '@layouts/topbar/topbar.component'
import { NgbOffcanvas } from '@ng-bootstrap/ng-bootstrap'
import { Store } from '@ngrx/store'
import { changemenusize } from '@store/layout/layout-action'
import { getLayoutMenuSize } from '@store/layout/layout-selector'

@Component({
  selector: 'app-vertical-layout',
  standalone: true,
  imports: [
    SidebarComponent,
    FooterComponent,
    TopbarComponent,
    RouterModule,
    CommonModule,
  ],
  templateUrl: './vertical-layout.component.html',
  styles: ``,
})
export class VerticalLayoutComponent implements OnInit {
  private offcanvasService = inject(NgbOffcanvas)
  private store = inject(Store)
  private renderer = inject(Renderer2)
  size = 'default'
  ngOnInit(): void {
    if (document.documentElement.clientWidth <= 1140) {
      this.onResize()
    }
  }

  @HostListener('window:resize', ['$event'])
  onResize() {
    if (document.documentElement.clientWidth <= 767) {
      this.size = 'full'
      this.store.dispatch(changemenusize({ menusize: 'full' }))
    } else if (document.documentElement.clientWidth <= 1024) {
      this.store.dispatch(changemenusize({ menusize: 'condensed' }))
      document.getElementById('custom-backdrop')?.classList.add('d-none')
    } else if (document.documentElement.clientWidth >= 1024) {
      this.store.dispatch(changemenusize({ menusize: 'default' }))
      document.getElementById('custom-backdrop')?.classList.add('d-none')
      document.documentElement.classList.remove('sidebar-enable')
    }
    this.store.select(getLayoutMenuSize).subscribe((size: string) => {
      this.renderer.setAttribute(
        document.documentElement,
        'data-sidenav-size',
        size
      )
    })
  }

  onToggleMobileMenu() {
    const menuSize = document.documentElement.getAttribute('data-sidenav-size')!
    if (document.documentElement.clientWidth >= 767 && menuSize != 'full') {
      if (menuSize == 'condensed') {
        this.store.dispatch(changemenusize({ menusize: 'default' }))
      } else {
        this.store.dispatch(changemenusize({ menusize: 'condensed' }))
      }
    } else {
      document.documentElement.classList.toggle('sidebar-enable')
      document.getElementById('custom-backdrop')?.classList.remove('d-none')
    }

    this.store.select(getLayoutMenuSize).subscribe((size: string) => {
      this.renderer.setAttribute(
        document.documentElement,
        'data-sidenav-size',
        size
      )
    })
  }
}
