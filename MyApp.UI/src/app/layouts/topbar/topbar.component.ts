import {
  CUSTOM_ELEMENTS_SCHEMA,
  Component,
  EventEmitter,
  Output,
  Renderer2,
  inject,
} from '@angular/core'
import { RouterModule } from '@angular/router'
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap'
import { Store } from '@ngrx/store'
import { changetheme } from '@store/layout/layout-action'
import { getLayoutColor } from '@store/layout/layout-selector'
import { SimplebarAngularModule } from 'simplebar-angular'
import { Notifications, profileMenus } from './data'
import { LogoBoxComponent } from '@component/logo-box/logo-box.component'

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [
    NgbDropdownModule,
    SimplebarAngularModule,
    RouterModule,
    LogoBoxComponent,
  ],
  templateUrl: './topbar.component.html',
  styles: ``,
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class TopbarComponent {
  notificationList = Notifications
  profileList = profileMenus

  @Output() rightSidebarButton = new EventEmitter()
  @Output() mobileMenuButtonClicked = new EventEmitter()
  store = inject(Store)

  languages = [
    { flag: 'assets/images/flags/us.jpg', name: 'English' },
    { flag: 'assets/images/flags/germany.jpg', name: 'German' },
    { flag: 'assets/images/flags/italy.jpg', name: 'Italian' },
    { flag: 'assets/images/flags/spain.jpg', name: 'Spanish' },
    { flag: 'assets/images/flags/russia.jpg', name: 'Russian' },
  ]

  settingButton() {
    this.rightSidebarButton.emit()
  }

  changeTheme() {
    const color = document.documentElement.getAttribute('data-bs-theme')
    if (color == 'light') {
      this.store.dispatch(changetheme({ color: 'dark' }))
    } else {
      this.store.dispatch(changetheme({ color: 'light' }))
    }
    this.store.select(getLayoutColor).subscribe((color) => {
      document.documentElement.setAttribute('data-bs-theme', color)
    })
  }

  toggleMobileMenu() {
    this.mobileMenuButtonClicked.emit()
  }

  timeSince(date: Date) {
    if (typeof date !== 'object') {
      date = new Date(date)
    }

    var seconds = Math.floor((new Date().valueOf() - date.valueOf()) / 1000)
    var intervalType: string

    var interval = Math.floor(seconds / 31536000)
    if (interval >= 1) {
      intervalType = 'year'
    } else {
      interval = Math.floor(seconds / 2592000)
      if (interval >= 1) {
        intervalType = 'month'
      } else {
        interval = Math.floor(seconds / 86400)
        if (interval >= 1) {
          intervalType = 'day'
        } else {
          interval = Math.floor(seconds / 3600)
          if (interval >= 1) {
            intervalType = 'hour'
          } else {
            interval = Math.floor(seconds / 60)
            if (interval >= 1) {
              intervalType = 'minute'
            } else {
              interval = seconds
              intervalType = 'second'
            }
          }
        }
      }
    }
    if (interval > 1 || interval === 0) {
      intervalType += 's'
    }
    return interval + ' ' + intervalType + ' ago'
  }
}
