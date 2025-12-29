import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { Product } from '../models/product.interface';

// Interface para o retorno paginado que o C# envia
export interface PagedResult<T> {
  items: T[];
  totalItems: number;
  page: number;
  pageSize: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private http = inject(HttpClient);
  
  // URL centralizada vinda do environment
  private readonly apiUrl = `${environment.apiUrl}/product`;

  // --- LEITURA (HOME E BUSCA) ---

  /**
   * Busca produtos com suporte a filtros e paginação
   */
  searchProducts(
    nome?: string, 
    categoria?: string, 
    min?: number, 
    max?: number, 
    page: number = 1, 
    pageSize: number = 12
  ): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (nome) params = params.set('nome', nome);
    if (categoria) params = params.set('categoria', categoria);
    if (min) params = params.set('min', min.toString());
    if (max) params = params.set('max', max.toString());

    // GET api/product
    return this.http.get<PagedResult<Product>>(this.apiUrl, { params });
  }

  /**
   * Busca dados da Home (Geralmente produtos em destaque ou categorias)
   */
  getHomeData(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/home`);
  }

  /**
   * Busca detalhes de um produto específico
   */
  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  /**
   * Busca as categorias únicas existentes no banco
   */
  getCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/categories`);
  }

  // --- ESCRITA (VENDEDOR) ---

  /**
   * Cria um novo produto enviando FormData para suportar upload de imagem
   */
  async createProduct(produto: Partial<Product>, imagem: File): Promise<Product> {
    const formData = new FormData();
    
    // Mapeia as propriedades do objeto para o FormData
    Object.keys(produto).forEach(key => {
      const value = (produto as any)[key];
      if (value !== null && value !== undefined) {
        formData.append(key, value.toString());
      }
    });

    // Adiciona o arquivo binário da imagem
    formData.append('imagem', imagem);

    // Converte para Promise para manter o padrão async/await do seu componente
    return firstValueFrom(this.http.post<Product>(this.apiUrl, formData));
  }

  /**
   * Atualiza um produto existente (imagem opcional)
   */
  async updateProduct(id: number, produto: Partial<Product>, imagem?: File): Promise<any> {
    const formData = new FormData();
    
    Object.keys(produto).forEach(key => {
      const value = (produto as any)[key];
      if (value !== null && value !== undefined) {
        formData.append(key, value.toString());
      }
    });

    if (imagem) {
      formData.append('imagem', imagem);
    }

    return firstValueFrom(this.http.put(`${this.apiUrl}/${id}`, formData));
  }

  /**
   * Remove um produto (Inativação lógica no C#)
   */
  deleteProduct(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}