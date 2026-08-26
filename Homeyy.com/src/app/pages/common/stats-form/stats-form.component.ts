import { Component, ElementRef, Input, OnDestroy, OnInit } from '@angular/core';

@Component({
  selector: 'stats-form',
  templateUrl: './stats-form.component.html',
  styleUrls: ['./stats-form.component.scss'],
})
export class StatsFormComponent implements OnInit, OnDestroy {

  @Input() target = 0;
  @Input() duration = 2000; // ms
  @Input() suffix = '';

  value = 0;

  private observer?: IntersectionObserver;
  private timer?: any;

  constructor(private el: ElementRef) {}

  ngOnInit() {
    // Count up the first time the tile scrolls into view
    this.observer = new IntersectionObserver(entries => {
      if (entries.some(entry => entry.isIntersecting)) {
        this.observer?.disconnect();
        this.animateCount();
      }
    }, { threshold: 0.4 });

    this.observer.observe(this.el.nativeElement);
  }

  ngOnDestroy() {
    this.observer?.disconnect();
    clearInterval(this.timer);
  }

  private animateCount() {
    if (this.target <= 0) {
      this.value = this.target;
      return;
    }

    const stepTime = Math.abs(Math.floor(this.duration / this.target));

    this.timer = setInterval(() => {
      this.value++;

      if (this.value === this.target) {
        clearInterval(this.timer);
      }
    }, stepTime);
  }

}
