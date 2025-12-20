import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly API_URL = 'http://localhost:5235/api/auth';

  isAuthenticated = signal<boolean>(!!localStorage.getItem('token'));

  // Auxiliar para o Interceptor
  getToken(): string | null {
    return localStorage.getItem('token');
  }

  async login(credentials: any) {
    const response = await firstValueFrom(
      this.http.post<{ token: string }>(`${this.API_URL}/login`, credentials)
    );
    
    if (response.token) {
      localStorage.setItem('token', response.token);
      this.isAuthenticated.set(true);
    }
    return response;
  }

  async register(userData: any) {
    return await firstValueFrom(
      this.http.post(`${this.API_URL}/register`, userData)
    );
  }

  logout() {
    localStorage.removeItem('token');
    this.isAuthenticated.set(false);
  }
}