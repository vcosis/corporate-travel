import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TravelRequests } from './travel-requests';

describe('TravelRequests', () => {
  let component: TravelRequests;
  let fixture: ComponentFixture<TravelRequests>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TravelRequests]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TravelRequests);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
