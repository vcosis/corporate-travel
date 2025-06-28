import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoggingService } from '../core/logging.service';

export interface TravelRequest {
  id: string;
  requestCode: string;
  requestingUserId?: string;
  requestingUserName?: string;
  origin: string;
  destination: string;
  startDate: string;
  endDate: string;
  reason: string;
  purpose: string;
  status: string;
  approverId?: string;
  approverName?: string;
  approvalDate?: string;
  createdAt: string;
  updatedAt?: string;
  requestingUser?: any;
  approver?: any;
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

export interface TravelRequestFilters {
  period?: string;
  requestingUser?: string;
  approver?: string;
  sortBy?: string;
  sortOrder?: string;
  startDate?: string;
  endDate?: string;
}

@Injectable({ providedIn: 'root' })
export class TravelRequestService {
  private apiUrl = environment.apiUrl + '/travelrequests';

  constructor(
    private http: HttpClient,
    private loggingService: LoggingService
  ) { }

  create(travelRequest: Partial<TravelRequest>): Observable<TravelRequest> {
    return this.http.post<TravelRequest>(this.apiUrl, travelRequest);
  }

  getAll(
    page = 1, 
    pageSize = 10, 
    status?: string, 
    searchTerm?: string, 
    additionalFilters?: TravelRequestFilters
  ): Observable<PaginatedResult<TravelRequest>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    
    if (status) params = params.set('status', status);
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    
    // Adicionar filtros adicionais
    if (additionalFilters) {
      if (additionalFilters.period) params = params.set('period', additionalFilters.period);
      if (additionalFilters.requestingUser) params = params.set('requestingUser', additionalFilters.requestingUser);
      if (additionalFilters.approver) params = params.set('approver', additionalFilters.approver);
      if (additionalFilters.sortBy) params = params.set('sortBy', additionalFilters.sortBy);
      if (additionalFilters.sortOrder) params = params.set('sortOrder', additionalFilters.sortOrder);
      if (additionalFilters.startDate) params = params.set('startDate', additionalFilters.startDate);
      if (additionalFilters.endDate) params = params.set('endDate', additionalFilters.endDate);
    }
    
    return this.http.get<PaginatedResult<TravelRequest>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<TravelRequest> {
    this.loggingService.debug('TravelRequestService.getById called with id:', id);
    return this.http.get<TravelRequest>(`${this.apiUrl}/${id}`);
  }

  update(id: string, travelRequest: Partial<TravelRequest>): Observable<TravelRequest> {
    return this.http.put<TravelRequest>(`${this.apiUrl}/${id}`, travelRequest);
  }

  approve(id: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/approve`, {});
  }

  reject(id: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/reject`, {});
  }

  delete(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  batchApprove(ids: string[]): Observable<any> {
    return this.http.post(`${this.apiUrl}/batch-approve`, { ids });
  }

  batchReject(ids: string[]): Observable<any> {
    return this.http.post(`${this.apiUrl}/batch-reject`, { ids });
  }

  batchDelete(ids: string[]): Observable<any> {
    return this.http.post(`${this.apiUrl}/batch-delete`, { ids });
  }

  // get all, get by id, approve, reject methods will be added here
} 