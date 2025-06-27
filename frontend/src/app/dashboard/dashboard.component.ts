import { Component, OnInit, OnDestroy } from '@angular/core';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { DashboardService, DashboardStats } from './dashboard.service';
import { TravelRequest } from '../travel-requests/travel-request.service';
import { Subscription } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { BaseChartDirective } from 'ng2-charts';
import { NgFor, NgIf, NgClass, DatePipe } from '@angular/common';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { BreadcrumbComponent, BreadcrumbItem } from '../shared/breadcrumb/breadcrumb.component';
import { BreadcrumbService } from '../shared/breadcrumb/breadcrumb.service';
import { ThemeService } from '../core/theme.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    MatCardModule, 
    MatIconModule, 
    MatProgressBarModule,
    BaseChartDirective, 
    NgFor, 
    NgIf,
    NgClass,
    DatePipe,
    MatSnackBarModule,
    BreadcrumbComponent
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss'],
})
export class DashboardComponent implements OnInit, OnDestroy {
  private subscription: Subscription = new Subscription();

  public pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    plugins: {
      legend: {
        display: true,
        position: 'top',
      },
    },
  };
  public pieChartData: ChartData<'pie', number[], string | string[]> = {
    labels: ['Pending', 'Approved', 'Rejected'],
    datasets: [{
      data: [0, 0, 0],
      backgroundColor: ['#ff9800', '#4caf50', '#f44336']
    }]
  };
  public pieChartType: ChartType = 'pie';

  stats: DashboardStats = {
    pending: 0,
    approved: 0,
    rejected: 0,
    total: 0
  };

  recentRequests: TravelRequest[] = [];
  loading = false;
  error = false;
  breadcrumbItems: BreadcrumbItem[] = [];

  constructor(
    private dashboardService: DashboardService,
    private snackBar: MatSnackBar,
    private breadcrumbService: BreadcrumbService,
    private themeService: ThemeService
  ) {}

  ngOnInit(): void {
    this.initializeBreadcrumb();
    this.loadDashboardData();
    this.themeService.theme$.subscribe(() => {
      this.updatePieChart();
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  private initializeBreadcrumb(): void {
    this.breadcrumbService.setDashboardBreadcrumb();
    this.breadcrumbItems = this.breadcrumbService.getBreadcrumbs();
  }

  private loadDashboardData(): void {
    this.loading = true;
    this.error = false;

    this.subscription.add(
      this.dashboardService.getStats().subscribe({
        next: (stats) => {
          this.stats = stats;
          this.updatePieChart();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading dashboard stats:', error);
          this.error = true;
          this.loading = false;
          this.snackBar.open('Failed to load dashboard statistics', 'Close', { duration: 3000 });
        }
      })
    );

    this.subscription.add(
      this.dashboardService.getRecentRequests().subscribe({
        next: (requests) => {
          this.recentRequests = requests;
        },
        error: (error) => {
          console.error('Error loading recent requests:', error);
          this.snackBar.open('Failed to load recent requests', 'Close', { duration: 3000 });
        }
      })
    );
  }

  private getStatusColors(): string[] {
    const styles = getComputedStyle(document.body);
    return [
      styles.getPropertyValue('--pending-bg').trim() || '#1976d2',   // Pending
      styles.getPropertyValue('--approved-bg').trim() || '#388e3c',  // Approved
      styles.getPropertyValue('--rejected-bg').trim() || '#d32f2f'   // Rejected
    ];
  }

  private updatePieChart(): void {
    this.pieChartData = {
      labels: ['Pending', 'Approved', 'Rejected'],
      datasets: [{
        data: [this.stats.pending, this.stats.approved, this.stats.rejected],
        backgroundColor: this.getStatusColors()
      }]
    };
    // Atualizar opções para legendas e labels no dark mode
    const styles = getComputedStyle(document.documentElement);
    const textColor = styles.getPropertyValue('--text-primary').trim() || '#212121';
    const baseOptions = this.pieChartOptions || {};
    const basePlugins = baseOptions.plugins || {};
    const baseLegend = basePlugins.legend || {};
    const backgroundColor = styles.getPropertyValue('--card-color').trim() || '#23272b';

    this.pieChartOptions = {
      ...baseOptions,
      plugins: {
        ...basePlugins,
        legend: {
          ...baseLegend,
          labels: {
            color: textColor
          }
        }
      },
      backgroundColor: backgroundColor
    };
  }

  getStatusClass(status: string): string {
    return typeof status === 'string' ? status.toLowerCase() : '';
  }

  getStatusIcon(status: string): string {
    if (typeof status !== 'string') {
      return 'schedule'; // default icon for non-string status
    }
    
    switch (status.toLowerCase()) {
      case 'approved':
        return 'check_circle';
      case 'rejected':
        return 'cancel';
      case 'pending':
      default:
        return 'schedule';
    }
  }
} 