using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<PagedResult<Product>> SearchAsync(
        string? nome,
        string? categoria,
        decimal? minPreco,
        decimal? maxPreco,
        int page,
        int pageSize);
}