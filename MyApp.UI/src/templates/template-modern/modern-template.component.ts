import { CommonModule } from '@angular/common'
import { Component, Input } from '@angular/core'

@Component({
  selector: 'app-modern-template',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modern-template.component.html',
  styleUrl: './modern-template.component.scss',
})
export class ModernTemplateComponent {
  @Input() site: any

  isCollapsed = false

  navItems = [
    { label: 'Home', link: '#' },
    { label: 'About', link: '#' },
    { label: 'Services', link: '#' },
    { label: 'Projects', link: '#' },
    { label: 'Contact', link: '#' },
  ]

  features = [
    {
      icon: '<i class="fa-solid fa-award"></i>',
      title: 'Certified Experts',
      desc: 'Professional and trusted service experts.',
    },
    {
      icon: '<i class="fa-solid fa-clock"></i>',
      title: 'Fast Response',
      desc: 'Quick support and emergency availability.',
    },
    {
      icon: '<i class="fa-solid fa-shield-halved"></i>',
      title: 'Reliable Service',
      desc: 'Quality-focused and customer-first approach.',
    },
  ]

  marquee = [
    { title: 'Professional Service' },
    { title: 'Trusted Experts' },
    { title: 'Fast Response' },
    { title: 'Premium Quality' },
    { title: '24/7 Support' },
  ]

  toggleNavbar() {
    this.isCollapsed = !this.isCollapsed
  }
}