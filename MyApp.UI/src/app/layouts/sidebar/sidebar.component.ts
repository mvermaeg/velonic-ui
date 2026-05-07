import { CommonModule } from '@angular/common'
import {
  AfterViewInit,
  CUSTOM_ELEMENTS_SCHEMA,
  Component,
  inject,
  type OnInit,
} from '@angular/core'
import { NavigationEnd, Router, RouterModule } from '@angular/router'
import { NgbCollapse, NgbCollapseModule } from '@ng-bootstrap/ng-bootstrap'
import { Store } from '@ngrx/store'
import { SimplebarAngularModule } from 'simplebar-angular'
import { MENU, type MenuItem } from '@common/menu-meta'
import { basePath } from '@common/constants'
import { findAllParent, findMenuItem } from '@core/helpers/utils'
import { changemenusize } from '@store/layout/layout-action'
import { getLayoutMenuSize } from '@store/layout/layout-selector'
import { LogoBoxComponent } from '@component/logo-box/logo-box.component'

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    LogoBoxComponent,
    SimplebarAngularModule,
    RouterModule,
    CommonModule,
    NgbCollapseModule,
  ],
  templateUrl: './sidebar.component.html',
  styles: ``,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class SidebarComponent implements OnInit, AfterViewInit {
  menuItems: MenuItem[] = []
  activeMenuItems: string[] | any[] = []
  router = inject(Router)
  store = inject(Store)
  trimmedURL = this.router.url?.replaceAll(
    basePath !== '' ? basePath + '/' : '',
    '/'
  )

  constructor() {
    this.router.events.forEach((event) => {
      if (event instanceof NavigationEnd) {
        this.trimmedURL = this.router.url?.replaceAll(
          basePath !== '' ? basePath + '/' : '',
          '/'
        )
        this._activateMenu()
      }
    })
  }

  ngOnInit(): void {
    this.initMenu()
  }

  ngAfterViewInit() {
    setTimeout(() => {
      this._activateMenu()
    })
  }

  scrollTo(element: Element, to: number, duration: number): void {
    const start = element.scrollTop
    const change = to - start
    const increment = 20
    let currentTime = 0

    const animateScroll = () => {
      currentTime += increment
      const val = this.easeInOutQuad(currentTime, start, change, duration)
      element.scrollTop = val
      if (currentTime < duration) {
        setTimeout(animateScroll, increment)
      }
    }
    animateScroll()
  }

  easeInOutQuad(t: number, b: number, c: number, d: number): number {
    t /= d / 2
    if (t < 1) return (c / 2) * t * t + b
    t--
    return (-c / 2) * (t * (t - 2) - 1) + b
  }

  hasSubmenu(menu: MenuItem): boolean {
    return menu.subMenu ? true : false
  }

  initMenu(): void {
    this.menuItems = MENU
  }

  _activateMenu(): void {
    const div = document.getElementById('main-side-menu')

    let matchingMenuItem = null
    if (div) {
      let items: any = div.getElementsByClassName('side-nav-link-ref')
      for (let i = 0; i < items.length; ++i) {
        if (window.location.pathname === items[i].pathname) {
          matchingMenuItem = items[i]
          break
        }
      }
      if (matchingMenuItem) {
        const mid = matchingMenuItem.getAttribute('aria-controls')
        const activeMt = findMenuItem(this.menuItems, mid)

        if (activeMt) {
          const matchingObjs = [
            activeMt['key'],
            ...findAllParent(this.menuItems, activeMt),
          ]

          this.activeMenuItems = matchingObjs
          this.menuItems.forEach((menu: MenuItem) => {
            menu.collapsed = !matchingObjs.includes(menu.key!)
          })
        }
      }
    }

    setTimeout(() => {
      var activatedItem = matchingMenuItem!
      if (activatedItem != null) {
        var simplebarContent = document.querySelector(
          '#leftside-menu-container .simplebar-content-wrapper'
        )
        var offset = activatedItem!.offsetTop - 300
        if (simplebarContent && offset > 100) {
          this.scrollTo(simplebarContent, offset, 600)
        }
      }
    }, 200)
  }

  toggleMenuItems(menuItem: MenuItem, collapse: NgbCollapse): void {
    collapse.toggle()
    let openMenuItems: any[]
    if (!menuItem.collapsed) {
      openMenuItems = [
        menuItem['key'],
        ...findAllParent(this.menuItems, menuItem),
      ]
      this.menuItems.forEach((menu: MenuItem) => {
        if (!openMenuItems.includes(menu.key!)) {
          menu.collapsed = true
        }
      })
    }
  }

  changeSidebarSize() {
    let menusize = document.documentElement.getAttribute('data-sidenav-size')
    if (menusize == 'sm-hover') {
      menusize = 'sm-hover-active'
    } else {
      menusize = 'sm-hover'
    }
    this.store.dispatch(changemenusize({ menusize }))
    this.store.select(getLayoutMenuSize).subscribe((menusize) => {
      document.documentElement.setAttribute('data-sidenav-size', menusize)
    })
  }

  // Hide Backdrop
  hideBackdrop() {
    document.getElementById('custom-backdrop')?.classList.add('d-none')
    document.documentElement.classList.toggle('sidebar-enable')
  }

//   openParentMenu(menuItem: MenuItem, collapse: NgbCollapse) {
//   collapse.toggle()

//   // Open first submenu page automatically
//   if (menuItem.subMenu && menuItem.subMenu.length > 0) {
//     const firstChild = menuItem.subMenu[0]

//     if (firstChild.link) {
//       this.router.navigate([firstChild.link])
//     }
//   }

//   let openMenuItems: any[]

//   if (!menuItem.collapsed) {
//     openMenuItems = [
//       menuItem['key'],
//       ...findAllParent(this.menuItems, menuItem),
//     ]

//     this.menuItems.forEach((menu: MenuItem) => {
//       if (!openMenuItems.includes(menu.key!)) {
//         menu.collapsed = true
//       }
//     })
//   }
// }


}
