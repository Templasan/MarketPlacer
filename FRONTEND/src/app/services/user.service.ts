import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../models/user.model';
import { environment } from '../../environments/environment'; // Importe o seu environment

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  
  // Agora a URL é montada dinamicamente com base no que estiver no environment
  private readonly API_URL = `${environment.apiUrl}/users`;

  getUserById(id: number): Observable<User> {
    return this.http.get<User>(`${this.API_URL}/${id}`);
  }

  updateUser(id: number, userData: { nome: string; email: string }): Observable<void> {
    return this.http.put<void>(`${this.API_URL}/${id}`, userData);
  }

  // Envio corrigido para evitar o erro 400
  changePassword(id: number, data: any): Observable<any> {
    const passwordDto = {
      currentPassword: data.current,
      newPassword: data.new
    };
    return this.http.post<any>(`${this.API_URL}/${id}/change-password`, passwordDto);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }
}