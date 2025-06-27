import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface UpdateProfileRequest {
  name: string;
  currentPassword?: string;
  newPassword?: string;
}

export interface ProfileResponse {
  success: boolean;
  message: string;
  user?: {
    name: string;
    email: string;
    roles: string[];
    createdAt: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private apiUrl = `${environment.apiUrl}/profile`;

  constructor(private http: HttpClient) {}

  getProfile(): Observable<ProfileResponse> {
    return this.http.get<ProfileResponse>(this.apiUrl);
  }

  updateProfile(updateData: UpdateProfileRequest): Observable<ProfileResponse> {
    return this.http.put<ProfileResponse>(this.apiUrl, updateData);
  }
} 