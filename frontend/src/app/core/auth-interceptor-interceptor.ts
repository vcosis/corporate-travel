import { HttpEvent, HttpHandlerFn, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../auth/auth.service';
import { Router } from '@angular/router';

export function AuthInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> {
  const authService = inject(AuthService);
  const router = inject(Router);
  
  const token = authService.getToken();
  
  if (token) {
    const cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    
    return next(cloned).pipe(
      catchError((error: HttpErrorResponse) => {
        console.log('AuthInterceptor - Error caught:', error.status, error.message);
        
        if (error.status === 401) {
          console.log('AuthInterceptor - 401 Unauthorized detected, logging out user');
          
          // Limpar autenticação
          authService.logout();
          
          // Redirecionar para login
          console.log('AuthInterceptor - Redirecting to login page');
          router.navigate(['/login']).then(() => {
            console.log('AuthInterceptor - Successfully redirected to login');
          }).catch(err => {
            console.error('AuthInterceptor - Error redirecting to login:', err);
          });
        }
        
        return throwError(() => error);
      })
    );
  }
  
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      console.log('AuthInterceptor - Error caught (no token):', error.status, error.message);
      return throwError(() => error);
    })
  );
}
