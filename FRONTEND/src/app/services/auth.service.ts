import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment'; // Importação do environment

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  
  // URL resolvida dinamicamente através do arquivo de environment
  private readonly API_URL = `${environment.apiUrl}/auth`;

  isAuthenticated = signal<boolean>(!!localStorage.getItem('token'));

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  // --- FUNÇÃO: EXTRAIR ROLE DO TOKEN JWT ---
  getUserRole(): string {
    const token = this.getToken();
    if (!token) return '';

    try {
      // O JWT é dividido por pontos: Header.Payload.Signature
      // Pegamos a parte do meio (Payload) e decodificamos do Base64
      const payloadBase64 = token.split('.')[1];
      const payloadJson = atob(payloadBase64);
      const decoded = JSON.parse(payloadJson);

      // No ASP.NET Core, a role geralmente vem em um destes dois campos:
      // 1. "role" (configuração simples do JWT)
      // 2. A URL completa do Schema da Microsoft (padrão do Identity)
      return decoded['role'] || 
             decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 
             '';
    } catch (e) {
      console.error('Erro ao decodificar token', e);
      return '';
    }
  }

  /**
   * Realiza o login e armazena o token no LocalStorage.
   */
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

  /**
   * Registra um novo usuário no sistema.
   */
  async register(userData: any) {
    return await firstValueFrom(
      this.http.post(`${this.API_URL}/register`, userData)
    );
  }

  /**
   * Remove o token e limpa o estado de autenticação.
   */
  logout() {
    localStorage.removeItem('token');
    this.isAuthenticated.set(false);
  }
}