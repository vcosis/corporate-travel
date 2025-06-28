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
import { FormControl, ReactiveFormsModule, FormGroup } from '@angular/forms';
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
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatChipsModule } from '@angular/material/chips';
import { SelectionModel } from '@angular/cdk/collections';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { LoggingService } from '../../core/logging.service';

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
    BreadcrumbComponent,
    MatTabsModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatChipsModule,
    MatCheckboxModule
  ],
  templateUrl: './travel-request-list.html',
  styleUrls: ['./travel-request-list.scss']
})
export class TravelRequestListComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = [];
  dataSource = new MatTableDataSource<TravelRequest>([]);
  pendingDataSource = new MatTableDataSource<TravelRequest>([]);
  historyDataSource = new MatTableDataSource<TravelRequest>([]);
  isLoading = false;
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  breadcrumbItems: BreadcrumbItem[] = [];
  isManager = false;
  isAdmin = false;
  selectedTabIndex = 0;
  pendingRequests: TravelRequest[] = [];
  historyRequests: TravelRequest[] = [];
  selection = new SelectionModel<TravelRequest>(true, []);

  // Filtros refinados
  pendingFilters = new FormGroup({
    period: new FormControl('7days'),
    requestingUser: new FormControl(''),
    sortBy: new FormControl('createdAt'),
    sortOrder: new FormControl('desc')
  });

  historyFilters = new FormGroup({
    status: new FormControl(''),
    period: new FormControl('30days'),
    requestingUser: new FormControl(''),
    approver: new FormControl(''),
    sortBy: new FormControl('approvalDate'),
    sortOrder: new FormControl('desc')
  });

  // Opções de filtros
  periodOptions = [
    { value: '7days', label: 'Últimos 7 dias' },
    { value: '30days', label: 'Últimos 30 dias' },
    { value: '3months', label: 'Últimos 3 meses' },
    { value: '6months', label: 'Últimos 6 meses' },
    { value: '1year', label: 'Último ano' },
    { value: 'all', label: 'Todas' }
  ];

  statusOptions = [
    { value: '', label: 'Todos os status' },
    { value: 'approved', label: 'Aprovadas' },
    { value: 'rejected', label: 'Rejeitadas' }
  ];

  sortOptions = [
    { value: 'createdAt', label: 'Data de criação' },
    { value: 'startDate', label: 'Data de início' },
    { value: 'requestingUserName', label: 'Solicitante' },
    { value: 'requestCode', label: 'Código da solicitação' }
  ];

  historySortOptions = [
    { value: 'approvalDate', label: 'Data de aprovação' },
    { value: 'createdAt', label: 'Data de criação' },
    { value: 'startDate', label: 'Data de início' },
    { value: 'requestingUserName', label: 'Solicitante' },
    { value: 'approverName', label: 'Aprovador' }
  ];

  sortOrderOptions = [
    { value: 'desc', label: 'Mais recentes primeiro' },
    { value: 'asc', label: 'Mais antigas primeiro' }
  ];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private travelRequestService: TravelRequestService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private breadcrumbService: BreadcrumbService,
    private loggingService: LoggingService
  ) {}

  ngOnInit(): void {
    this.isManager = this.authService.hasRole('Manager');
    this.isAdmin = this.authService.hasRole('Admin');
    this.loggingService.debug('TravelRequestListComponent initialized');
    this.loggingService.debug('Current user:', this.authService.getCurrentUser());
    this.loggingService.debug('Is authenticated:', this.authService.isAuthenticated());
    this.loggingService.debug('Has Manager role:', this.authService.hasRole('Manager'));
    
    this.initializeBreadcrumb();
    this.setDisplayedColumns();
    this.setupFilterSubscriptions();
    this.loadTravelRequests();
  }

  private setupFilterSubscriptions(): void {
    // Filtros da aba pendentes
    this.pendingFilters.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => {
      this.pageIndex = 0;
      this.loadTravelRequests();
    });

    // Filtros da aba histórico
    this.historyFilters.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => {
      this.pageIndex = 0;
      this.loadTravelRequests();
    });
  }

  private setDisplayedColumns(): void {
    this.displayedColumns = ['requestCode', 'requestingUserName', 'origin', 'destination', 'startDate', 'status'];
    
    // Adicionar coluna de ações apenas para administradores
    if (this.isAdmin) {
      this.displayedColumns.push('actions');
    }
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
    
    // Determinar filtros baseados na aba selecionada
    let statusFilter = '';
    let additionalFilters: any = {};
    
    if (this.selectedTabIndex === 0) {
      // Aba Pendentes
      statusFilter = 'pending';
      additionalFilters = this.pendingFilters.value;
    } else {
      // Aba Histórico
      additionalFilters = this.historyFilters.value;
      statusFilter = additionalFilters['status'] || '';
    }

    this.travelRequestService.getAll(
      this.pageIndex + 1, 
      this.pageSize, 
      statusFilter, 
      undefined,
      additionalFilters
    ).subscribe({
      next: (result) => {
        this.loggingService.debug('Travel requests loaded:', result);
        this.dataSource.data = result.items;
        this.totalCount = result.totalCount;
        this.loggingService.debug('Total count set to:', this.totalCount);
        this.separateRequests(result.items);
        this.isLoading = false;
      },
      error: (error) => {
        this.loggingService.error('Erro ao carregar solicitações de viagem', error);
        this.isLoading = false;
      }
    });
  }

  separateRequests(requests: TravelRequest[]): void {
    this.pendingRequests = requests.filter(req => {
      const status = String(req.status).toLowerCase();
      return status === '0' || status === 'pending';
    });
    
    this.historyRequests = requests.filter(req => {
      const status = String(req.status).toLowerCase();
      return status === '1' || status === 'approved' || status === '2' || status === 'rejected';
    });

    this.pendingDataSource.data = this.pendingRequests;
    this.historyDataSource.data = this.historyRequests;
  }

  onTabChange(index: number): void {
    this.selectedTabIndex = index;
    this.pageIndex = 0;
    // Atualizar o paginator baseado na aba selecionada
    if (this.paginator) {
      this.paginator.pageIndex = 0;
      this.paginator.pageSize = this.pageSize;
    }
    this.loadTravelRequests();
  }

  formatDate(dateString: string): string {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleDateString('pt-BR');
  }

  getStatusClass(status: any): string {
    if (status === null || status === undefined) return '';
    
    const value = String(status).toLowerCase();
    switch (value) {
      case '0':
      case 'pending':
        return 'status-pending';
      case '1':
      case 'approved':
        return 'status-approved';
      case '2':
      case 'rejected':
        return 'status-rejected';
      default:
        return '';
    }
  }

  onPageChange(event?: any) {
    this.loggingService.debug('Page change event:', event);
    if (event) {
      this.pageIndex = event.pageIndex;
      this.pageSize = event.pageSize;
      this.loggingService.debug('New page index:', this.pageIndex, 'New page size:', this.pageSize);
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

  delete(id: string): void {
    this.travelRequestService.delete(id).subscribe({
      next: () => {
        this.snackBar.open('Solicitação de viagem deletada com sucesso!', 'Fechar', {
          duration: 3000
        });
        this.loadTravelRequests();
      },
      error: (error) => {
        this.loggingService.error('Erro ao deletar solicitação de viagem', error);
      }
    });
  }

  canEdit(row: TravelRequest): boolean {
    // Apenas administradores podem editar
    if (!this.isAdmin) return false;
    
    // Apenas solicitações pendentes podem ser editadas
    const status = String(row.status).toLowerCase();
    const isPending = status === '0' || status === 'pending';
    
    return isPending;
  }

  canDelete(row: TravelRequest): boolean {
    // Apenas administradores podem excluir
    if (!this.isAdmin) return false;
    
    // Apenas solicitações pendentes podem ser excluídas
    const status = String(row.status).toLowerCase();
    const isPending = status === '0' || status === 'pending';
    
    return isPending;
  }

  getDeleteTooltip(row: TravelRequest): string {
    if (!this.isAdmin) {
      return 'Apenas administradores podem excluir solicitações';
    }
    
    const status = String(row.status).toLowerCase();
    if (status === '1' || status === 'approved') {
      return 'Solicitações aprovadas não podem ser excluídas';
    } else if (status === '2' || status === 'rejected') {
      return 'Solicitações rejeitadas não podem ser excluídas';
    } else {
      return 'Excluir solicitação';
    }
  }

  getEditTooltip(row: TravelRequest): string {
    if (!this.isAdmin) {
      return 'Apenas administradores podem editar solicitações';
    }
    
    const status = String(row.status).toLowerCase();
    if (status === '1' || status === 'approved') {
      return 'Solicitações aprovadas não podem ser editadas';
    } else if (status === '2' || status === 'rejected') {
      return 'Solicitações rejeitadas não podem ser editadas';
    } else {
      return 'Editar solicitação';
    }
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

  get hasPendingSelection(): boolean {
    return this.selection.selected.length > 0;
  }

  toggleAllPendingRows(): void {
    if (this.isAllPendingSelected()) {
      this.selection.clear();
    } else {
      this.pendingDataSource.data.forEach(row => this.selection.select(row));
    }
  }

  isAllPendingSelected(): boolean {
    const numSelected = this.selection.selected.length;
    const numRows = this.pendingDataSource.data.length;
    return numSelected === numRows && numRows > 0;
  }

  togglePendingRow(row: TravelRequest): void {
    this.selection.toggle(row);
  }

  get pendingTableColumns() {
    return (this.isAdmin || this.isManager)
      ? ['select', ...this.displayedColumns]
      : this.displayedColumns;
  }

  async batchApprove() {
    const selected = this.selection.selected;
    if (!selected.length) return;
    try {
      await this.travelRequestService.batchApprove(selected.map(r => r.id)).toPromise();
      this.snackBar.open('Solicitações aprovadas com sucesso!', 'Fechar', { duration: 3000 });
      this.selection.clear();
      this.loadTravelRequests();
    } catch (e) {
      this.snackBar.open('Erro ao aprovar solicitações.', 'Fechar', { duration: 3000 });
    }
  }

  async batchReject() {
    const selected = this.selection.selected;
    if (!selected.length) return;
    try {
      await this.travelRequestService.batchReject(selected.map(r => r.id)).toPromise();
      this.snackBar.open('Solicitações rejeitadas com sucesso!', 'Fechar', { duration: 3000 });
      this.selection.clear();
      this.loadTravelRequests();
    } catch (e) {
      this.snackBar.open('Erro ao rejeitar solicitações.', 'Fechar', { duration: 3000 });
    }
  }

  async batchDelete() {
    const selected = this.selection.selected;
    if (!selected.length) return;
    try {
      await this.travelRequestService.batchDelete(selected.map(r => r.id)).toPromise();
      this.snackBar.open('Solicitações excluídas com sucesso!', 'Fechar', { duration: 3000 });
      this.selection.clear();
      this.loadTravelRequests();
    } catch (e) {
      this.snackBar.open('Erro ao excluir solicitações.', 'Fechar', { duration: 3000 });
    }
  }

  exportHistory() {
    this.isLoading = true;
    const filters = Object.fromEntries(
      Object.entries(this.historyFilters.value).map(([k, v]) => [k, v ?? undefined])
    );
    this.travelRequestService.getAll(1, 1000, '', '', filters).subscribe({
      next: (result) => {
        const items = result.items;
        const csv = this.convertToCSV(items);
        const blob = new Blob([csv], { type: 'text/csv' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'historico-solicitacoes.csv';
        a.click();
        window.URL.revokeObjectURL(url);
        this.isLoading = false;
      },
      error: () => {
        this.snackBar.open('Erro ao exportar histórico.', 'Fechar', { duration: 3000 });
        this.isLoading = false;
      }
    });
  }

  convertToCSV(items: TravelRequest[]): string {
    const header = ['Código', 'Solicitante', 'Origem', 'Destino', 'Data de Início', 'Status'];
    const rows = items.map(item => [
      item.requestCode,
      item.requestingUserName,
      item.origin,
      item.destination,
      item.startDate,
      this.getStatusDescription(item.status)
    ]);
    return [header, ...rows].map(e => e.join(';')).join('\n');
  }
}

