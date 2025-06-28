import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface User {
  name: string;
  email: string;
  roles: string[];
  createdAt?: string;
}

export interface AuthResponse {
  token: string;
  name: string;
  email: string;
  roles: string[];
  expiresAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadUserFromStorage();
  }

  private loadUserFromStorage(): void {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');
    
    if (token && userStr) {
      // Verificar se o token não está expirado
      if (this.isTokenExpired()) {
        this.clearAuth();
        return;
      }
      
      try {
        const user = JSON.parse(userStr);
        this.currentUserSubject.next(user);
      } catch (e) {
        this.clearAuth();
      }
    }
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { email, password })
      .pipe(
        map(response => {
          localStorage.setItem('token', response.token);
          const user: User = {
            name: response.name,
            email: response.email,
            roles: response.roles
          };
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSubject.next(user);
          return response;
        })
      );
  }

  register(name: string, email: string, password: string, role: string = 'User'): Observable<any> {
    return this.http.post(`${environment.apiUrl}/auth/register`, { name, email, password, role });
  }

  logout(): void {
    this.clearAuth();
  }

  private clearAuth(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) {
      return true;
    }
    
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Date.now() / 1000;
      const expirationTime = payload.exp;
      
      return expirationTime < currentTime;
    } catch (error) {
      return true;
    }
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    
    // Verificar se o token não está expirado
    if (this.isTokenExpired()) {
      this.clearAuth();
      return false;
    }
    
    return true;
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.roles?.includes(role) || false;
  }

  getCurrentUserId(): string | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      // Tenta os campos mais comuns para ID de usuário em JWT
      const userId = payload.nameid || payload.sub || payload.userId || payload.id || null;
      return userId;
    } catch (error) {
      return null;
    }
  }

  updateCurrentUser(updatedUser: User): void {
    this.currentUserSubject.next(updatedUser);
    localStorage.setItem('user', JSON.stringify(updatedUser));
  }

  // Método de teste para simular token expirado
  testTokenExpiration(): void {
    if (this.isTokenExpired()) {
      this.clearAuth();
    }
  }
} 