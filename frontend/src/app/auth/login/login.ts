import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { LoggingService } from '../../core/logging.service';
import { PasswordRequirementsService } from '../../core/password-requirements.service';
import { PasswordRequirementsComponent } from '../../shared/password-requirements/password-requirements.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    PasswordRequirementsComponent
  ],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent {
  loginForm: FormGroup;
  loginError = false;
  isLoading = false;
  passwordErrors: string[] = [];
  showPasswordRequirements = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private loggingService: LoggingService,
    private passwordService: PasswordRequirementsService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });

    // Monitorar mudanças na senha para validação
    this.loginForm.get('password')?.valueChanges.subscribe(password => {
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

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.loginError = false;
      
      const password = this.loginForm.value.password;
      const validation = this.passwordService.validatePassword(password);
      
      if (!validation.isValid) {
        this.isLoading = false;
        this.loginError = true;
        this.passwordErrors = validation.errors;
        return;
      }
      
      this.authService.login(this.loginForm.value.email, this.loginForm.value.password).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          this.isLoading = false;
          this.loginError = true;
          this.loggingService.error('Erro no login', error);
        }
      });
    }
  }

  getPasswordErrors(): string {
    return this.passwordErrors.join(', ');
  }
}
