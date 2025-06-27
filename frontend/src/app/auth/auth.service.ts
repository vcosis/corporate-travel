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
    
    console.log('=== loadUserFromStorage ===');
    console.log('Token exists:', !!token);
    console.log('User string:', userStr);
    
    if (token && userStr) {
      try {
        const user = JSON.parse(userStr);
        console.log('Parsed user:', user);
        console.log('User roles:', user.roles);
        this.currentUserSubject.next(user);
      } catch (e) {
        console.error('Error parsing user from storage:', e);
        this.clearAuth();
      }
    }
    console.log('=== End loadUserFromStorage ===');
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { email, password })
      .pipe(
        map(response => {
          console.log('=== Login Response ===');
          console.log('Response:', response);
          console.log('Roles:', response.roles);
          console.log('Roles type:', typeof response.roles);
          console.log('Roles length:', response.roles?.length);
          
          localStorage.setItem('token', response.token);
          const user: User = {
            name: response.name,
            email: response.email,
            roles: response.roles
          };
          console.log('User object to store:', user);
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSubject.next(user);
          console.log('=== End Login Response ===');
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

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    const hasRole = user?.roles?.includes(role) || false;
    console.log(`hasRole(${role}):`, {
      user: user,
      roles: user?.roles,
      hasRole: hasRole
    });
    return hasRole;
  }

  getCurrentUserId(): string | null {
    const token = this.getToken();
    console.log('getCurrentUserId - token:', token ? 'exists' : 'null');
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      console.log('getCurrentUserId - payload:', payload);
      // Tenta os campos mais comuns para ID de usuário em JWT
      const userId = payload.nameid || payload.sub || payload.userId || payload.id || null;
      console.log('getCurrentUserId - userId:', userId);
      return userId;
    } catch (error) {
      console.error('getCurrentUserId - error parsing token:', error);
      return null;
    }
  }

  updateCurrentUser(updatedUser: User): void {
    this.currentUserSubject.next(updatedUser);
    localStorage.setItem('user', JSON.stringify(updatedUser));
  }
} 