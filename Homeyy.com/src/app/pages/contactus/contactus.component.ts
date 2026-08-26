import { Component } from '@angular/core';

@Component({
  selector: 'app-contactus',
  templateUrl: './contactus.component.html',
  styleUrls: ['./contactus.component.scss'],
})
export class ContactusComponent {

  contacts = [
    {
      title: 'Email Support',
      value: 'info@bcgnj.com',
      icon: 'fa-solid fa-envelope'
    },
    {
      title: 'Phone Number',
      value: '+1 (712) 724-8001',
      icon: 'fa-solid fa-phone'
    },
    {
      title: 'Office Address',
      value: `15 Corporate Pl S Suite# 210,
       Piscataway, NJ,
       United States`,
      icon: 'fa-solid fa-location-dot'
    }
  ];

}
