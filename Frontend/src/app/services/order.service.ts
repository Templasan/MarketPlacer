import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order } from '../models/user.model';
import { environment } from '../../environments/environment'; // Importação do environment

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);
  
  // URL resolvida dinamicamente através do arquivo de environment
  private readonly API_URL = `${environment.apiUrl}/orders`;

  /**
   * Busca o histórico de pedidos do usuário autenticado.
   * Rota C#: [HttpGet("meus-pedidos")]
   */
  getMyOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.API_URL}/meus-pedidos`);
  }

  /**
   * Cria um novo pedido no banco de dados.
   * Transforma arrays de IDs e quantidades no DTO esperado pelo C# (CreateOrderRequest).
   */
  createOrder(productIds: number[], quantities: number[]): Observable<Order> {
    // Mapeia para o formato: { productId: x, quantity: y }
    const itensRequest = productIds.map((id, index) => ({
      productId: id,
      quantity: quantities[index]
    }));

    // Envelopa o payload para bater com o record CreateOrderRequest(List<OrderItemRequest> Itens)
    const payload = {
      itens: itensRequest 
    };

    return this.http.post<Order>(this.API_URL, payload);
  }

  /**
   * Simula ou confirma o pagamento de um pedido específico.
   * Rota C#: [HttpPost("{id}/pagar")]
   */
  payOrder(orderId: number): Observable<void> {
    return this.http.post<void>(`${this.API_URL}/${orderId}/pagar`, {});
  }
}