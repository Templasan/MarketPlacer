import { Routes } from '@angular/router';
import { authGuard } from './services/auth.guard';

export const routes: Routes = [
  // 1. Rota Raiz (Home)
  { 
    path: '', 
    loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent),
    title: 'MarketPlacer - Início'
  },

  // 2. Redirecionamento de /home para a raiz
  { 
    path: 'home', 
    redirectTo: '', 
    pathMatch: 'full' 
  },

  // 3. Detalhes do Produto
  { 
    path: 'product/:id', 
    loadComponent: () => import('./pages/product-detail/product-detail.component').then(m => m.ProductDetailComponent),
    title: 'MarketPlacer - Detalhes do Produto'
  },

  // 4. Checkout (Página de Finalização de Compra) - PROTEGIDA
  { 
    path: 'checkout', 
    loadComponent: () => import('./components/checkout.component').then(m => m.CheckoutComponent),
    canActivate: [authGuard],
    title: 'MarketPlacer - Finalizar Compra'
  },

  // 5. Login e Cadastro
  { 
    path: 'login', 
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent),
    title: 'MarketPlacer - Entrar'
  },

  // 6. Perfil do Cliente (Protegido)
  { 
    path: 'profile', 
    loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [authGuard],
    title: 'MarketPlacer - Meu Perfil'
  },

  // 7. Dashboard do Vendedor (Protegido)
  { 
    path: 'seller', 
    loadComponent: () => import('./pages/seller-dashboard/seller-dashboard.component').then(m => m.SellerDashboardComponent),
    canActivate: [authGuard],
    title: 'MarketPlacer - Painel do Vendedor'
  },

  // 8. Rota "Coringa" (Sempre por último)
  {
    path: '**',
    redirectTo: ''
  }
];