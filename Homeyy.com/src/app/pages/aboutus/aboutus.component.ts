import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-aboutus',
  templateUrl: './aboutus.component.html',
  styleUrls: ['./aboutus.component.scss'],
})
export class AboutusComponent implements OnInit {

  constructor(public router:Router) {

  }

  ngOnInit(): void {

  }

   redirect_two(path: any) {
    this.router.navigateByUrl(`${path}`);
  }

}









