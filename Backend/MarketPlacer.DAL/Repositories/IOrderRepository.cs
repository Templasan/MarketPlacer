using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL.Repositories;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
}