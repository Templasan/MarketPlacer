import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSliderModule } from '@angular/material/slider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-filter-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatSliderModule,
    MatCheckboxModule,
    MatIconModule,
    FormsModule
  ],
  template: `
    <div class="flex flex-col h-full max-h-[85vh] overflow-hidden">
      
      <div class="flex justify-between items-center px-6 py-4 border-b border-slate-100 bg-white z-10">
        <h2 class="text-xl font-bold text-slate-800 m-0 tracking-tight">Filtrar Produtos</h2>
        <button mat-icon-button (click)="close()" class="!text-slate-400 hover:!text-slate-600 -mr-2">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div class="flex-1 overflow-y-auto px-6 py-4 custom-scrollbar">
        
        <div class="mb-8">
          <div class="flex justify-between items-center mb-4">
            <h3 class="text-sm font-bold text-slate-500 uppercase tracking-wider">Faixa de Preço</h3>
            <span class="text-xs font-semibold text-indigo-600 bg-indigo-50 px-2 py-1 rounded">
              R$ {{ priceMin }} - R$ {{ priceMax }}
            </span>
          </div>
          
          <div class="px-2">
            <mat-slider min="0" max="5000" step="50" showTickMarks discrete class="w-full !m-0">
              <input matSliderStartThumb [(ngModel)]="priceMin">
              <input matSliderEndThumb [(ngModel)]="priceMax">
            </mat-slider>
          </div>
        </div>

        <div>
          <h3 class="text-sm font-bold text-slate-500 uppercase tracking-wider mb-4">Categorias</h3>
          <div class="flex flex-col gap-3">
            <mat-checkbox [(ngModel)]="categories.eletronicos" class="text-slate-600" color="primary">Eletrônicos</mat-checkbox>
            <mat-checkbox [(ngModel)]="categories.moda" class="text-slate-600" color="primary">Moda & Acessórios</mat-checkbox>
            <mat-checkbox [(ngModel)]="categories.casa" class="text-slate-600" color="primary">Casa & Decoração</mat-checkbox>
            <mat-checkbox [(ngModel)]="categories.gamer" class="text-slate-600" color="primary">Gamer</mat-checkbox>
            <mat-checkbox [(ngModel)]="categories.esportes" class="text-slate-600" color="primary">Esportes</mat-checkbox>
          </div>
        </div>
      </div>

      <div class="p-4 border-t border-slate-100 bg-slate-50 flex items-center justify-between">
        
        <button 
          mat-button 
          (click)="resetFilters()" 
          class="!text-slate-500 hover:!bg-slate-200 hover:!text-slate-700"
          matTooltip="Resetar todos os filtros"
        >
          Limpar Filtros
        </button>

        <div class="flex gap-3">
          <button mat-stroked-button (click)="close()" class="!border-slate-300 !text-slate-600">
            Cancelar
          </button>
          <button mat-flat-button color="primary" (click)="apply()" class="!rounded-md">
            Aplicar
          </button>
        </div>
      </div>

    </div>
  `,
  styles: [`
    :host { display: block; }
    .custom-scrollbar::-webkit-scrollbar { width: 6px; }
    .custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
    .custom-scrollbar::-webkit-scrollbar-thumb { background-color: #cbd5e1; border-radius: 20px; }
  `]
})
export class FilterDialogComponent {
  readonly dialogRef = inject(MatDialogRef<FilterDialogComponent>);
  
  // Valores padrão
  readonly defaultMin = 0;
  readonly defaultMax = 5000;

  // Estado atual
  priceMin = 250;
  priceMax = 3800;

  categories = {
    eletronicos: false,
    moda: false,
    casa: false,
    gamer: false,
    esportes: false
  };

  resetFilters(): void {
    // Reseta Preços
    this.priceMin = this.defaultMin;
    this.priceMax = this.defaultMax;
    
    // Reseta Categorias
    Object.keys(this.categories).forEach(key => {
      (this.categories as any)[key] = false;
    });
  }

  close(): void {
    this.dialogRef.close();
  }

  apply(): void {
    this.dialogRef.close({ 
      price: { min: this.priceMin, max: this.priceMax },
      categories: this.categories
    });
  }
}