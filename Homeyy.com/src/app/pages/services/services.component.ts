import { Component } from '@angular/core';

@Component({
  selector: 'app-services',
  templateUrl: './services.component.html',
  styleUrls: ['./services.component.scss'],
})
export class ServicesComponent {

  services = [
    { title: 'AC & Heat', path: 'hvac', icon: '../../../assets/images/icons/ac.png' },
    { title: 'Bathroom', path: 'bathroom', icon: '../../../assets/images/icons/bathroom.png' },
    { title: 'Kitchen', path: 'kitchen', icon: '../../../assets/images/icons/kitchen.png' },
    { title: 'Plumbing', path: 'plumbing', icon: '../../../assets/images/icons/plumbing.png' },
    { title: 'Window', path: 'window', icon: '../../../assets/images/icons/windows.png' },
    { title: 'Door', path: 'door', icon: '../../../assets/images/icons/door.png' },
    { title: 'Flooring', path: 'flooring', icon: '../../../assets/images/icons/flooring.png' },
    { title: 'Gutter', path: 'gutter', icon: '../../../assets/images/icons/gutter.png' },
    { title: 'Fencing', path: 'fencing', icon: '../../../assets/images/icons/fencing.png' },
    { title: 'Solar', path: 'solar', icon: '../../../assets/images/icons/solar.png' },
    { title: 'Roofing', path: 'roofing', icon: '../../../assets/images/icons/roofing.png' },
    { title: 'Home Security', path: 'home-security', icon: '../../../assets/images/icons/security.png' },
    { title: 'Siding', path: 'siding', icon: '../../../assets/images/icons/siding.png' }
  ];

  redirect_to(data: { path: string }) {
    window.open(`/services/${data.path}`, '_blank');
  }

}
