using Microsoft.EntityFrameworkCore;
using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> SearchAsync(string? nome, string? categoria, decimal? minPreco, decimal? maxPreco)
    {
        // Começa com todos os produtos (mas não executa a query ainda)
        var query = _dbSet.AsQueryable();

        // Aplica filtros se eles foram passados
        if (!string.IsNullOrEmpty(nome))
            query = query.Where(p => p.Nome.Contains(nome));

        if (!string.IsNullOrEmpty(categoria))
            query = query.Where(p => p.Categoria.Contains(categoria));

        if (minPreco.HasValue)
            query = query.Where(p => p.Preco >= minPreco.Value);

        if (maxPreco.HasValue)
            query = query.Where(p => p.Preco <= maxPreco.Value);

        // Inclui quem é o vendedor (Join) para exibir o nome dele na tela
        query = query.Include(p => p.Vendedor);

        return await query.ToListAsync();
    }
}