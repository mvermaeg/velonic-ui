import { AfterViewInit, Component, ElementRef, HostListener, Input, NgZone, OnDestroy, ViewChild } from '@angular/core';

@Component({
  selector: 'app-image-compare',
  templateUrl: './image-compare.component.html',
  styleUrls: ['./image-compare.component.scss'],
})
export class ImageCompareComponent implements AfterViewInit, OnDestroy {

  @Input() before!: string;
  @Input() after!: string;

  @ViewChild('container') container!: ElementRef;
  @ViewChild('afterWrapper') afterWrapper!: ElementRef;
  @ViewChild('slider') slider!: ElementRef;

  isDragging = false;

  current = 50;
  target = 50;

  private frameId = 0;

  constructor(private zone: NgZone) {}

  ngAfterViewInit() {
    // The loop only writes inline styles, so keep it out of the zone
    // instead of running change detection on every frame.
    this.zone.runOutsideAngular(() => this.animate());
  }

  ngOnDestroy() {
    cancelAnimationFrame(this.frameId);
  }

  startDrag(e: MouseEvent | TouchEvent) {
    e.preventDefault();
    this.isDragging = true;
  }

  @HostListener('window:mouseup')
  @HostListener('window:touchend')
  stopDrag() {
    this.isDragging = false;
  }

  @HostListener('window:mousemove', ['$event'])
  @HostListener('window:touchmove', ['$event'])
  onMove(e: MouseEvent | TouchEvent) {
    if (!this.isDragging) return;

    const rect = this.container.nativeElement.getBoundingClientRect();

    const clientX = e instanceof MouseEvent
      ? e.clientX
      : e.touches[0].clientX;

    const percent = ((clientX - rect.left) / rect.width) * 100;
    this.target = Math.max(0, Math.min(100, percent));
  }

  animate() {
    this.frameId = requestAnimationFrame(() => this.animate());

    this.current += (this.target - this.current) * 0.1;

    const value = this.current;

    this.slider.nativeElement.style.left = `${value}%`;
    this.afterWrapper.nativeElement.style.clipPath = `inset(0 ${100 - value}% 0 0)`;
  }

}
