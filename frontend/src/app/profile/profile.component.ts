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
import { LoggingService } from '../core/logging.service';
import { PasswordRequirementsService } from '../core/password-requirements.service';
import { PasswordRequirementsComponent } from '../shared/password-requirements/password-requirements.component';

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
    BreadcrumbComponent,
    PasswordRequirementsComponent
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
  showPasswordRequirements = false;

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private profileService: ProfileService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private breadcrumbService: BreadcrumbService,
    private loggingService: LoggingService,
    private passwordService: PasswordRequirementsService
  ) {}

  ngOnInit(): void {
    this.setupBreadcrumb();
    this.loadCurrentUser();
    this.initializeForms();
  }

  private setupBreadcrumb(): void {
    this.breadcrumbItems = [
      { label: 'Dashboard', url: '/dashboard' },
      { label: 'Perfil', url: '/profile' }
    ];
    this.breadcrumbService.setBreadcrumbs(this.breadcrumbItems);
  }

  private loadCurrentUser(): void {
    this.currentUser = this.authService.getCurrentUser();
    if (!this.currentUser) {
      this.loggingService.error('Usuário não encontrado');
      this.snackBar.open('Erro ao carregar dados do usuário', 'Fechar', {
        duration: 3000
      });
    }
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
      currentPassword: ['', [Validators.required, Validators.minLength(8)]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(8)]]
    }, { validators: this.passwordMatchValidator });

    // Monitorar mudanças na nova senha para validação
    this.passwordForm.get('newPassword')?.valueChanges.subscribe(password => {
      if (password) {
        const validation = this.passwordService.validatePassword(password);
        this.showPasswordRequirements = !validation.isValid && password.length > 0;
      } else {
        this.showPasswordRequirements = false;
      }
    });
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
    if (this.profileForm.invalid) {
      this.snackBar.open('Por favor, corrija os erros no formulário', 'Fechar', {
        duration: 3000
      });
      return;
    }

    const formValue = this.profileForm.value;
    const updateData = {
      name: formValue.name,
      currentPassword: null,
      newPassword: null
    };

    this.isLoading = true;

    this.profileService.updateProfile(updateData).subscribe({
      next: (response) => {
        this.snackBar.open('Perfil atualizado com sucesso!', 'Fechar', {
          duration: 3000
        });
        
        // Atualizar dados do usuário no localStorage
        if (this.currentUser) {
          this.currentUser.name = formValue.name;
          this.authService.updateCurrentUser(this.currentUser);
        }
        
        this.isLoading = false;
      },
      error: (error) => {
        this.loggingService.error('Erro ao atualizar perfil:', error);
        this.snackBar.open('Erro ao atualizar perfil', 'Fechar', {
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
    
    // Validar a nova senha
    const validation = this.passwordService.validatePassword(formValue.newPassword);
    if (!validation.isValid) {
      this.snackBar.open('Por favor, corrija os erros na senha: ' + validation.errors.join(', '), 'Fechar', {
        duration: 5000
      });
      return;
    }

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
        this.loggingService.error('Erro ao alterar senha:', error);
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