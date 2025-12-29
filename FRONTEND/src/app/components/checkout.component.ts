import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CartService } from '../services/cart.service';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ImgUrlPipe } from '../pipes/img-url.pipe';
import { OrderService } from '../services/order.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatDividerModule, MatSnackBarModule, ImgUrlPipe],
  template: `
    <div class="min-h-screen bg-slate-50 p-4 md:p-12">
      <div class="max-w-4xl mx-auto">
        
        <button mat-button (click)="voltar()" class="mb-8 !text-slate-500">
          <mat-icon>arrow_back</mat-icon> Continuar Comprando
        </button>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
          
          <div class="lg:col-span-2 space-y-4">
            <h1 class="text-3xl font-black text-slate-900 mb-6">Finalizar Pedido</h1>
            
            @if (cart.items().length === 0) {
              <div class="bg-white p-12 rounded-3xl text-center border-2 border-dashed border-slate-200">
                <p class="text-slate-500 font-medium">Seu carrinho está vazio.</p>
              </div>
            }

            @for (item of cart.items(); track item.product.id) {
              <div class="bg-white p-4 rounded-3xl shadow-sm border border-slate-100 flex gap-4 items-center">
                <img [src]="item.product.imagemURL | imgUrl" class="w-24 h-24 rounded-2xl object-cover">
                
                <div class="flex-grow">
                  <h3 class="font-bold text-slate-800">{{ item.product.nome }}</h3>
                  <p class="text-slate-500 text-[10px] uppercase font-bold tracking-wider">Vendedor #{{ item.product.vendedorId }}</p>
                  <div class="flex justify-between items-center mt-2">
                    <span class="font-black text-indigo-600">{{ item.product.preco | currency:'BRL' }}</span>
                    <span class="text-sm font-medium bg-slate-100 px-3 py-1 rounded-full">Qtd: {{ item.quantity }}</span>
                  </div>
                </div>
              </div>
            }
          </div>

          <div class="lg:col-span-1">
            <div class="bg-white p-8 rounded-[2.5rem] shadow-xl shadow-slate-200/60 sticky top-8">
              <h2 class="text-xl font-bold mb-6">Resumo</h2>
              
              <div class="space-y-3 mb-6">
                <div class="flex justify-between text-slate-500">
                  <span>Subtotal</span>
                  <span>{{ cart.total() | currency:'BRL' }}</span>
                </div>
                <div class="flex justify-between text-slate-500">
                  <span>Frete</span>
                  <span class="text-emerald-500 font-bold">Grátis</span>
                </div>
                <mat-divider></mat-divider>
                <div class="flex justify-between text-xl font-black text-slate-900 pt-2">
                  <span>Total</span>
                  <span>{{ cart.total() | currency:'BRL' }}</span>
                </div>
              </div>

              <button mat-flat-button color="primary" 
                      [disabled]="cart.count() === 0 || isProcessing()"
                      (click)="finalizarCompra()"
                      class="w-full !py-8 !rounded-2xl !text-lg !font-bold shadow-lg shadow-indigo-200">
                {{ isProcessing() ? 'Processando...' : 'Confirmar Pagamento' }}
              </button>

              <p class="text-[10px] text-center text-slate-400 mt-4 uppercase tracking-widest font-bold">
                Ambiente Seguro <mat-icon class="!text-[12px] !w-3 !h-3">lock</mat-icon>
              </p>
            </div>
          </div>

        </div>
      </div>
    </div>
  `
})
export class CheckoutComponent {
  cart = inject(CartService);
  private orderService = inject(OrderService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);
  
  isProcessing = signal(false);

  async finalizarCompra() {
    const itensCarrinho = this.cart.items();
    if (itensCarrinho.length === 0) return;

    this.isProcessing.set(true);

    try {
      // 1. AGRUPAMENTO POR VENDEDOR (Split de Pedido)
      // Criamos um mapa onde a chave é o ID do vendedor e o valor é a lista de itens dele
      const gruposPorVendedor = itensCarrinho.reduce((acc, item) => {
        const vId = item.product.vendedorId;
        if (!acc[vId]) acc[vId] = [];
        acc[vId].push(item);
        return acc;
      }, {} as Record<number, any[]>);

      // 2. PROCESSAMENTO DE CADA GRUPO
      const vendedorIds = Object.keys(gruposPorVendedor);
      
      for (const vId of vendedorIds) {
        const itensDoVendedor = gruposPorVendedor[Number(vId)];
        
        const productIds = itensDoVendedor.map(i => i.product.id);
        const quantities = itensDoVendedor.map(i => i.quantity);

        // Cria o pedido específico para este vendedor no C#
        const pedido = await firstValueFrom(this.orderService.createOrder(productIds, quantities));

        // Simula o pagamento para este pedido específico
        await firstValueFrom(this.orderService.payOrder(pedido.id));
      }

      // 3. SUCESSO TOTAL
      this.snackBar.open('🚀 Compra realizada! Seus pedidos foram gerados por vendedor.', 'OK', { duration: 5000 });
      this.cart.clearCart();
      this.router.navigate(['/profile']); // Vai para o perfil ver os pedidos

    } catch (error: any) {
      console.error('Falha no checkout:', error);
      const errorMsg = error.error?.error || 'Erro ao processar um dos pedidos.';
      this.snackBar.open('❌ ' + errorMsg, 'Fechar', { duration: 7000 });
    } finally {
      this.isProcessing.set(false);
    }
  }

  voltar() {
    this.router.navigate(['/']);
  }
}