import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { TravelRequestService } from '../travel-request.service';
import { AuthService } from '../../auth/auth.service';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { LoggingService } from '../../core/logging.service';

@Component({
  selector: 'app-travel-request-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    RouterModule,
    MatSnackBarModule
  ],
  templateUrl: './travel-request-form.html',
  styleUrls: ['./travel-request-form.scss']
})
export class TravelRequestFormComponent implements OnInit {
  travelRequestForm!: FormGroup;
  loading = false;
  isEditMode = false;
  travelRequestId: string | null = null;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private travelRequestService: TravelRequestService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private loggingService: LoggingService
  ) { }

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }

    this.travelRequestForm = this.fb.group({
      origin: ['', Validators.required],
      destination: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      reason: ['', Validators.required]
    });

    // Check if we're in edit mode
    this.route.params.subscribe(params => {
      this.travelRequestId = params['id'];
      if (this.travelRequestId) {
        this.isEditMode = true;
        this.loadTravelRequest();
      }
    });
  }

  loadTravelRequest(): void {
    if (!this.travelRequestId) return;

    this.loggingService.debug('Loading travel request with ID:', this.travelRequestId);

    this.loading = true;
    this.travelRequestService.getById(this.travelRequestId).subscribe({
      next: (travelRequest) => {
        this.loggingService.debug('Travel request loaded successfully:', travelRequest);
        this.travelRequestForm.patchValue({
          origin: travelRequest.origin,
          destination: travelRequest.destination,
          startDate: new Date(travelRequest.startDate),
          endDate: new Date(travelRequest.endDate),
          reason: travelRequest.reason
        });
        this.loading = false;
      },
      error: (err) => {
        this.loggingService.error('Erro ao carregar requisição de viagem', err);
        this.router.navigate(['/travel-requests']);
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.travelRequestForm.valid) {
      this.loading = true;
      
      if (this.isEditMode && this.travelRequestId) {
        // Update existing travel request
        this.travelRequestService.update(this.travelRequestId, this.travelRequestForm.value).subscribe({
          next: () => {
            this.snackBar.open('Travel request updated successfully', 'Close', { duration: 2000 });
            this.router.navigate(['/travel-requests']);
            this.loading = false;
          },
          error: (err) => {
            this.loggingService.error('Erro ao atualizar requisição de viagem', err);
            this.loading = false;
          }
        });
      } else {
        // Create new travel request
        this.travelRequestService.create(this.travelRequestForm.value).subscribe({
          next: () => {
            this.snackBar.open('Travel request created successfully', 'Close', { duration: 2000 });
            this.router.navigate(['/travel-requests']);
            this.loading = false;
          },
          error: (err) => {
            this.loggingService.error('Erro ao criar requisição de viagem', err);
            this.loading = false;
          }
        });
      }
    }
  }
}
