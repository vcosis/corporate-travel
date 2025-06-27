import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

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

  constructor(private http: HttpClient) {}

  getUsers(page: number = 1, pageSize: number = 10, searchTerm?: string): Observable<PaginatedResult<User>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    const url = this.apiUrl;
    console.log('Making request to:', url, 'with params:', params.toString());
    
    return this.http.get<PaginatedResult<User>>(this.apiUrl, { params });
  }

  registerUser(user: RegisterUserRequest): Observable<any> {
    return this.http.post(`${environment.apiUrl}/auth/register-admin`, user);
  }

  updateUser(user: UpdateUserRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${user.id}`, user);
  }

  deleteUser(userId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${userId}`);
  }
} 