import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsService } from 'src/app/services/forms.service';

@Component({
  selector: 'app-thank-you',
  templateUrl: './thank-you.component.html',
  styleUrls: ['./thank-you.component.scss']
})
export class ThankYouComponent implements OnInit {

  path_name = '';
  imageUrl = '';

  constructor(
    private activatedRoute: ActivatedRoute,
    private formsService: FormsService
  ) { }

  ngOnInit(): void {
    this.path_name = this.activatedRoute.snapshot.paramMap.get('qoute-name') ?? '';
    this.imageUrl = this.formsService.qoutes.find(x => x.path === this.path_name)?.icon ?? '';
  }

}
