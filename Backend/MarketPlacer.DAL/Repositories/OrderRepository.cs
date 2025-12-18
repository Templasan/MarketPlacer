using Microsoft.EntityFrameworkCore;
using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(o => o.Itens)       // Carrega os itens do pedido
            .ThenInclude(i => i.Product) // Carrega os dados do Produto (Nome, Img)
            .Where(o => o.ClienteId == userId)
            .OrderByDescending(o => o.DataPedido)
            .ToListAsync();
    }
}