import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product, PagedResult } from '../models/product.interface';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private http = inject(HttpClient);
  // URL base do seu backend .NET
  private readonly API_URL = 'http://localhost:5235/api/products'; 

  /**
   * Realiza a busca paginada com suporte a múltiplos filtros.
   * Os parâmetros min e max agora permitem filtrar por faixa de preço.
   */
  searchProducts(
    termo?: string, 
    categoria?: string, 
    min?: number, 
    max?: number, 
    page: number = 1, 
    size: number = 12
  ): Observable<PagedResult<Product>> {
    
    // Inicializa os parâmetros base de paginação
    let params = new HttpParams()
      .set('page', page.toString())
      .set('size', size.toString());

    // Adiciona filtros opcionais apenas se tiverem valor
    if (termo) params = params.set('termo', termo);
    if (categoria) params = params.set('categoria', categoria);
    
    // Filtros de preço vindos do FilterDialog
    if (min !== undefined && min !== null) params = params.set('min', min.toString());
    if (max !== undefined && max !== null) params = params.set('max', max.toString());
    
    // Faz a chamada para a rota /search do Controller .NET
    return this.http.get<PagedResult<Product>>(`${this.API_URL}/search`, { params });
  }

  /**
   * Busca os detalhes de um único produto por ID.
   */
  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.API_URL}/${id}`);
  }
}