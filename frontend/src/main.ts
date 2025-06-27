import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { routes } from './app/app-routing-module';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { AuthInterceptor } from './app/core/auth-interceptor-interceptor';
import { provideNativeDateAdapter } from '@angular/material/core';

// Registrar controllers do Chart.js
import { Chart, registerables } from 'chart.js';
Chart.register(...registerables);

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([AuthInterceptor])),
    provideAnimations(),
    provideNativeDateAdapter()
  ]
}).catch((err: unknown) => console.error(err));
