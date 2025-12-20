import { Routes } from '@angular/router';
import { authGuard } from './services/auth.guard'; // Ajustado para minúsculo

export const routes: Routes = [
  // 1. Rota Raiz (Home)
  { 
    path: '', 
    loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent),
    title: 'MarketPlacer - Início'
  },

  // 2. Redirecionamento de /home
  { 
    path: 'home', 
    redirectTo: '', 
    pathMatch: 'full' 
  },

  // 3. Detalhes do Produto (IMPORTANTE: Deve vir antes da rota coringa)
  { 
    path: 'product/:id', 
    loadComponent: () => import('./pages/product-detail/product-detail.component').then(m => m.ProductDetailComponent),
    title: 'MarketPlacer - Detalhes do Produto'
  },

  // 4. Login e Cadastro
  { 
    path: 'login', 
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent),
    title: 'MarketPlacer - Entrar'
  },

  // 5. Perfil (Protegido)
  { 
    path: 'profile', 
    loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [authGuard],
    title: 'MarketPlacer - Meu Perfil'
  },

  // 6. Rota "Coringa" (Sempre por último)
  {
    path: '**',
    redirectTo: ''
  }
];