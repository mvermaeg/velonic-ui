import { AfterViewInit, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import gsap from 'gsap';
import ScrollTrigger from 'gsap/ScrollTrigger';
import { DataService } from 'src/app/services/data.service';

gsap.registerPlugin(ScrollTrigger);

// Public landing-page slug -> content key used by DataService and the quote form
const SERVICE_SLUGS: { [slug: string]: string } = {
  'hvac-contractors': 'hvac',
  'bathroom-remodelling-contractors': 'bathroom',
  'kitchen-modeling-contractors': 'kitchen',
  'plumbing-contractors': 'plumbing',
  'window-installation-contractors': 'window',
  'door-installation-contractors': 'door',
  'flooring-contractors': 'flooring',
  'gutter-installation-contractors': 'gutter',
  'fencing-installation-contractors': 'fencing',
  'solar-installation-contractors': 'solar',
  'roofing-installation-contractors': 'roofing',
  'home-security-contractors': 'homesecurity',
  'siding-installation-contractors': 'siding'
};

const SLIDE_INTERVAL = 4000;

// Cards visible in the services slider at once — the track stops sliding this many cards early
const VISIBLE_SLIDES = 4;

@Component({
  selector: 'app-roofing',
  templateUrl: './roofing.component.html',
  styleUrls: ['./roofing.component.scss'],
})
export class RoofingComponent implements OnInit, AfterViewInit, OnDestroy {

  path_name: string;

  heroData: any;
  features: any;
  aboutData: any;
  statsData: any;
  servicesData: any;
  marqueeData: any;
  sliderData: any;
  comparisonData: any;
  testimonialData: any;

  // Shared by the services slider and the testimonial slider
  currentIndex = 0;

  private sliderInterval: any;

  constructor(
    private activatedRoute: ActivatedRoute,
    private dataService: DataService,
  ) {
    const slug = this.activatedRoute.snapshot.paramMap.get('page-name') ?? '';
    this.path_name = SERVICE_SLUGS[slug] || slug;
  }

  ngOnInit(): void {
    this.heroData = this.dataService.heroContent[this.path_name];
    this.features = this.dataService.features;
    this.aboutData = this.dataService.aboutContent[this.path_name];
    this.statsData = this.dataService.statsContent[this.path_name];
    this.servicesData = this.dataService.servicesContent[this.path_name];
    this.marqueeData = this.dataService.marqueeContent[this.path_name];
    this.sliderData = this.dataService.sliderContent[this.path_name];
    this.comparisonData = this.dataService.comparisonContent[this.path_name];
    this.testimonialData = this.dataService.testimonialContent[this.path_name];

    this.sliderInterval = setInterval(() => {
      this.currentIndex++;

      if (this.currentIndex > this.sliderData.services.length - VISIBLE_SLIDES) {
        this.currentIndex = 0;
      }
    }, SLIDE_INTERVAL);
  }

  ngOnDestroy(): void {
    clearInterval(this.sliderInterval);
  }

  ngAfterViewInit() {
    this.text_animation();
    this.About_Animation();
    this.initServicesAnimation();
  }

  // *\---------------- Text animation -----------------\*

  text_animation(): void {
    const tl = gsap.timeline();
    tl.from('.tagline', { y: 50, opacity: 0, duration: 0.8, ease: 'power3.out' })
      .from('#hero h1', { y: 60, opacity: 0, duration: 1, ease: 'power3.out' }, "-=0.4")
      .from('#hero h5', { y: 60, opacity: 0, duration: 1, ease: 'power3.out' }, "-=0.5")
      .from('.cta-group', { y: 40, opacity: 0, duration: 0.8, ease: 'power3.out' }, "-=0.5")
      .from('.hero-quote-card', { x: 70, opacity: 0, duration: 0.8, ease: 'power3.out' }, "-=0.5");
  }

  // *\----------------  About  animation -----------------\*

  About_Animation() {
    const tl = gsap.timeline({
      scrollTrigger: { trigger: '#about-us', start: 'top 70%', toggleActions: 'play none none none' }
    });

    tl.from('#about-us .section-header', { y: 60, opacity: 0, duration: 0.8 })
      .from('#about-us .about-image', { x: -100, opacity: 0, duration: 1 }, '-=0.4')
      .from('#about-us .feature-box', { x: 100, opacity: 0, stagger: 0.2, duration: 0.8 }, '-=0.6');
  }

  initServicesAnimation() {
    const tl = gsap.timeline({
      scrollTrigger: { trigger: '#services', start: 'top 75%', end: 'bottom 20%', toggleActions: 'play none none none' }
    });

    tl.from('#services .left-content', { x: -100, opacity: 0, duration: 1, ease: 'power3.out' })
      .from('#services .right-content', { x: 100, opacity: 0, duration: 1, ease: 'power3.out' }, '-=0.7');
  }

}
