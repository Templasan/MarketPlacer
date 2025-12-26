import { Injectable, signal, computed, inject, effect } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Product } from '../models/product.interface';
import { CartItem } from '../models/cart-item.interface';
import { MatSnackBar } from '@angular/material/snack-bar';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment'; // Importação do environment

@Injectable({ providedIn: 'root' })
export class CartService {
  private http = inject(HttpClient);
  private snackBar = inject(MatSnackBar);
  
  // URL base agora vinda do environment para apontar para a porta 5000 no Docker
  private readonly API_URL = `${environment.apiUrl}/cart`; 
  private readonly STORAGE_KEY = 'marketplacer_cart_v2';

  // ESTADO: Usando CartItem para agrupar (Produto + Quantidade)
  private cartItems = signal<CartItem[]>([]);

  // SELECTORS (Computados)
  items = computed(() => this.cartItems());
  
  // count: Soma as quantidades totais de itens no carrinho
  count = computed(() => 
    this.cartItems().reduce((acc, item) => acc + item.quantity, 0)
  );

  // total: Soma do Preço unitário * Quantidade de cada item
  total = computed(() => 
    this.cartItems().reduce((acc, item) => acc + (item.product.preco * item.quantity), 0)
  );

  constructor() {
    this.loadInitialCart();

    // Sincroniza o LocalStorage sempre que o sinal cartItems mudar
    effect(() => {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.cartItems()));
    });
  }

  /**
   * Adiciona um produto ao carrinho e sincroniza com o banco via API
   */
  async addToCart(product: Product, quantity: number = 1) {
    if (product.estoque <= 0) {
      this.showToast('Produto sem estoque no momento.');
      return;
    }

    try {
      // 1. POST para o .NET (Sincronização com o container da API)
      await firstValueFrom(this.http.post(`${this.API_URL}/items`, { 
        productId: product.id, 
        quantity: quantity 
      }));

      // 2. Atualização Reativa local com Agrupamento
      this.cartItems.update(items => {
        const index = items.findIndex(i => i.product.id === product.id);
        
        if (index > -1) {
          const updatedItems = [...items];
          updatedItems[index] = { 
            ...updatedItems[index], 
            quantity: updatedItems[index].quantity + quantity 
          };
          return updatedItems;
        }
        return [...items, { product, quantity }];
      });
      
      this.showToast(`${quantity}x ${product.nome} adicionado!`);
      
    } catch (error) {
      console.error('Erro no servidor:', error);
      this.showToast('Erro ao salvar no servidor.');
    }
  }

  /**
   * Remove um item específico do carrinho no banco e localmente
   */
  async removeItem(productId: number) {
    if (!productId) {
      console.error('Falha ao remover: ID do produto é undefined');
      return;
    }

    try {
      // DELETE enviando o ID real do produto para a rota do .NET
      await firstValueFrom(this.http.delete(`${this.API_URL}/items/${productId}`));
      
      // Filtra o Signal para remover a linha da interface
      this.cartItems.update(items => items.filter(i => i.product.id !== productId));
      this.showToast('Item removido do carrinho.');
    } catch (error) {
      console.error('Erro ao remover:', error);
      this.showToast('Erro ao remover do servidor.');
    }
  }

  /**
   * Limpa o estado local e o cache do navegador
   */
  clearCart() {
    this.cartItems.set([]);
    localStorage.removeItem(this.STORAGE_KEY);
  }

  private loadInitialCart() {
    const saved = localStorage.getItem(this.STORAGE_KEY);
    if (saved) {
      try {
        this.cartItems.set(JSON.parse(saved));
      } catch {
        this.cartItems.set([]);
      }
    }
  }

  private showToast(message: string) {
    this.snackBar.open(message, 'Fechar', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top',
    });
  }
}