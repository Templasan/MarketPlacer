import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { animate, state, style, transition, trigger } from '@angular/animations'; // 1. Importação para animação

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
  // 2. Adicionando a animação para expandir/colapsar os detalhes
  animations: [
    trigger('detailExpand', [
      state('collapsed,void', style({height: '0px', minHeight: '0'})),
      state('expanded', style({height: '*'})),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
  imports: [
    CommonModule, 
    FormsModule, 
    RouterLink,
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
  // Injeções de Dependência
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private orderService = inject(OrderService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  // Estados com Signals
  user = signal<any>({ nome: '', email: '', tipo: '' });
  orders = signal<any[]>([]); // Usando any[] para flexibilidade com os campos calculados
  userId: number = 0;

  // Controle da Tabela Expansível
  // Adicionamos 'expand' nas colunas e criamos a variável de controle
  columnsToDisplay: string[] = ['id', 'data', 'total', 'status', 'expand'];
  expandedElement: any | null = null;

  // Dados de formulário
  passwordData = { current: '', new: '', confirm: '' };

  ngOnInit() {
    this.loadUserData();
    this.loadOrders();
  }

  /**
   * Decodifica o token para obter o ID e busca os dados do usuário
   */
  loadUserData() {
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const decoded: any = jwtDecode(token);
        this.userId = Number(decoded.nameid || decoded.sub || decoded.id);
        
        this.userService.getUserById(this.userId).subscribe({
          next: (data) => this.user.set(data),
          error: () => this.snackBar.open('Erro ao carregar dados do perfil', 'Erro', { duration: 3000 })
        });
      } catch (e) {
        console.error('Token inválido ou corrompido');
        this.logout();
      }
    } else {
      this.router.navigate(['/login']);
    }
  }

  /**
   * Busca o histórico de pedidos e prepara os dados para exibição (Cálculo de Totais)
   */
  loadOrders() {
    this.orderService.getMyOrders().subscribe({
      next: (data: any[]) => {
        // Mapeamos os dados vindo do C# para garantir que temos o total calculado
        // e os nomes dos campos alinhados com o HTML
        const mappedOrders = data.map(order => {
          // Calcula o valor total do pedido somando (unitPrice * quantity) de cada item
          const totalValue = order.orderItems?.reduce(
            (acc: number, item: any) => acc + (item.unitPrice * item.quantity), 0
          ) || 0;

          return {
            ...order,
            // Garante que o campo 'data' exista (se o back mandar orderDate)
            data: order.orderDate || order.data, 
            // Armazena o total calculado para exibir na linha principal
            totalCalculado: totalValue,
            // Mantém a lista de itens para ser usada no detalhe expandido
            items: order.orderItems 
          };
        });

        this.orders.set(mappedOrders);
      },
      error: () => this.snackBar.open('Não foi possível carregar seus pedidos', 'Erro', { duration: 3000 })
    });
  }

  /**
   * Salva alterações de Nome/Email
   */
  saveProfile() {
    const dto = { nome: this.user().nome, email: this.user().email };
    this.userService.updateUser(this.userId, dto).subscribe({
      next: () => {
        this.snackBar.open('Perfil atualizado com sucesso!', 'OK', { 
          duration: 3000,
          panelClass: ['success-snackbar'] 
        });
      },
      error: (err) => {
        const msg = err.error?.message || err.error?.error || 'Erro ao atualizar dados';
        this.snackBar.open(msg, 'Erro', { duration: 4000 });
      }
    });
  }

  /**
   * Lógica completa de alteração de senha
   */
  updatePassword() {
    if (!this.passwordData.current || !this.passwordData.new) {
      this.snackBar.open('Preencha a senha atual e a nova senha.', 'Aviso', { duration: 3000 });
      return;
    }

    if (this.passwordData.new !== this.passwordData.confirm) {
      this.snackBar.open('A nova senha e a confirmação não coincidem!', 'Erro', { duration: 3000 });
      return;
    }

    if (this.passwordData.new.length < 6) {
      this.snackBar.open('A nova senha deve ter pelo menos 6 caracteres.', 'Aviso', { duration: 3000 });
      return;
    }

    this.userService.changePassword(this.userId, this.passwordData).subscribe({
      next: (res: any) => {
        this.snackBar.open(res?.message || 'Senha alterada com sucesso!', 'Sucesso', { 
          duration: 3000,
          panelClass: ['success-snackbar'] 
        });
        this.passwordData = { current: '', new: '', confirm: '' };
      },
      error: (err) => {
        const errorMsg = err.error?.message || err.error?.error || 
                         (err.status === 404 ? 'Rota não encontrada' : 'Erro ao alterar a senha');
        this.snackBar.open(errorMsg, 'Erro', { duration: 5000 });
      }
    });
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  /**
   * Cores de status compatíveis com Tailwind
   */
  getStatusColor(status: string): string {
    const colors: any = {
      'Pago': 'bg-emerald-100 text-emerald-800 border-emerald-200',
      'Enviado': 'bg-blue-100 text-blue-800 border-blue-200',
      'Pendente': 'bg-amber-100 text-amber-800 border-amber-200',
      'Cancelado': 'bg-rose-100 text-rose-800 border-rose-200',
      'Entregue': 'bg-purple-100 text-purple-800 border-purple-200'
    };
    return colors[status] || 'bg-slate-100 text-slate-800';
  }
}