using Microsoft.EntityFrameworkCore;
using MarketPlacer.DAL;
using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    // O construtor passa o context para a base (GenericRepository)
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<PagedResult<Product>> SearchAsync(
        string? nome,
        string? categoria,
        decimal? minPreco,
        decimal? maxPreco,
        int page,
        int pageSize)
    {
        // 1. Monta a Query (Lazy Loading)
        var query = _dbSet.AsQueryable();

        // --- Filtros ---
        if (!string.IsNullOrEmpty(nome))
            query = query.Where(p => p.Nome.Contains(nome));

        if (!string.IsNullOrEmpty(categoria))
            query = query.Where(p => p.Categoria == categoria);

        if (minPreco.HasValue)
            query = query.Where(p => p.Preco >= minPreco.Value);

        if (maxPreco.HasValue)
            query = query.Where(p => p.Preco <= maxPreco.Value);

        // 2. Conta o TOTAL (Antes da paginação)
        var totalCount = await query.CountAsync();

        // 3. Executa a Busca Paginada
        // IMPORTANTE: Adicionei o OrderBy aqui. Sem ele, o Skip/Take pode falhar no SQL Server.
        var items = await query
            .OrderBy(p => p.Nome) // Ordenação padrão alfabética
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 4. Retorna o resultado
        return new PagedResult<Product>
        {
            Items = items,           // Correto
            TotalItems = totalCount, // Mudamos de TotalCount para TotalItems
            Page = page,             // Mudamos de PageNumber para Page
            PageSize = pageSize      // Correto
        };
    }
}