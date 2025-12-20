using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL.Repositories;

// Certifique-se de que IGenericRepository existe, senão remova a herança e adicione CreateAsync/UpdateAsync manualmente aqui.
public interface IOrderRepository : IGenericRepository<Order>
{
    // Métodos específicos de Pedidos que não existem no Genérico:
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<Order?> GetOrderWithItemsAsync(int orderId);
    Task<IEnumerable<Order>> GetOrdersForSellerAsync(int sellerId);
    Task UpdateStatusAsync(int orderId, string novoStatus);
}