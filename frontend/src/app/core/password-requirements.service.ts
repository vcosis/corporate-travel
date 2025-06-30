import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PasswordRequirements {
  minimumLength: number;
  requireDigit: boolean;
  requireLowercase: boolean;
  requireUppercase: boolean;
  requireNonAlphanumeric: boolean;
  description: string;
}

@Injectable({
  providedIn: 'root'
})
export class PasswordRequirementsService {
  private requirements: PasswordRequirements | null = null;

  constructor(private http: HttpClient) {}

  getPasswordRequirements(): Observable<PasswordRequirements> {
    return this.http.get<PasswordRequirements>(`${environment.apiUrl}/auth/password-requirements`);
  }

  validatePassword(password: string): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    if (!password) {
      errors.push('A senha é obrigatória.');
      return { isValid: false, errors };
    }

    if (password.length < 8) {
      errors.push('A senha deve ter pelo menos 8 caracteres.');
    }

    if (!/\d/.test(password)) {
      errors.push('A senha deve conter pelo menos um dígito.');
    }

    if (!/[a-z]/.test(password)) {
      errors.push('A senha deve conter pelo menos uma letra minúscula.');
    }

    if (!/[A-Z]/.test(password)) {
      errors.push('A senha deve conter pelo menos uma letra maiúscula.');
    }

    if (!/[^a-zA-Z0-9]/.test(password)) {
      errors.push('A senha deve conter pelo menos um caractere especial.');
    }

    return { isValid: errors.length === 0, errors };
  }

  getPasswordStrength(password: string): { strength: 'weak' | 'medium' | 'strong'; percentage: number } {
    let score = 0;
    
    if (password.length >= 8) score += 20;
    if (password.length >= 12) score += 10;
    if (/\d/.test(password)) score += 20;
    if (/[a-z]/.test(password)) score += 20;
    if (/[A-Z]/.test(password)) score += 20;
    if (/[^a-zA-Z0-9]/.test(password)) score += 10;

    if (score < 40) return { strength: 'weak', percentage: score };
    if (score < 80) return { strength: 'medium', percentage: score };
    return { strength: 'strong', percentage: score };
  }
} 