import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { isLanderRoute } from 'src/app/shared/lander-routes';

@Component({
  selector: 'com-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss']
})
export class FooterComponent implements OnInit, OnDestroy {

  YEAR = new Date().getFullYear();

  // Landing pages show the copyright strip only
  showFooterBottomOnly = false;

  private routeSub?: Subscription;

  constructor(public router: Router) {}

  ngOnInit(): void {
    this.routeSub = isLanderRoute(this.router).subscribe(isLander => this.showFooterBottomOnly = isLander);
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
  }

}
