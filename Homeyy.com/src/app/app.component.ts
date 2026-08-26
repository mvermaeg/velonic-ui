import { Component } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { MetaPixelService } from './services/meta-pixel.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {

  title = 'Homeyy';

  constructor(router: Router, metaPixel: MetaPixelService) {
    router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => metaPixel.pageView());
  }

}
