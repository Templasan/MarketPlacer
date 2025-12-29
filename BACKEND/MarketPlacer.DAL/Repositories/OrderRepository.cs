using Microsoft.EntityFrameworkCore;
using MarketPlacer.DAL.Models;
using MarketPlacer.DAL; // Certifique-se que o namespace do AppDbContext está correto

namespace MarketPlacer.DAL.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    // 1. Visão do Cliente
    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(o => o.OrderItems) // Corrigido de OrderItens para OrderItems
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId) // Corrigido de ClienteId para UserId
            .OrderByDescending(o => o.OrderDate) // Corrigido de DataPedido para OrderDate
            .ToListAsync();
    }

    // 2. Visão do Sistema (Para processar Pagamento/Estoque)
    public async Task<Order?> GetOrderWithItemsAsync(int orderId)
    {
        return await _dbSet
            .Include(o => o.OrderItems) // Corrigido de OrderItens para OrderItems
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    // 3. Visão do Vendedor (Painel de Vendas)
    public async Task<IEnumerable<Order>> GetOrdersForSellerAsync(int sellerId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .Include(o => o.User) // Corrigido de Cliente para User
            .Where(o => o.OrderItems.Any(i => i.Product != null && i.Product.VendedorId == sellerId))
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    // 4. Atualizar Status
    public async Task UpdateStatusAsync(int orderId, string novoStatus)
    {
        var order = await _dbSet.FindAsync(orderId);
        if (order != null)
        {
            order.Status = novoStatus;
            await _context.SaveChangesAsync();
        }
    }
}