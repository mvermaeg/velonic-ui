import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { isLanderRoute } from 'src/app/shared/lander-routes';

@Component({
  selector: 'com-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, OnDestroy {

  isCollapsed = false;
  showLogoOnly = false;

  navItems = [
    { label: 'Home', link: '/' },
    { label: 'About us', link: '/about-us' },
    { label: 'Services', link: '/services' },
    { label: 'Blog', link: '/blogs' },
    { label: 'Contact Us', link: '/contact-us' }
  ];

  private routeSub?: Subscription;

  constructor(public router: Router) {}

  ngOnInit(): void {
    this.routeSub = isLanderRoute(this.router).subscribe(isLander => this.showLogoOnly = isLander);
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
  }

  toggleNavbar() {
    this.isCollapsed = !this.isCollapsed;
  }

  // The nav items navigate through routerLink so they stay real, crawlable links
  closeNavbar() {
    this.isCollapsed = false;
  }

  redirect() {
    this.router.navigate(['/']);

    setTimeout(() => {
      this.router.navigate(['/'], { fragment: 'contact' });
    }, 10);

    this.isCollapsed = false;
  }

}
