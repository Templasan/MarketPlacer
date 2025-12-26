// src/app/models/user.model.ts

export interface OrderItem {
  productId: number;
  quantity: number;
  unitPrice: number;
  product?: {
    nome: string;
    vendedorId: number;
  };
}

export interface Order {
  id: number;
  userId: number;
  status: string;
  orderDate: string; 
  orderItems: OrderItem[]; 
}