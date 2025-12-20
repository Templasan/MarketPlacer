// Crie ou atualize este arquivo
export interface Order {
  id: number;
  data: Date;
  total: number;
  status: 'Pendente' | 'Enviado' | 'Entregue' | 'Cancelado';
  itensCount: number;
}

export interface User {
  id: number;
  nome: string;
  email: string;
  tipo: string;
}