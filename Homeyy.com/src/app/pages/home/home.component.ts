import { AfterViewInit, Component } from '@angular/core';
import { Router } from '@angular/router';

import gsap from 'gsap';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements AfterViewInit {

  features = [
    {
      title: 'Customized Solutions',
      desc: 'We tailor our air conditioning services to fit your specific needs and preferences, ensuring optimal performance and comfort in your home for years to come.',
      icon: '<i class="fa-solid fa-pen-ruler"></i>'
    },
    {
      title: 'Timely Response',
      desc: 'Our team is committed to prompt service, quick responses to your air conditioning issues, minimizing downtime, and restoring your comfort efficiently.',
      icon: '<i class="fa-solid fa-stopwatch"></i>'
    },
    {
      title: 'Quality Products',
      desc: 'We use only high-quality, reliable products and equipment to ensure that your air conditioning system operates efficiently and lasts longer.',
      icon: '<i class="fa-solid fa-award"></i>'
    },
    {
      title: 'Transparent Pricing',
      desc: 'We provide straightforward, upfront pricing with no hidden fees, so you can make informed decisions about your air conditioning services.',
      icon: '<i class="fa-solid fa-hand-holding-dollar"></i>'
    }
  ];

  // Drives both the service grid and the marquee strip
  services = [
    { title: 'AC & Heat', path: 'hvac-contractors', icon: '../../../assets/images/icons/ac.png' },
    { title: 'Bathroom', path: 'bathroom-remodelling-contractors', icon: '../../../assets/images/icons/bathroom.png' },
    { title: 'Kitchen', path: 'kitchen-modeling-contractors', icon: '../../../assets/images/icons/kitchen.png' },
    { title: 'Plumbing', path: 'plumbing-contractors', icon: '../../../assets/images/icons/plumbing.png' },
    { title: 'Window', path: 'window-installation-contractors', icon: '../../../assets/images/icons/windows.png' },
    { title: 'Door', path: 'door-installation-contractors', icon: '../../../assets/images/icons/door.png' },
    { title: 'Flooring', path: 'flooring-contractors', icon: '../../../assets/images/icons/flooring.png' },
    { title: 'Gutter', path: 'gutter-installation-contractors', icon: '../../../assets/images/icons/gutter.png' },
    { title: 'Fencing', path: 'fencing-installation-contractors', icon: '../../../assets/images/icons/fencing.png' },
    { title: 'Solar', path: 'solar-installation-contractors', icon: '../../../assets/images/icons/solar.png' },
    { title: 'Roofing', path: 'roofing-installation-contractors', icon: '../../../assets/images/icons/roofing.png' },
    { title: 'Home Security', path: 'home-security-contractors', icon: '../../../assets/images/icons/security.png' },
    { title: 'Siding', path: 'siding-installation-contractors', icon: '../../../assets/images/icons/siding.png' }
  ];

  constructor(public router: Router) {}

  ngAfterViewInit() {
    this.text_animation();
  }

  // *\---------------- Text animation -----------------\*

  text_animation(): void {
    gsap.timeline()
      .from('.tagline', { y: 50, opacity: 0, duration: 0.8, ease: 'power3.out' })
      .from('#hero h1', { y: 60, opacity: 0, duration: 1, ease: 'power3.out' }, '-=0.4')
      .from('#hero h5', { y: 60, opacity: 0, duration: 1, ease: 'power3.out' }, '-=0.5')
      .from('.cta-group', { y: 40, opacity: 0, duration: 0.8, ease: 'power3.out' }, '-=0.5');
  }

  redirect_to(data: { path: string }) {
    window.open(`/${data.path}`, '_blank');
  }

  redirect() {
    this.router.navigate(['/']);

    setTimeout(() => {
      this.router.navigate(['/'], { fragment: 'contact' });
    }, 10);
  }

  redirect_two(path: string) {
    this.router.navigateByUrl(path);
  }

}
