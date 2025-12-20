using MarketPlacer.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketPlacer.DAL.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        // Passamos o context para a classe base (GenericRepository)
        public CartRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Cart?> GetCartByUserIdAsync(int userId)
        {
            // AQUI ACONTECE A MÁGICA DA ORDENAÇÃO
            return await _dbSet
                // Inclui os itens, garantindo a ordem pelo AddedAt
                .Include(c => c.Items.OrderBy(i => i.AddedAt))
                // Traz os dados do Produto (Nome, Preço, Imagem) para exibir no front
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task ClearCartAsync(int cartId)
        {
            // Acessamos direto a tabela de Items para deletar em lote
            var items = await _context.Set<CartItem>()
                .Where(x => x.CartId == cartId)
                .ToListAsync();

            if (items.Any())
            {
                _context.Set<CartItem>().RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }
    }
}