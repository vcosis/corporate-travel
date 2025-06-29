import { Component, OnInit, OnDestroy, ViewChild, ChangeDetectorRef } from '@angular/core';
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
import { LoggingService } from '../core/logging.service';

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
  @ViewChild(BaseChartDirective) chart?: BaseChartDirective;
  private subscription: Subscription = new Subscription();
  showChart = true;

  public pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    plugins: {
      legend: {
        display: true,
        position: 'top',
        labels: {
          color: '#212121',
          font: {
            size: 12
          },
          padding: 20
        }
      },
      tooltip: {
        backgroundColor: '#ffffff',
        titleColor: '#212121',
        bodyColor: '#212121',
        borderColor: '#e0e0e0',
        borderWidth: 1,
        cornerRadius: 8,
        displayColors: true,
        titleFont: {
          size: 14,
          weight: 'bold'
        },
        bodyFont: {
          size: 13
        }
      }
    }
  };
  public pieChartData: ChartData<'pie', number[], string | string[]> = {
    labels: ['Pendente', 'Aprovado', 'Reprovado'],
    datasets: [{
      data: [0, 0, 0],
      backgroundColor: ['#ff9800', '#4caf50', '#f44336'],
      borderWidth: 2,
      borderColor: '#ffffff'
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
    private themeService: ThemeService,
    private cdr: ChangeDetectorRef,
    private loggingService: LoggingService
  ) {}

  ngOnInit(): void {
    this.initializeBreadcrumb();
    this.loadDashboardData();
    this.themeService.theme$.subscribe(() => {
      // Forçar recriação completa do gráfico
      this.showChart = false;
      this.cdr.detectChanges();
      
      setTimeout(() => {
        this.updatePieChart();
        this.showChart = true;
        this.cdr.detectChanges();
        
        // Forçar update do gráfico após recriação
        setTimeout(() => {
          if (this.chart && this.chart.chart) {
            this.chart.chart.update('none');
          }
        }, 100);
      }, 50);
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
          this.loggingService.error('Error loading dashboard stats:', error);
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
          this.loggingService.error('Error loading recent requests:', error);
          this.snackBar.open('Failed to load recent requests', 'Close', { duration: 3000 });
        }
      })
    );
  }

  private getStatusColors(): string[] {
    const styles = getComputedStyle(document.body);
    return [
      styles.getPropertyValue('--pending-bg').trim() || '#ff9800',   // Pending
      styles.getPropertyValue('--approved-bg').trim() || '#4caf50',  // Approved
      styles.getPropertyValue('--rejected-bg').trim() || '#f44336'   // Rejected
    ];
  }

  private updatePieChart(): void {
    const isDarkMode = document.body.classList.contains('dark-mode');
    this.loggingService.debug('updatePieChart - Dark mode:', isDarkMode);

    // Definir cores específicas para modo escuro
    const legendColor = isDarkMode ? '#ffffff' : '#212121';
    const tooltipBg = isDarkMode ? '#424242' : '#ffffff';
    const tooltipTextColor = isDarkMode ? '#ffffff' : '#212121';
    const borderColor = isDarkMode ? '#424242' : '#ffffff';

    this.loggingService.debug('updatePieChart - Colors:', {
      legendColor,
      tooltipBg,
      tooltipTextColor,
      borderColor
    });

    this.pieChartData = {
      labels: ['Pendente', 'Aprovado', 'Reprovado'],
      datasets: [{
        data: [this.stats.pending, this.stats.approved, this.stats.rejected],
        backgroundColor: this.getStatusColors(),
        borderWidth: 2,
        borderColor: borderColor
      }]
    };

    this.pieChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: true,
          position: 'top',
          labels: {
            color: legendColor,
            font: {
              size: 12,
              weight: 'bold'
            },
            padding: 20,
            usePointStyle: true,
            pointStyle: 'circle'
          }
        },
        tooltip: {
          backgroundColor: tooltipBg,
          titleColor: tooltipTextColor,
          bodyColor: tooltipTextColor,
          borderColor: isDarkMode ? '#666666' : '#e0e0e0',
          borderWidth: 1,
          cornerRadius: 8,
          displayColors: true,
          titleFont: {
            size: 14,
            weight: 'bold'
          },
          bodyFont: {
            size: 13
          }
        }
      }
    };

    this.loggingService.debug('updatePieChart - Updated chart options');
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