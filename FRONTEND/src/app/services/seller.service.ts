import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment'; // Importação do environment

// Interfaces para tipagem forte
export interface Product {
  id?: number;
  nome: string;
  descricao: string;
  preco: number;
  estoque: number;
  categoria: string;
  imagemURL?: string;
  ativo: boolean;
}

export interface Order {
  id: number;
  userId: number;
  status: string;
  orderDate: string;
  total: number;
  orderItems: any[];
}

@Injectable({ providedIn: 'root' })
export class SellerService {
  private http = inject(HttpClient);
  
  // URLs montadas dinamicamente com base no environment
  private readonly API_PRODUCT = `${environment.apiUrl}/product`;
  private readonly API_ORDERS = `${environment.apiUrl}/orders`;

  // --- GESTÃO DE PRODUTOS ---

  /**
   * Busca apenas os produtos do vendedor logado.
   */
  getMeusProdutos(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.API_PRODUCT}/meus-produtos`);
  }

  /**
   * Busca a lista de categorias válidas definidas no backend.
   */
  getCategorias(): Observable<string[]> {
    return this.http.get<string[]>(`${this.API_PRODUCT}/categories`);
  }

  /**
   * Cria um novo produto enviando FormData (suporta upload de arquivo IFormFile).
   */
  criarProduto(formData: FormData): Observable<Product> {
    return this.http.post<Product>(this.API_PRODUCT, formData);
  }

  /**
   * Atualiza um produto existente via PUT.
   */
  atualizarProduto(id: number, formData: FormData): Observable<Product> {
    return this.http.put<Product>(`${this.API_PRODUCT}/${id}`, formData);
  }

  /**
   * Remove (ou inativa) um produto do estoque via DELETE.
   */
  excluirProduto(id: number): Observable<any> {
    return this.http.delete(`${this.API_PRODUCT}/${id}`);
  }

  // --- GESTÃO DE VENDAS (HISTÓRICO) ---

  /**
   * Busca todos os pedidos que contêm produtos deste vendedor.
   */
  getMinhasVendas(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.API_ORDERS}/vendas`);
  }

  /**
   * Atualiza o status de uma venda (ex: 'Enviado', 'Entregue', 'Cancelado').
   */
  atualizarStatusVenda(id: number, status: string): Observable<any> {
    // Note: O objeto enviado { status } bate com o DTO UpdateStatusRequest do seu C#
    return this.http.patch(`${this.API_ORDERS}/${id}/status`, { status });
  }
}