import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService, User } from '../auth/auth.service';
import { BreadcrumbComponent, BreadcrumbItem } from '../shared/breadcrumb/breadcrumb.component';
import { BreadcrumbService } from '../shared/breadcrumb/breadcrumb.service';
import { ProfileService } from './profile.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    ReactiveFormsModule,
    BreadcrumbComponent
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  isLoading = false;
  isPasswordLoading = false;
  currentUser: User | null = null;
  breadcrumbItems: BreadcrumbItem[] = [];

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private profileService: ProfileService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private breadcrumbService: BreadcrumbService
  ) {}

  ngOnInit(): void {
    this.initializeBreadcrumb();
    this.loadCurrentUser();
    this.initializeForms();
  }

  private initializeBreadcrumb(): void {
    this.breadcrumbService.setProfileBreadcrumb();
    this.breadcrumbItems = this.breadcrumbService.getBreadcrumbs();
  }

  private loadCurrentUser(): void {
    this.currentUser = this.authService.getCurrentUser();
    if (!this.currentUser) {
      this.snackBar.open('Erro ao carregar dados do usuário', 'Fechar', {
        duration: 3000
      });
      return;
    }

    // Carregar dados completos do perfil do backend
    this.isLoading = true;
    this.profileService.getProfile().subscribe({
      next: (response) => {
        if (response.user) {
          this.currentUser = {
            name: response.user.name,
            email: response.user.email,
            roles: response.user.roles,
            createdAt: response.user.createdAt
          };
          this.initializeForms();
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar perfil:', error);
        this.snackBar.open('Erro ao carregar dados do perfil', 'Fechar', {
          duration: 3000
        });
        this.isLoading = false;
      }
    });
  }

  private initializeForms(): void {
    // Formulário de informações pessoais
    this.profileForm = this.formBuilder.group({
      name: [this.currentUser?.name || '', [Validators.required, Validators.minLength(2)]],
      email: [this.currentUser?.email || '', [Validators.required, Validators.email]]
    });

    // Desabilitar email (não pode ser alterado)
    this.profileForm.get('email')?.disable();

    // Formulário de alteração de senha
    this.passwordForm = this.formBuilder.group({
      currentPassword: ['', [Validators.required, Validators.minLength(6)]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(form: FormGroup): { [key: string]: any } | null {
    const newPassword = form.get('newPassword')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;

    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      return { passwordMismatch: true };
    }

    return null;
  }

  saveProfile(): void {
    if (this.profileForm.get('name')?.invalid) {
      this.snackBar.open('Por favor, corrija os erros no formulário', 'Fechar', {
        duration: 3000
      });
      return;
    }

    const formValue = this.profileForm.value;
    const updateData = {
      name: formValue.name
    };

    this.isLoading = true;

    this.profileService.updateProfile(updateData).subscribe({
      next: (response) => {
        this.snackBar.open('Informações atualizadas com sucesso!', 'Fechar', {
          duration: 3000
        });
        
        // Atualizar dados locais
        if (this.currentUser) {
          this.currentUser.name = formValue.name;
          this.authService.updateCurrentUser(this.currentUser);
        }
        
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao atualizar informações:', error);
        this.snackBar.open('Erro ao atualizar informações', 'Fechar', {
          duration: 3000
        });
        this.isLoading = false;
      }
    });
  }

  savePassword(): void {
    if (this.passwordForm.invalid) {
      this.snackBar.open('Por favor, corrija os erros no formulário de senha', 'Fechar', {
        duration: 3000
      });
      return;
    }

    const formValue = this.passwordForm.value;
    const updateData = {
      name: this.currentUser?.name || '', // Manter o nome atual
      currentPassword: formValue.currentPassword,
      newPassword: formValue.newPassword
    };

    this.isPasswordLoading = true;

    this.profileService.updateProfile(updateData).subscribe({
      next: (response) => {
        this.snackBar.open('Senha alterada com sucesso!', 'Fechar', {
          duration: 3000
        });
        
        this.clearPasswordForm();
        this.isPasswordLoading = false;
      },
      error: (error) => {
        console.error('Erro ao alterar senha:', error);
        this.snackBar.open('Erro ao alterar senha', 'Fechar', {
          duration: 3000
        });
        this.isPasswordLoading = false;
      }
    });
  }

  clearPasswordForm(): void {
    this.passwordForm.reset();
  }

  getRolesDisplay(roles: string[]): string {
    return roles.join(', ');
  }

  getFormattedDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('pt-BR');
  }
} 