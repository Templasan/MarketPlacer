import { Component, inject, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CartService } from '../services/cart.service';

@Component({
  selector: 'app-cart-drawer',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  template: `
    <div class="h-full flex flex-col p-6 bg-white shadow-xl">
      <div class="flex items-center justify-between border-b pb-4 mb-4">
        <h2 class="text-xl font-bold text-slate-800 flex items-center gap-2">
          <mat-icon class="text-indigo-600">shopping_cart</mat-icon> 
          Carrinho ({{ cart.count() }})
        </h2>
        <button mat-icon-button (click)="onClose.emit()">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div class="flex-grow overflow-y-auto space-y-4 pr-2">
        @for (item of cart.items(); track item.product.id) {
          <div class="flex gap-4 bg-slate-50 p-3 rounded-2xl border border-slate-100 items-center">
            <img [src]="item.product.imagemURL" class="w-16 h-16 rounded-xl object-cover border border-white shadow-sm">
            
            <div class="flex-grow">
              <h4 class="font-bold text-slate-800 text-sm line-clamp-1">{{ item.product.nome }}</h4>
              
              <div class="flex items-center gap-2">
                <span class="bg-indigo-100 text-indigo-700 text-[10px] font-bold px-2 py-0.5 rounded-md">
                  {{ item.quantity }}x
                </span>
                <span class="text-indigo-600 font-bold text-xs">
                  {{ item.product.preco | currency:'BRL' }}
                </span>
              </div>
            </div>

            <button mat-icon-button color="warn" (click)="cart.removeItem(item.product.id)">
              <mat-icon>delete_outline</mat-icon>
            </button>
          </div>
        } @empty {
          <div class="h-full flex flex-col items-center justify-center text-slate-400 opacity-50">
            <mat-icon class="!w-16 !h-16 !text-6xl mb-4">remove_shopping_cart</mat-icon>
            <p>O carrinho está vazio</p>
          </div>
        }
      </div>

      @if (cart.count() > 0) {
        <div class="border-t pt-6 mt-4 space-y-4">
          <div class="flex justify-between items-center px-2">
            <span class="text-slate-500 font-medium">Subtotal</span>
            <span class="text-2xl font-black text-slate-900">{{ cart.total() | currency:'BRL' }}</span>
          </div>
          <button mat-flat-button color="primary" 
                  class="w-full !py-7 !text-lg !rounded-2xl !font-bold shadow-lg shadow-indigo-100 transition-transform active:scale-95">
            Finalizar Compra
          </button>
        </div>
      }
    </div>
  `
})
export class CartDrawerComponent {
  cart = inject(CartService);
  onClose = output<void>();
}