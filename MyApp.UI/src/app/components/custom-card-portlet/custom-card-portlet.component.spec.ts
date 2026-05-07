import { ComponentFixture, TestBed } from '@angular/core/testing'

import { CustomCardPortletComponent } from './custom-card-portlet.component'

describe('CustomCardPortletComponent', () => {
  let component: CustomCardPortletComponent
  let fixture: ComponentFixture<CustomCardPortletComponent>

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomCardPortletComponent],
    }).compileComponents()

    fixture = TestBed.createComponent(CustomCardPortletComponent)
    component = fixture.componentInstance
    fixture.detectChanges()
  })

  it('should create', () => {
    expect(component).toBeTruthy()
  })
})
