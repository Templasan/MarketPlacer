import { Component, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { toSignal, toObservable } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, switchMap, startWith, combineLatest } from 'rxjs';

// Models e Interfaces
import { Product } from '../../models/product.interface';

// Material Design
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSidenavModule } from '@angular/material/sidenav';

// Serviços
import { ProductService } from '../../services/product.service';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';

// Componentes
import { FilterDialogComponent } from '../../components/filter-dialog.component';
import { CartDrawerComponent } from '../../components/cart-drawer.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule, 
    MatIconModule, MatFormFieldModule, MatInputModule, MatDialogModule, 
    MatTooltipModule, MatSidenavModule, CartDrawerComponent
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  private router = inject(Router);
  private productService = inject(ProductService);
  private authService = inject(AuthService);
  private cartService = inject(CartService);
  private dialog = inject(MatDialog);

  // --- Estados ---
  isCartOpen = signal(false);
  cartCount = this.cartService.count;
  
  currentPage = signal(1);
  pageSize = 12;
  
  // Filtros ativos
  activeFilters = signal<{categoria?: string, min?: number, max?: number}>({});
  searchControl = new FormControl('', { nonNullable: true });

  // --- Stream Reativa Principal ---
  // Combinamos: Busca + Página + Filtros do Dialog
  private productsResource = toSignal(
    combineLatest({
      termo: this.searchControl.valueChanges.pipe(
        startWith(''), 
        debounceTime(400), 
        distinctUntilChanged()
      ),
      page: toObservable(this.currentPage),
      filters: toObservable(this.activeFilters)
    }).pipe(
      switchMap(({ termo, page, filters }) => 
        this.productService.searchProducts(
          termo,
          filters.categoria,
          filters.min,
          filters.max,
          page,
          this.pageSize
        )
      )
    )
  );

  // --- Computed para o Template ---
  products = computed(() => this.productsResource()?.items ?? []);
  totalItems = computed(() => this.productsResource()?.totalItems ?? 0);
  totalPages = computed(() => Math.ceil(this.totalItems() / this.pageSize));

  // --- Ações ---

  changePage(delta: number) {
    this.currentPage.update(p => p + delta);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  openFilters() {
    const dialogRef = this.dialog.open(FilterDialogComponent, {
      width: '400px',
      panelClass: 'custom-dialog-container',
      data: this.activeFilters() // Passa filtros atuais para o dialog
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Mapeia o retorno do dialog para o que o Service espera
        this.activeFilters.set({
          min: result.price?.min,
          max: result.price?.max,
          // Pega a chave da categoria que está como 'true'
          categoria: Object.keys(result.categories).find(key => result.categories[key])
        });
        this.currentPage.set(1); // Reseta para pág 1 ao filtrar
      }
    });
  }

  toggleCart() {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }
    this.isCartOpen.update(v => !v);
  }

  async addToCart(product: Product) {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }
    await this.cartService.addToCart(product, 1);
  }

  goToProduct(id: number) {
    this.router.navigate(['/product', id]);
  }

  navigateToProfile() {
    this.router.navigate(['/profile']);
  }
}