import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TravelRequestList } from './travel-request-list';

describe('TravelRequestList', () => {
  let component: TravelRequestList;
  let fixture: ComponentFixture<TravelRequestList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TravelRequestList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TravelRequestList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
