import { Component, Inject, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { ProductService } from '../services/product.service';
import { Product } from '../models/product.interface'

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule
  ],
  template: `
    <div class="p-2">
      <h2 mat-dialog-title class="!font-black !text-2xl !p-0 mb-4 flex items-center gap-2">
        <mat-icon class="text-amber-500 scale-125">{{ data ? 'edit' : 'add_circle' }}</mat-icon>
        {{ data ? 'Editar Produto' : 'Novo Produto' }}
      </h2>

      <mat-dialog-content class="!px-1">
        <form [formGroup]="form" class="flex flex-col gap-4">
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <mat-form-field appearance="outline" class="w-full">
              <mat-label>Nome do Produto</mat-label>
              <input matInput formControlName="nome" placeholder="Ex: Teclado Mecânico">
              <mat-error *ngIf="form.get('nome')?.hasError('required')">Nome é obrigatório</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="w-full">
              <mat-label>Categoria</mat-label>
              <mat-select formControlName="categoria">
                @for (cat of categorias(); track cat) {
                  <mat-option [value]="cat">{{cat}}</mat-option>
                }
              </mat-select>
            </mat-form-field>
          </div>

          <mat-form-field appearance="outline" class="w-full">
            <mat-label>Descrição Curta</mat-label>
            <textarea matInput formControlName="descricao" rows="2" placeholder="Detalhes do que você está vendendo..."></textarea>
          </mat-form-field>

          <div class="grid grid-cols-2 gap-4">
            <mat-form-field appearance="outline">
              <mat-label>Preço (R$)</mat-label>
              <input matInput type="number" formControlName="preco" prefix="R$">
              <mat-icon matPrefix class="mr-1 text-slate-400">payments</mat-icon>
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Estoque</mat-label>
              <input matInput type="number" formControlName="estoque">
              <mat-icon matPrefix class="mr-1 text-slate-400">inventory_2</mat-icon>
            </mat-form-field>
          </div>

          <div class="group relative flex flex-col items-center justify-center p-6 border-2 border-dashed 
                      border-slate-300 rounded-[2rem] bg-slate-50 hover:bg-slate-100 transition-all cursor-pointer"
               (click)="fileInput.click()">
            
            @if (imagePreview()) {
              <div class="relative">
                <img [src]="imagePreview()" class="w-40 h-40 object-cover rounded-2xl shadow-lg border-4 border-white">
                <div class="absolute -top-2 -right-2 bg-white rounded-full p-1 shadow-md text-amber-500">
                  <mat-icon>cached</mat-icon>
                </div>
              </div>
            } @else {
              <div class="flex flex-col items-center text-slate-400">
                <mat-icon class="!w-12 !h-12 text-[48px] mb-4">cloud_upload</mat-icon>
                <p class="font-bold">Clique para enviar a foto</p>
                <p class="text-xs text-slate-400">PNG, JPG ou WEBP</p>
              </div>
            }
            
            <input type="file" #fileInput class="hidden" (change)="onFileSelected($event)" accept="image/*">
          </div>
        </form>
      </mat-dialog-content>

      <mat-dialog-actions align="end" class="!px-0 !py-4 gap-3">
        <button mat-button mat-dialog-close class="!rounded-xl !font-bold">Cancelar</button>
        <button mat-flat-button color="primary" 
                [disabled]="form.invalid || isSubmitting()" 
                (click)="save()"
                class="!rounded-2xl !py-6 !px-8 !font-black !shadow-lg !shadow-indigo-200">
          {{ isSubmitting() ? 'Salvando...' : (data ? 'Salvar Alterações' : 'Publicar Produto') }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    :host { display: block; }
    mat-dialog-content { max-height: 80vh !important; }
    // Estilização customizada para os campos
    ::ng-deep .mat-mdc-form-field-subscript-wrapper { display: none; }
  `]
})
export class ProductFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private productService = inject(ProductService);
  private dialogRef = inject(MatDialogRef<ProductFormComponent>);
  
  constructor(@Inject(MAT_DIALOG_DATA) public data: Product | null) {}

  form!: FormGroup;
  categorias = signal<string[]>([]);
  selectedFile: File | null = null;
  imagePreview = signal<string | null>(null);
  isSubmitting = signal(false);

  ngOnInit() {
    this.buildForm();
    this.loadCategories();
    
    if (this.data) {
      this.form.patchValue(this.data);
      // Se já houver imagem no objeto vindo do C#, exibe no preview
      if (this.data.imagemURL) this.imagePreview.set(this.data.imagemURL);
    }
  }

  private buildForm() {
    this.form = this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(3)]],
      descricao: ['', [Validators.required]],
      preco: [0, [Validators.required, Validators.min(0.01)]],
      estoque: [1, [Validators.required, Validators.min(0)]],
      categoria: ['', [Validators.required]]
    });
  }

  private loadCategories() {
    this.productService.getCategories().subscribe(cats => this.categorias.set(cats));
  }

  onFileSelected(event: any) {
    const file = event.target.files[0] as File;
    if (file) {
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => this.imagePreview.set(reader.result as string);
      reader.readAsDataURL(file);
    }
  }

  async save() {
    if (this.form.invalid || this.isSubmitting()) return;
    
    // Validação extra para novo produto (imagem obrigatória)
    if (!this.data && !this.selectedFile) {
      alert("Selecione uma imagem para o novo produto.");
      return;
    }

    this.isSubmitting.set(true);
    try {
      const pData = this.form.value;

      if (this.data?.id) {
        await this.productService.updateProduct(this.data.id, pData, this.selectedFile || undefined);
      } else {
        await this.productService.createProduct(pData, this.selectedFile!);
      }

      this.dialogRef.close(true);
    } catch (error: any) {
      console.error("Erro ao salvar:", error);
      alert(error.error?.message || "Erro de conexão com o servidor.");
    } finally {
      this.isSubmitting.set(false);
    }
  }
}