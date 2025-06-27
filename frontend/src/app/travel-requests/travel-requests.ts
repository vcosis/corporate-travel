import { Component } from '@angular/core';
import { TravelRequestListComponent } from './travel-request-list/travel-request-list';

@Component({
  selector: 'app-travel-requests',
  standalone: true,
  imports: [TravelRequestListComponent],
  templateUrl: './travel-requests.html'
})
export class TravelRequestsComponent {}
