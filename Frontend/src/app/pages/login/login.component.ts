import { Component, signal, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router'; // Adicionado RouterLink
import { CommonModule } from '@angular/common';

// Material Modules
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

// Serviços e Componentes
import { AuthService } from '../../services/auth.service';
import { CartDrawerComponent } from '../../components/cart-drawer.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    MatCardModule, 
    MatButtonModule,
    MatIconModule, 
    MatFormFieldModule, 
    MatInputModule, 
    MatDialogModule,
    MatTooltipModule, 
    MatSidenavModule, 
    MatButtonToggleModule, 
    MatSnackBarModule, 
    CartDrawerComponent,
    RouterLink // Necessário para o botão de voltar (routerLink="/")
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  // Estado para alternar entre Login e Cadastro
  isRegistering = signal(false);

  // Formulário reativo único para as duas operações
  authForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required, Validators.minLength(6)]],
    nome: [''], // Validador adicionado dinamicamente no onSubmit
    tipo: ['Comum']
  });

  // Alterna o modo da tela e limpa o formulário
  toggleMode() {
    this.isRegistering.update(v => !v);
    this.authForm.reset({ tipo: 'Comum' });
  }

  async onSubmit() {
    const nomeControl = this.authForm.get('nome');

    // Ajusta a validação: Nome é obrigatório apenas no Registro
    if (this.isRegistering()) {
      nomeControl?.setValidators([Validators.required, Validators.minLength(3)]);
    } else {
      nomeControl?.clearValidators();
    }
    nomeControl?.updateValueAndValidity();

    if (this.authForm.invalid) {
      this.snackBar.open('Por favor, preencha os campos corretamente.', 'Aviso', { 
        duration: 3000,
        horizontalPosition: 'right',
        verticalPosition: 'top'
      });
      return;
    }

    const { email, senha, nome, tipo } = this.authForm.getRawValue();

    try {
      if (this.isRegistering()) {
        // Fluxo de Cadastro
        await this.authService.register({
          nome,
          email,
          senha,
          tipo: String(tipo)
        });
        
        this.snackBar.open('Cadastro realizado com sucesso! Pode entrar.', 'OK', { duration: 4000 });
        this.toggleMode(); // Volta para o modo login
        
      } else {
        // Fluxo de Login
        await this.authService.login({
          email: email,
          senha: senha
        });
        
        this.snackBar.open('Bem-vindo ao MarketPlacer!', 'Fechar', { duration: 3000 });
        this.router.navigate(['/']); // Redireciona para a Home
      }
    } catch (error: any) {
      // Captura mensagens de erro do seu Backend C#
      const errorMsg = error.error?.message || error.error?.error || 'Erro na autenticação. Tente novamente.';
      this.snackBar.open(errorMsg, 'Erro', { 
        duration: 5000,
        panelClass: ['error-snackbar'] 
      });
    }
  }
}