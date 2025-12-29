import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SellerService } from '../../services/seller.service';
import { animate, state, style, transition, trigger } from '@angular/animations'; // Importante para expansão

// Material Modules
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatMenuModule } from '@angular/material/menu';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';

import { ImgUrlPipe } from '../../pipes/img-url.pipe';
import { ProductFormComponent } from '../../components/product-form.component';

@Component({
  selector: 'app-seller-dashboard',
  standalone: true,
  animations: [
    trigger('detailExpand', [
      state('collapsed,void', style({height: '0px', minHeight: '0'})),
      state('expanded', style({height: '*'})),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ],
  imports: [
    CommonModule, MatCardModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatTooltipModule, MatTabsModule,
    MatTableModule, MatMenuModule, MatSnackBarModule, MatDialogModule, 
    MatDividerModule, ImgUrlPipe
  ],
  templateUrl: './seller-dashboard.component.html',
  styleUrl: './seller-dashboard.component.scss'
})
export class SellerDashboardComponent implements OnInit {
  private sellerService = inject(SellerService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private router = inject(Router);

  produtos = signal<any[]>([]);
  vendas = signal<any[]>([]);
  
  // Controle de expansão da tabela de vendas
  expandedElement: any | null = null;
  columnsVendas = ['id', 'data', 'total', 'status', 'expand'];

  ngOnInit() {
    this.carregarDados();
  }

  carregarDados() {
    this.sellerService.getMeusProdutos().subscribe({
      next: (res) => this.produtos.set(res || []),
      error: () => this.showSnackBar('Erro ao carregar estoque.')
    });

    this.sellerService.getMinhasVendas().subscribe({
      next: (res) => this.vendas.set(res || []),
      error: () => this.showSnackBar('Erro ao carregar vendas.')
    });
  }

  voltarHome() { this.router.navigate(['/']); }

  openProductModal(produto?: any) {
    const dialogRef = this.dialog.open(ProductFormComponent, {
      width: '600px',
      data: produto || null
    });
    dialogRef.afterClosed().subscribe(result => { if (result) this.carregarDados(); });
  }

  mudarStatus(id: number, novoStatus: string) {
    this.sellerService.atualizarStatusVenda(id, novoStatus).subscribe({
      next: () => {
        this.showSnackBar(`Pedido #${id} atualizado para ${novoStatus}`);
        this.carregarDados();
      },
      error: (err) => this.showSnackBar('Erro ao atualizar status.')
    });
  }

  confirmarExclusao(id: number) {
    if (confirm('Deseja realmente excluir este produto?')) {
      this.sellerService.excluirProduto(id).subscribe(() => {
        this.showSnackBar('Produto removido');
        this.carregarDados();
      });
    }
  }

  getStatusColor(status: string) {
    const colors: any = {
      'Pago': 'bg-emerald-50 text-emerald-600 border-emerald-100',
      'Enviado': 'bg-blue-50 text-blue-600 border-blue-100',
      'Entregue': 'bg-purple-50 text-purple-600 border-purple-100',
      'Cancelado': 'bg-rose-50 text-rose-600 border-rose-100'
    };
    return colors[status] || 'bg-slate-50 text-slate-500';
  }

  podeMudarPara(statusAtual: string, alvo: string): boolean {
    const fluxograma: Record<string, string[]> = {
      'Pago': ['Enviado', 'Cancelado'],
      'Enviado': ['Entregue'],
      'Pendente': ['Cancelado']
    };
    return fluxograma[statusAtual]?.includes(alvo) ?? false;
  }

  private showSnackBar(msg: string) {
    this.snackBar.open(msg, 'Fechar', { duration: 3000 });
  }
}