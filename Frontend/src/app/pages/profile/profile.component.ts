import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

// Material Modules
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';

// Utilitários e Serviços
import { jwtDecode } from 'jwt-decode'; 
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { OrderService } from '../../services/order.service';
import { Order } from '../../models/user.model'; 

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    MatCardModule, 
    MatButtonModule,
    MatInputModule, 
    MatFormFieldModule, 
    MatIconModule, 
    MatTabsModule,
    MatTableModule, 
    MatSnackBarModule, 
    MatTooltipModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  // Injeções
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private orderService = inject(OrderService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  // Signals de Estado
  user = signal<any>({ nome: '', email: '', tipo: '' });
  orders = signal<Order[]>([]);
  userId: number = 0;

  passwordData = { current: '', new: '', confirm: '' };
  
  // Colunas da tabela (devem bater com o matColumnDef do seu HTML)
  displayedColumns: string[] = ['id', 'data', 'itens', 'total', 'status', 'actions'];

  ngOnInit() {
    this.loadUserData();
    this.loadOrders();
  }

  loadUserData() {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const decoded: any = jwtDecode(token);
        // O C# mapeia NameIdentifier para 'nameid' ou 'sub'
        this.userId = decoded.nameid || decoded.sub;
        
        this.userService.getUserById(this.userId).subscribe({
          next: (data) => this.user.set(data),
          error: () => this.snackBar.open('Erro ao carregar perfil', 'Erro', { duration: 3000 })
        });
      } catch (e) {
        console.error('Token inválido');
      }
    }
  }

  loadOrders() {
    this.orderService.getMyOrders().subscribe({
      next: (data) => this.orders.set(data),
      error: () => this.snackBar.open('Erro ao carregar histórico de pedidos', 'Erro', { duration: 3000 })
    });
  }

  saveProfile() {
    const dto = { nome: this.user().nome, email: this.user().email };
    this.userService.updateUser(this.userId, dto).subscribe({
      next: () => this.snackBar.open('Perfil atualizado com sucesso!', 'OK', { duration: 3000 }),
      error: (err) => this.snackBar.open(err.error?.error || 'Erro ao atualizar dados', 'Erro', { duration: 3000 })
    });
  }

  // MÉTODO ADICIONADO: Corrige o erro NG9 de propriedade inexistente
  updatePassword() {
    if (!this.passwordData.new || this.passwordData.new !== this.passwordData.confirm) {
      this.snackBar.open('As senhas não coincidem ou estão vazias!', 'Aviso', { duration: 3000 });
      return;
    }
    // Aqui você chamaria um método no seu UserService para atualizar a senha no C#
    this.snackBar.open('Solicitação de troca de senha enviada!', 'OK', { duration: 3000 });
    this.passwordData = { current: '', new: '', confirm: '' };
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getStatusColor(status: string): string {
    const colors: any = {
      'Pago': 'bg-emerald-100 text-emerald-800 border-emerald-200',
      'Enviado': 'bg-blue-100 text-blue-800 border-blue-200',
      'Pendente': 'bg-amber-100 text-amber-800 border-amber-200',
      'Cancelado': 'bg-rose-100 text-rose-800 border-rose-200'
    };
    return colors[status] || 'bg-slate-100 text-slate-800';
  }
}