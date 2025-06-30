import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { CommonModule } from '@angular/common';
import { environment } from '../../environments/environment';
import { PasswordRequirementsService } from '../core/password-requirements.service';
import { PasswordRequirementsComponent } from '../shared/password-requirements/password-requirements.component';

@Component({
  selector: 'app-user-registration',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    PasswordRequirementsComponent
  ],
  templateUrl: './user-registration.component.html',
  styleUrls: ['./user-registration.component.scss']
})
export class UserRegistrationComponent {
  registrationForm: FormGroup;
  isLoading = false;
  hidePassword = true;
  showPasswordRequirements = false;
  passwordErrors: string[] = [];

  roles = [
    { value: 'User', label: 'User' },
    { value: 'Manager', label: 'Manager' },
    { value: 'Admin', label: 'Admin' }
  ];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    public router: Router,
    private snackBar: MatSnackBar,
    private passwordService: PasswordRequirementsService
  ) {
    this.registrationForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      role: ['User', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    // Monitorar mudanças na senha para validação
    this.registrationForm.get('password')?.valueChanges.subscribe(password => {
      if (password) {
        const validation = this.passwordService.validatePassword(password);
        this.passwordErrors = validation.errors;
        this.showPasswordRequirements = !validation.isValid && password.length > 0;
      } else {
        this.passwordErrors = [];
        this.showPasswordRequirements = false;
      }
    });
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  onSubmit() {
    if (this.registrationForm.valid) {
      const password = this.registrationForm.value.password;
      const validation = this.passwordService.validatePassword(password);
      
      if (!validation.isValid) {
        this.snackBar.open('Por favor, corrija os erros na senha: ' + validation.errors.join(', '), 'Fechar', {
          duration: 5000,
          horizontalPosition: 'center',
          verticalPosition: 'top'
        });
        return;
      }

      this.isLoading = true;
      
      const userData = {
        name: this.registrationForm.value.name,
        email: this.registrationForm.value.email,
        password: this.registrationForm.value.password,
        role: this.registrationForm.value.role
      };

      this.http.post(`${environment.apiUrl}/auth/register-admin`, userData)
        .subscribe({
          next: () => {
            this.snackBar.open('Usuário cadastrado com sucesso!', 'Fechar', {
              duration: 3000,
              horizontalPosition: 'center',
              verticalPosition: 'top'
            });
            this.router.navigate(['/users']);
          },
          error: (error) => {
            this.isLoading = false;
            let errorMessage = 'Erro ao cadastrar usuário';
            
            if (error.error && Array.isArray(error.error)) {
              errorMessage = error.error.map((e: any) => e.description).join(', ');
            } else if (error.error && typeof error.error === 'string') {
              errorMessage = error.error;
            }
            
            this.snackBar.open(errorMessage, 'Fechar', {
              duration: 5000,
              horizontalPosition: 'center',
              verticalPosition: 'top'
            });
          }
        });
    }
  }

  getErrorMessage(fieldName: string): string {
    const field = this.registrationForm.get(fieldName);
    
    if (field?.hasError('required')) {
      return 'Este campo é obrigatório';
    }
    
    if (fieldName === 'email' && field?.hasError('email')) {
      return 'Email inválido';
    }
    
    if (fieldName === 'name' && field?.hasError('minlength')) {
      return 'Nome deve ter pelo menos 2 caracteres';
    }
    
    if (fieldName === 'password' && field?.hasError('minlength')) {
      return 'Senha deve ter pelo menos 8 caracteres';
    }
    
    if (fieldName === 'confirmPassword' && field?.hasError('passwordMismatch')) {
      return 'Senhas não coincidem';
    }
    
    return '';
  }
} 