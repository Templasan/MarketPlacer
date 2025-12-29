export interface Product {
  id: number;
  nome: string;
  descricao: string;
  preco: number;
  imagemURL: string;
  categoria: string;
  estoque: number;
  ativo: boolean;
  vendedorId: number;
}

// Para o endpoint /api/products/search
export interface PagedResult<T> {
  items: T[];
  totalItems: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// Para o endpoint /api/products/home
export interface HomeData {
  sections: CategorySection[];
  cachedAt: string;
}

export interface CategorySection {
  categoryName: string;
  products: ProductMin[];
}

export interface ProductMin {
  id: number;
  name: string;
  price: number;
  imageUrl: string;
}