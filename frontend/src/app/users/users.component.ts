import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { combineLatest } from 'rxjs';
import { UserService, User, PaginatedResult } from './user.service';
import { EditUserDialogComponent } from './edit-user-dialog/edit-user-dialog.component';
import { CreateUserDialogComponent } from './create-user-dialog/create-user-dialog.component';
import { AuthService } from '../auth/auth.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../shared/breadcrumb/breadcrumb.component';
import { BreadcrumbService } from '../shared/breadcrumb/breadcrumb.service';
import { ConfirmationDialogService } from '../shared/confirmation-dialog/confirmation-dialog.service';
import { SelectionModel } from '@angular/cdk/collections';
import { LoggingService } from '../core/logging.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatDialogModule,
    RouterModule,
    BreadcrumbComponent
  ],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['select', 'name', 'email', 'roles', 'status', 'createdAt', 'actions'];
  dataSource = new MatTableDataSource<User>([]);
  selection = new SelectionModel<User>(true, []);
  isLoading = false;
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  filterControl = new FormControl('');
  roleFilterControl = new FormControl('');
  statusFilterControl = new FormControl('');
  sortByControl = new FormControl('name');
  filterValue = '';
  roleFilterValue = '';
  statusFilterValue = '';
  sortByValue = 'name';
  breadcrumbItems: BreadcrumbItem[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private userService: UserService,
    private router: Router,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private authService: AuthService,
    private breadcrumbService: BreadcrumbService,
    private confirmationDialogService: ConfirmationDialogService,
    private loggingService: LoggingService
  ) {}

  ngOnInit(): void {
    this.loggingService.debug('UsersComponent initialized');
    this.loggingService.debug('Current user:', this.authService.getCurrentUser());
    this.loggingService.debug('Is authenticated:', this.authService.isAuthenticated());
    this.loggingService.debug('Has Admin role:', this.authService.hasRole('Admin'));
    
    this.initializeBreadcrumb();
    this.setupFilters();
    this.loadUsers();
  }

  private initializeBreadcrumb(): void {
    this.breadcrumbService.setUsersBreadcrumb();
    this.breadcrumbItems = this.breadcrumbService.getBreadcrumbs();
  }

  private setupFilters(): void {
    // Combinar todos os filtros
    combineLatest([
      this.filterControl.valueChanges.pipe(debounceTime(400), distinctUntilChanged()),
      this.roleFilterControl.valueChanges.pipe(distinctUntilChanged()),
      this.statusFilterControl.valueChanges.pipe(distinctUntilChanged()),
      this.sortByControl.valueChanges.pipe(distinctUntilChanged())
    ]).subscribe(([searchValue, roleValue, statusValue, sortValue]) => {
      this.filterValue = searchValue || '';
      this.roleFilterValue = roleValue || '';
      this.statusFilterValue = statusValue || '';
      this.sortByValue = sortValue || 'name';
      this.pageIndex = 0;
      this.selection.clear();
      this.loadUsers();
    });
  }

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
    if (this.paginator) {
      this.paginator.page.subscribe(() => this.onPageChange());
    }
  }

  loadUsers() {
    this.isLoading = true;
    this.loggingService.debug('Loading users - Page:', this.pageIndex + 1, 'PageSize:', this.pageSize, 'Filter:', this.filterValue, 'Role:', this.roleFilterValue, 'Status:', this.statusFilterValue, 'Sort:', this.sortByValue);
    
    this.userService.getUsers(this.pageIndex + 1, this.pageSize, this.filterValue, this.roleFilterValue, this.statusFilterValue, this.sortByValue)
      .subscribe({
        next: (result: PaginatedResult<User>) => {
          this.loggingService.debug('Users loaded:', result);
          this.dataSource.data = result.items;
          this.totalCount = result.totalCount;
          this.loggingService.debug('Total count set to:', this.totalCount);
          this.isLoading = false;
        },
        error: (error) => {
          this.loggingService.error('Erro ao carregar usuários', error);
          this.isLoading = false;
        }
      });
  }

  onPageChange(event?: any) {
    this.loggingService.debug('Page change event:', event);
    if (event) {
      this.pageIndex = event.pageIndex;
      this.pageSize = event.pageSize;
      this.loggingService.debug('New page index:', this.pageIndex, 'New page size:', this.pageSize);
    }
    this.loadUsers();
  }

  // Métodos de seleção
  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.dataSource.data.length;
    return numSelected === numRows;
  }

  masterToggle() {
    this.isAllSelected() ?
      this.selection.clear() :
      this.dataSource.data.forEach(row => this.selection.select(row));
  }

  get selectedUsers(): User[] {
    return this.selection.selected;
  }

  // Métodos de filtros
  clearFilters() {
    this.filterControl.setValue('');
    this.roleFilterControl.setValue('');
    this.statusFilterControl.setValue('');
    this.sortByControl.setValue('name');
  }

  // Métodos de exibição
  getRolesDisplay(roles: string[]): string {
    return roles.join(', ');
  }

  getStatusDisplay(user: User): string {
    // Simular status baseado em alguma propriedade do usuário
    // Você pode ajustar conforme sua lógica de negócio
    return user.roles.includes('Admin') ? 'Ativo' : 'Ativo';
  }

  getFormattedDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('pt-BR');
  }

  // Métodos de ações da toolbar
  createUser() {
    const dialogRef = this.dialog.open(CreateUserDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Usuário criado com sucesso!', 'Fechar', {
          duration: 3000
        });
        this.loadUsers();
      }
    });
  }

  updateSelectedUsers() {
    if (this.selectedUsers.length === 1) {
      this.editUser(this.selectedUsers[0]);
    } else if (this.selectedUsers.length > 1) {
      this.snackBar.open('Selecione apenas um usuário para editar', 'Fechar', {
        duration: 3000
      });
    }
  }

  deleteSelectedUsers() {
    if (this.selectedUsers.length === 0) return;

    const adminUsers = this.selectedUsers.filter(user => user.roles.includes('Admin'));
    if (adminUsers.length > 0) {
      this.snackBar.open('Não é possível deletar usuários administradores', 'Fechar', {
        duration: 3000
      });
      return;
    }

    const message = this.selectedUsers.length === 1 
      ? `Tem certeza que deseja excluir o usuário "${this.selectedUsers[0].name}"? Esta ação não pode ser desfeita.`
      : `Tem certeza que deseja excluir ${this.selectedUsers.length} usuários selecionados? Esta ação não pode ser desfeita.`;

    this.confirmationDialogService.confirm({
      title: 'Confirmar Exclusão',
      message: message,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      confirmColor: 'warn',
      type: 'error'
    }).subscribe(confirmed => {
      if (confirmed) {
        const deletePromises = this.selectedUsers.map(user => 
          this.userService.deleteUser(user.id).toPromise()
        );

        Promise.all(deletePromises)
          .then(() => {
            this.snackBar.open(`${this.selectedUsers.length} usuário(s) excluído(s) com sucesso!`, 'Fechar', {
              duration: 3000
            });
            this.selection.clear();
            this.loadUsers();
          })
          .catch(error => {
            this.loggingService.error('Erro ao excluir usuários', error);
          });
      }
    });
  }

  editUser(user: User) {
    const dialogRef = this.dialog.open(EditUserDialogComponent, {
      data: { user },
      width: '500px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Usuário atualizado com sucesso!', 'Fechar', {
          duration: 3000
        });
        this.loadUsers();
      }
    });
  }

  deleteUser(userId: string) {
    this.loggingService.debug('=== deleteUser ===');
    this.loggingService.debug('User ID to delete:', userId);
    this.loggingService.debug('User ID type:', typeof userId);
    this.loggingService.debug('User ID length:', userId.length);

    // Validar se o ID não está vazio
    if (!userId || userId.trim() === '') {
      this.snackBar.open('ID do usuário inválido', 'Fechar', {
        duration: 3000
      });
      return;
    }

    // Encontrar o usuário na lista
    const user = this.dataSource.data.find(u => u.id === userId);
    this.loggingService.debug('Found user:', user);

    if (!user) {
      this.loggingService.debug('User not found in data source');
      this.snackBar.open('Usuário não encontrado', 'Fechar', {
        duration: 3000
      });
      return;
    }

    // Verificar se é um usuário administrador
    if (user.roles.includes('Admin')) {
      this.snackBar.open('Não é possível excluir um usuário administrador', 'Fechar', {
        duration: 3000
      });
      return;
    }

    const message = `Tem certeza que deseja excluir o usuário "${user.name}"? Esta ação não pode ser desfeita.`;

    this.confirmationDialogService.confirm({
      title: 'Confirmar Exclusão',
      message: message,
      confirmText: 'Excluir',
      cancelText: 'Cancelar',
      confirmColor: 'warn',
      type: 'error'
    }).subscribe(confirmed => {
      if (confirmed) {
        this.loggingService.debug('Confirmation confirmed, calling deleteUser service with ID:', userId);

        this.userService.deleteUser(userId).subscribe({
          next: () => {
            this.loggingService.debug('User deleted successfully');
            this.snackBar.open('Usuário excluído com sucesso!', 'Fechar', {
              duration: 3000
            });
            this.loadUsers();
          },
          error: (error) => {
            this.loggingService.error('Erro ao excluir usuário', error);
            this.loggingService.debug('Error status:', error.status);
            this.loggingService.debug('Error message:', error.message);
            this.loggingService.debug('Error error:', error.error);

            let errorMessage = 'Erro ao excluir usuário';
            if (error.error?.error) {
              errorMessage = error.error.error;
            } else if (error.error?.message) {
              errorMessage = error.error.message;
            } else if (error.message) {
              errorMessage = error.message;
            }

            this.snackBar.open(errorMessage, 'Fechar', {
              duration: 5000
            });
          }
        });
      }
    });
  }

  hasActiveFilters(): boolean {
    return !!(
      this.filterControl.value ||
      this.roleFilterControl.value ||
      this.statusFilterControl.value ||
      (this.sortByControl.value && this.sortByControl.value !== 'name')
    );
  }
} 