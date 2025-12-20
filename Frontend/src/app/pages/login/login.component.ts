import { Component, signal, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
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
    CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule,
    MatIconModule, MatFormFieldModule, MatInputModule, MatDialogModule,
    MatTooltipModule, MatSidenavModule, MatButtonToggleModule, 
    MatSnackBarModule, CartDrawerComponent
  ],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  isRegistering = signal(false);

  authForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    senha: ['', [Validators.required, Validators.minLength(6)]],
    nome: [''], // Iniciamos sem validador fixo para não travar o login
    tipo: ['Comum']
  });

  toggleMode() {
    this.isRegistering.update(v => !v);
    this.authForm.reset({ tipo: 'Comum' });
  }

  async onSubmit() {
    const nomeControl = this.authForm.get('nome');

    // AJUSTE DE VALIDAÇÃO: Nome só é obrigatório no Registro
    if (this.isRegistering()) {
      nomeControl?.setValidators([Validators.required]);
    } else {
      nomeControl?.clearValidators(); // No login, ignoramos o nome
    }
    nomeControl?.updateValueAndValidity();

    // Se ainda for inválido após o ajuste, paramos aqui
    if (this.authForm.invalid) {
      this.snackBar.open('Preencha os campos corretamente.', 'Aviso', { duration: 3000 });
      return;
    }

    const { email, senha, nome, tipo } = this.authForm.getRawValue();

    try {
      if (this.isRegistering()) {
        await this.authService.register({
          nome,
          email,
          senha,
          tipo: String(tipo)
        });
        this.toggleMode();
        this.snackBar.open('Cadastro realizado! Faça seu login.', 'OK', { duration: 4000 });
      } else {
        // Envia apenas o que o DTO de Login espera
        await this.authService.login({
          email: email,
          senha: senha
        });
        this.router.navigate(['/']); 
      }
    } catch (error: any) {
      const errorMsg = error.error?.error || 'Erro na operação. Verifique os dados.';
      this.snackBar.open(errorMsg, 'Erro', { duration: 5000 });
    }
  }
}