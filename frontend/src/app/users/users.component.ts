import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { UserService, User, PaginatedResult } from './user.service';
import { EditUserDialogComponent } from './edit-user-dialog/edit-user-dialog.component';
import { CreateUserDialogComponent } from './create-user-dialog/create-user-dialog.component';
import { AuthService } from '../auth/auth.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../shared/breadcrumb/breadcrumb.component';
import { BreadcrumbService } from '../shared/breadcrumb/breadcrumb.service';

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
    MatSnackBarModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    RouterModule,
    BreadcrumbComponent
  ],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['name', 'email', 'roles', 'createdAt', 'actions'];
  dataSource = new MatTableDataSource<User>([]);
  isLoading = false;
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  filterControl = new FormControl('');
  filterValue = '';
  breadcrumbItems: BreadcrumbItem[] = [];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private userService: UserService,
    private router: Router,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private authService: AuthService,
    private breadcrumbService: BreadcrumbService
  ) {}

  ngOnInit(): void {
    console.log('UsersComponent initialized');
    console.log('Current user:', this.authService.getCurrentUser());
    console.log('Is authenticated:', this.authService.isAuthenticated());
    console.log('Has Admin role:', this.authService.hasRole('Admin'));
    
    this.initializeBreadcrumb();
    this.loadUsers();
    this.filterControl.valueChanges.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(value => {
      this.filterValue = value || '';
      this.pageIndex = 0;
      this.loadUsers();
    });
  }

  private initializeBreadcrumb(): void {
    this.breadcrumbService.setUsersBreadcrumb();
    this.breadcrumbItems = this.breadcrumbService.getBreadcrumbs();
  }

  ngAfterViewInit() {
    // this.dataSource.paginator = this.paginator; // REMOVIDO para server-side paging
    this.dataSource.sort = this.sort;
    if (this.paginator) {
      this.paginator.page.subscribe(() => this.onPageChange());
    }
  }

  loadUsers() {
    this.isLoading = true;
    console.log('Loading users - Page:', this.pageIndex + 1, 'PageSize:', this.pageSize, 'Filter:', this.filterValue);
    
    this.userService.getUsers(this.pageIndex + 1, this.pageSize, this.filterValue)
      .subscribe({
        next: (result: PaginatedResult<User>) => {
          console.log('Users loaded:', result);
          this.dataSource.data = result.items;
          this.totalCount = result.totalCount;
          console.log('Total count set to:', this.totalCount);
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Erro ao carregar usuários:', error);
          this.snackBar.open('Erro ao carregar usuários', 'Fechar', {
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
    this.loadUsers();
  }

  getRolesDisplay(roles: string[]): string {
    return roles.join(', ');
  }

  getFormattedDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('pt-BR');
  }

  createUser() {
    const dialogRef = this.dialog.open(CreateUserDialogComponent, {
      width: '500px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Usuário criado com sucesso!', 'Fechar', {
          duration: 3000
        });
        this.loadUsers(); // Recarregar a lista
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
        this.loadUsers(); // Recarregar a lista
      }
    });
  }

  deleteUser(userId: string) {
    const user = this.dataSource.data.find(u => u.id === userId);
    if (!user) return;

    // Verificar se é um admin
    if (user.roles.includes('Admin')) {
      this.snackBar.open('Não é possível deletar um usuário administrador', 'Fechar', {
        duration: 3000
      });
      return;
    }

    const confirmMessage = `Tem certeza que deseja deletar o usuário "${user.name}"?`;
    if (confirm(confirmMessage)) {
      this.userService.deleteUser(userId).subscribe({
        next: () => {
          this.snackBar.open('Usuário deletado com sucesso!', 'Fechar', {
            duration: 3000
          });
          this.loadUsers(); // Recarregar a lista
        },
        error: (error) => {
          console.error('Erro ao deletar usuário:', error);
          this.snackBar.open('Erro ao deletar usuário', 'Fechar', {
            duration: 3000
          });
        }
      });
    }
  }
} 