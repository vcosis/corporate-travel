import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TravelRequestForm } from './travel-request-form';

describe('TravelRequestForm', () => {
  let component: TravelRequestForm;
  let fixture: ComponentFixture<TravelRequestForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TravelRequestForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TravelRequestForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
