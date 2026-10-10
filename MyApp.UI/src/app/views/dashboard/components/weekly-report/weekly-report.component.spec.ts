import { ChartComponent } from 'ng-apexcharts'
import { ComponentFixture, TestBed } from '@angular/core/testing'

import { WeeklyReportComponent } from './weekly-report.component'

describe('WeeklyReportComponent', () => {
  let component: WeeklyReportComponent
  let fixture: ComponentFixture<WeeklyReportComponent>

  beforeEach(async () => {
    // Unit smoke tests do not render third-party charts after fixture teardown.
    spyOn(ChartComponent.prototype, 'ngOnChanges').and.stub()
    await TestBed.configureTestingModule({
      imports: [WeeklyReportComponent],
    }).compileComponents()

    fixture = TestBed.createComponent(WeeklyReportComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
