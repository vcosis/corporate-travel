import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { TravelRequestService, TravelRequest, PaginatedResult } from '../travel-request.service';
import { AuthService } from '../../auth/auth.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RouterModule } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CreateTravelRequestDialogComponent } from '../create-travel-request-dialog/create-travel-request-dialog.component';
import { EditTravelRequestDialogComponent } from '../edit-travel-request-dialog/edit-travel-request-dialog.component';
import { TravelRequestDetailsDialogComponent } from '../travel-request-details-dialog/travel-request-details-dialog.component';
import { BreadcrumbComponent, BreadcrumbItem } from '../../shared/breadcrumb/breadcrumb.component';
import { BreadcrumbService } from '../../shared/breadcrumb/breadcrumb.service';

@Component({
  selector: 'app-travel-request-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    RouterModule,
    MatSnackBarModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    MatTooltipModule,
    BreadcrumbComponent
  ],
  templateUrl: './travel-request-list.html',
  styleUrls: ['./travel-request-list.scss']
})
export class TravelRequestListComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['requestCode', 'requestingUserName', 'origin', 'destination', 'startDate', 'status', 'actions'];
  dataSource = new MatTableDataSource<TravelRequest>([]);
  isLoading = false;
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  filterControl = new FormControl('');
  filterValue = '';
  breadcrumbItems: BreadcrumbItem[] = [];
  isManager = false;
  isAdmin = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private travelRequestService: TravelRequestService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private breadcrumbService: BreadcrumbService
  ) {}

  ngOnInit(): void {
    this.isManager = this.authService.hasRole('Manager');
    this.isAdmin = this.authService.hasRole('Admin');
    console.log('TravelRequestListComponent initialized');
    console.log('Current user:', this.authService.getCurrentUser());
    console.log('Is authenticated:', this.authService.isAuthenticated());
    console.log('Has Manager role:', this.authService.hasRole('Manager'));
    
    this.initializeBreadcrumb();
    this.loadTravelRequests();
    this.filterControl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(value => {
      this.filterValue = value || '';
      this.pageIndex = 0;
      this.loadTravelRequests();
    });
  }

  private initializeBreadcrumb(): void {
    this.breadcrumbService.setTravelRequestsBreadcrumb();
    this.breadcrumbItems = this.breadcrumbService.getBreadcrumbs();
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
    if (this.paginator) {
      this.paginator.page.subscribe(() => this.onPageChange());
    }
  }

  loadTravelRequests() {
    this.isLoading = true;
    console.log('Loading travel requests - Page:', this.pageIndex + 1, 'PageSize:', this.pageSize, 'Filter:', this.filterValue);
    
    this.travelRequestService.getAll(this.pageIndex + 1, this.pageSize, undefined, this.filterValue).subscribe({
      next: (result: PaginatedResult<TravelRequest>) => {
        console.log('Travel requests loaded:', result);
        this.dataSource.data = result.items;
        this.totalCount = result.totalCount;
        console.log('Total count set to:', this.totalCount);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar solicitações de viagem:', error);
        this.snackBar.open('Erro ao carregar solicitações de viagem', 'Fechar', {
          duration: 3000
        });
        this.isLoading = false;
      }
    });
  }

  onPageChange(event?: any) {
    console.log('Page change event:', event);
    if (event) {
      this.pageIndex = event.pageIndex;
      this.pageSize = event.pageSize;
      console.log('New page index:', this.pageIndex, 'New page size:', this.pageSize);
    }
    this.loadTravelRequests();
  }

  getStatusDescription(status: any): string {
    if (status === null || status === undefined) return '-';
    
    // Se vier como objeto com propriedade name
    if (typeof status === 'object' && status !== null && 'name' in status) {
      return this.getStatusDescription(status.name);
    }
    
    // Se vier como string textual ou número
    const value = String(status).toLowerCase();
    switch (value) {
      case '0':
      case 'pending':
        return 'Pendente';
      case '1':
      case 'approved':
        return 'Aprovado';
      case '2':
      case 'rejected':
        return 'Rejeitado';
      default:
        return value;
    }
  }

  createTravelRequest() {
    const dialogRef = this.dialog.open(CreateTravelRequestDialogComponent, {
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Solicitação de viagem criada com sucesso!', 'Fechar', {
          duration: 3000
        });
        this.loadTravelRequests(); // Recarregar a lista
      }
    });
  }

  editTravelRequest(travelRequest: TravelRequest) {
    const dialogRef = this.dialog.open(EditTravelRequestDialogComponent, {
      data: { travelRequest },
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Solicitação de viagem atualizada com sucesso!', 'Fechar', {
          duration: 3000
        });
        this.loadTravelRequests(); // Recarregar a lista
      }
    });
  }

  approve(id: string): void {
    this.travelRequestService.approve(id).subscribe({
      next: () => {
        this.snackBar.open('Solicitação aprovada com sucesso!', 'Fechar', { duration: 3000 });
        this.loadTravelRequests();
      },
      error: () => this.snackBar.open('Erro ao aprovar solicitação', 'Fechar', { duration: 3000 })
    });
  }

  reject(id: string): void {
    this.travelRequestService.reject(id).subscribe({
      next: () => {
        this.snackBar.open('Solicitação rejeitada com sucesso!', 'Fechar', { duration: 3000 });
        this.loadTravelRequests();
      },
      error: () => this.snackBar.open('Erro ao rejeitar solicitação', 'Fechar', { duration: 3000 })
    });
  }

  delete(id: string): void {
    const travelRequest = this.dataSource.data.find(tr => tr.id === id);
    if (!travelRequest) return;

    const confirmMessage = `Tem certeza que deseja deletar a solicitação "${travelRequest.origin} → ${travelRequest.destination}"?`;
    if (confirm(confirmMessage)) {
      this.travelRequestService.delete(id).subscribe({
        next: () => {
          this.snackBar.open('Solicitação de viagem deletada com sucesso!', 'Fechar', {
            duration: 3000
          });
          this.loadTravelRequests(); // Recarregar a lista
        },
        error: (error) => {
          console.error('Erro ao deletar solicitação de viagem:', error);
          this.snackBar.open('Erro ao deletar solicitação de viagem', 'Fechar', {
            duration: 3000
          });
        }
      });
    }
  }

  canEdit(row: TravelRequest): boolean {
    // Admin pode editar qualquer solicitação
    if (this.isAdmin) return true;
    // Permissão padrão (exemplo: dono ou status)
    const currentUserId = this.authService.getCurrentUserId();
    return row.requestingUserId === currentUserId;
  }

  canDeleteTest(row: TravelRequest): boolean {
    // Admin pode excluir qualquer solicitação
    if (this.isAdmin) return true;
    // Permissão padrão (apenas pendentes e do dono)
    const status = String(row.status).toLowerCase();
    const isPending = status === '0' || status === 'pending';
    const currentUserId = this.authService.getCurrentUserId();
    let isOwner = false;
    if (currentUserId) {
      if (row.requestingUserId && row.requestingUserId === currentUserId) {
        isOwner = true;
      } else if (row.requestingUser && row.requestingUser.id === currentUserId) {
        isOwner = true;
      }
    }
    return isPending && isOwner;
  }

  openDetailsDialog(travelRequest: TravelRequest) {
    const dialogRef = this.dialog.open(TravelRequestDetailsDialogComponent, {
      data: { travelRequest },
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === 'approved') {
        this.snackBar.open('Solicitação aprovada com sucesso!', 'Fechar', {
          duration: 3000
        });
        this.loadTravelRequests(); // Recarregar a lista
      } else if (result === 'rejected') {
        this.snackBar.open('Solicitação rejeitada com sucesso!', 'Fechar', {
          duration: 3000
        });
        this.loadTravelRequests(); // Recarregar a lista
      }
    });
  }
}

