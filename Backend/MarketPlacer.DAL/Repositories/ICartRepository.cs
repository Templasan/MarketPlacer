using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL.Repositories
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        // Método específico para trazer o carrinho com os itens JÁ ORDENADOS
        Task<Cart?> GetCartByUserIdAsync(int userId);

        // Método otimizado para limpar o carrinho sem ter que carregar tudo na memória
        Task ClearCartAsync(int cartId);
    }
}