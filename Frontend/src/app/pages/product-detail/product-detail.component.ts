import { Component, inject, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductService } from '../../services/product.service'; 
import { CartService } from '../../services/cart.service';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Product } from '../../models/product.interface';
import { switchMap, filter } from 'rxjs';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatChipsModule, MatSnackBarModule],
  template: `
    <div class="min-h-screen bg-slate-50 py-8 md:py-16">
      <div class="container mx-auto px-4 max-w-6xl">
        
        <nav class="flex items-center gap-2 text-slate-400 text-sm mb-8">
          <a href="/" class="hover:text-indigo-600 transition-colors">Início</a>
          <mat-icon class="!w-4 !h-4 !text-sm">chevron_right</mat-icon>
          <span class="text-slate-600 font-medium">Detalhes do Produto</span>
        </nav>

        @if (product()) {
          @let p = product()!;
          <div class="grid grid-cols-1 lg:grid-cols-12 gap-12 items-start">
            
            <div class="lg:col-span-7 group">
              <div class="relative bg-white rounded-[2.5rem] p-4 shadow-sm border border-slate-100 overflow-hidden">
                <img [src]="p.imagemURL" [alt]="p.nome" 
                     class="w-full h-auto max-h-[600px] object-contain rounded-[2rem] group-hover:scale-105 transition-transform duration-700">
                
                <span class="absolute top-8 left-8 bg-indigo-600 text-white px-4 py-1.5 rounded-full text-xs font-bold uppercase tracking-widest shadow-lg">
                  {{ p.categoria }}
                </span>
              </div>
            </div>

            <div class="lg:col-span-5 flex flex-col gap-8">
              <div class="space-y-4">
                <h1 class="text-4xl md:text-5xl font-black text-slate-900 leading-tight">
                  {{ p.nome }}
                </h1>
                
                <div class="flex items-center gap-4">
                  <div class="h-4 w-px bg-slate-200"></div>
                  
                  @if (p.estoque > 0) {
                    <span class="flex items-center gap-1.5 text-emerald-600 font-bold text-sm bg-emerald-50 px-3 py-1 rounded-lg border border-emerald-100">
                      <div class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></div>
                      {{ p.estoque }} em estoque
                    </span>
                  } @else {
                    <span class="text-rose-600 font-bold text-sm bg-rose-50 px-3 py-1 rounded-lg border border-rose-100">
                      Esgotado
                    </span>
                  }
                </div>
              </div>

              <div class="bg-white p-6 rounded-3xl border border-slate-100 shadow-sm">
                <p class="text-slate-500 text-sm mb-1">Preço</p>
                <div class="flex items-baseline gap-3">
                  <span class="text-4xl font-black text-indigo-700">
                    {{ p.preco | currency:'BRL' }}
                  </span>
                </div>
              </div>

              <div class="flex flex-col gap-3">
                <label class="text-sm font-bold text-slate-500 uppercase tracking-widest ml-1">Quantidade</label>
                <div class="flex items-center gap-4 bg-white w-fit p-2 rounded-2xl border border-slate-200 shadow-inner">
                  <button mat-icon-button (click)="updateQuantity(-1)" [disabled]="quantity() <= 1" 
                          class="!bg-slate-100 !rounded-xl transition-all active:scale-90">
                    <mat-icon>remove</mat-icon>
                  </button>
                  
                  <span class="text-xl font-bold w-10 text-center text-slate-800">{{ quantity() }}</span>
                  
                  <button mat-icon-button (click)="updateQuantity(1)" [disabled]="quantity() >= p.estoque"
                          class="!bg-slate-100 !rounded-xl transition-all active:scale-90">
                    <mat-icon>add</mat-icon>
                  </button>
                </div>
                @if(quantity() >= p.estoque && p.estoque > 0) {
                   <p class="text-xs text-amber-600 font-medium ml-1 animate-bounce">Limite de estoque atingido</p>
                }
              </div>

              <div class="space-y-4">
                <h3 class="font-bold text-slate-800 flex items-center gap-2">
                  <mat-icon class="text-indigo-600">description</mat-icon> Descrição
                </h3>
                <p class="text-slate-600 leading-relaxed text-lg italic">
                  "{{ p.descricao }}"
                </p>
              </div>

              <div class="flex flex-col gap-3 mt-4">
                <button mat-flat-button color="primary" 
                        (click)="addToCart(p)"
                        class="!py-8 !text-xl !rounded-[1.5rem] !font-bold transition-all hover:shadow-indigo-200 hover:shadow-2xl active:scale-95"
                        [disabled]="p.estoque <= 0">
                  <mat-icon class="mr-2">shopping_cart</mat-icon> 
                  {{ p.estoque > 0 ? 'Adicionar ao Carrinho' : 'Produto Esgotado' }}
                </button>
              </div>
            </div>
          </div>
        } @else {
          <div class="animate-pulse flex flex-col lg:flex-row gap-12 mt-10">
            <div class="lg:w-7/12 bg-white h-[500px] rounded-[2.5rem] border border-slate-100"></div>
            <div class="lg:w-5/12 space-y-8 py-4">
              <div class="h-12 bg-slate-200 rounded-2xl w-3/4"></div>
              <div class="h-6 bg-slate-200 rounded-xl w-1/4"></div>
              <div class="h-32 bg-slate-200 rounded-3xl w-full"></div>
              <div class="h-16 bg-slate-200 rounded-2xl w-full"></div>
            </div>
          </div>
        }
      </div>
    </div>
  `
})
export class ProductDetailComponent {
  id = input<string>(); 
  private productService = inject(ProductService);
  private cartService = inject(CartService);
  private snackBar = inject(MatSnackBar);

  // Signal para controlar a quantidade selecionada localmente
  quantity = signal<number>(1);

  private id$ = toObservable(this.id);

  product = toSignal<Product>(
    this.id$.pipe(
      filter(id => !!id),
      switchMap(id => this.productService.getProductById(Number(id)))
    )
  );

  updateQuantity(val: number) {
    this.quantity.update(q => q + val);
  }

  addToCart(p: Product) {
    // Agora enviamos o produto e a quantidade selecionada
    // IMPORTANTE: Atualize seu CartService para aceitar um segundo parâmetro opcional 'quantity'
    this.cartService.addToCart(p, this.quantity());
    
    this.snackBar.open(`${this.quantity()}x ${p.nome} adicionado ao carrinho!`, 'Fechar', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['success-snackbar'] 
    });
    
    // Reseta para 1 após adicionar
    this.quantity.set(1);
  }
}