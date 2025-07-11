import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoggingService } from '../core/logging.service';

export interface User {
  id: string;
  name: string;
  email: string;
  roles: string[];
  createdAt: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface UpdateUserRequest {
  id: string;
  name: string;
  email: string;
  role: string;
}

export interface RegisterUserRequest {
  name: string;
  email: string;
  password: string;
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/users`;

  constructor(
    private http: HttpClient,
    private loggingService: LoggingService
  ) {}

  getUsers(page: number = 1, pageSize: number = 10, searchTerm?: string, roleFilter?: string, statusFilter?: string, sortBy?: string): Observable<PaginatedResult<User>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    if (roleFilter) {
      params = params.set('roleFilter', roleFilter);
    }

    if (statusFilter) {
      params = params.set('statusFilter', statusFilter);
    }

    if (sortBy) {
      params = params.set('sortBy', sortBy);
    }

    const url = this.apiUrl;
    this.loggingService.debug('Making request to:', url, 'with params:', params.toString());
    
    return this.http.get<PaginatedResult<User>>(this.apiUrl, { params });
  }

  registerUser(user: RegisterUserRequest): Observable<any> {
    return this.http.post(`${environment.apiUrl}/auth/register-admin`, user);
  }

  updateUser(user: UpdateUserRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${user.id}`, user);
  }

  deleteUser(userId: string): Observable<any> {
    this.loggingService.debug('=== deleteUser service ===');
    this.loggingService.debug('User ID:', userId);
    this.loggingService.debug('User ID type:', typeof userId);
    this.loggingService.debug('User ID length:', userId.length);
    this.loggingService.debug('Full URL:', `${this.apiUrl}/${userId}`);
    
    if (!userId || userId.trim() === '') {
      this.loggingService.error('Invalid user ID provided');
      throw new Error('ID do usuário é obrigatório');
    }
    
    return this.http.delete(`${this.apiUrl}/${userId}`);
  }
} 