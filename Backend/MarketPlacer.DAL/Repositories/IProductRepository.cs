using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    // Método específico para buscar com filtros
    Task<IEnumerable<Product>> SearchAsync(string? nome, string? categoria, decimal? minPreco, decimal? maxPreco);
}