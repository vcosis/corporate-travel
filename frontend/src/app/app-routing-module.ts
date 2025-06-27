import { Routes } from '@angular/router';
import { AuthGuard } from './auth/auth-guard';
import { AdminGuard } from './auth/admin-guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./auth/login/login').then(m => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./layout/layout').then(m => m.LayoutComponent),
    canActivate: [AuthGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'travel-requests',
        loadComponent: () => import('./travel-requests/travel-requests').then(m => m.TravelRequestsComponent)
      },
      {
        path: 'travel-requests/new',
        loadComponent: () => import('./travel-requests/travel-request-form/travel-request-form').then(m => m.TravelRequestFormComponent)
      },
      {
        path: 'notifications',
        loadComponent: () => import('./notifications/notifications.component').then(m => m.NotificationsComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./users/users.component').then(m => m.UsersComponent),
        canActivate: [AdminGuard]
      },
      {
        path: 'user-registration',
        loadComponent: () => import('./user-registration/user-registration.component').then(m => m.UserRegistrationComponent),
        canActivate: [AdminGuard]
      },
      {
        path: 'profile',
        loadComponent: () => import('./profile/profile.component').then(m => m.ProfileComponent)
      }
    ]
  }
];
