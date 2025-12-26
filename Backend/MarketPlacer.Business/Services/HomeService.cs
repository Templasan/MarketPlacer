using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using MarketPlacer.DAL.Models;
using MarketPlacer.DAL;
using MarketPlacer.Business.Dtos;

namespace MarketPlacer.Business.Services;

public class HomeService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private const string HomeCacheKey = "MarketPlace_Home_Data";

    public HomeService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<HomeDataDto> GetHomeDataAsync()
    {
        // 1. Tenta buscar do Cache
        if (_cache.TryGetValue(HomeCacheKey, out HomeDataDto? cachedHome))
        {
            return cachedHome!;
        }

        // 2. Configurações
        var categoriesToShow = new List<string> { "Eletrônicos", "Espadas", "Poções", "Armaduras" };
        int itemsPerCategory = 5;

        // 3. LÓGICA MELHORADA: Uma única consulta ao banco para todas as categorias
        // Buscamos todos os produtos ativos das categorias desejadas
        var allProducts = await _context.Products
            .AsNoTracking()
            .Where(p => p.Ativo && categoriesToShow.Contains(p.Categoria))
            .OrderByDescending(p => p.Id)
            .Select(p => new { p.Id, p.Nome, p.Preco, p.ImagemURL, p.Categoria })
            .ToListAsync();

        var homeData = new HomeDataDto { CachedAt = DateTime.Now };

        // 4. Monta as seções em memória (muito mais rápido que ir ao banco várias vezes)
        foreach (var catName in categoriesToShow)
        {
            var categoryProducts = allProducts
                .Where(p => p.Categoria == catName)
                .Take(itemsPerCategory)
                .Select(p => new ProductMinDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Preco = p.Preco,
                    ImagemURL = p.ImagemURL
                })
                .ToList();

            if (categoryProducts.Any())
            {
                homeData.Sections.Add(new CategorySectionDto
                {
                    CategoryName = catName,
                    Products = categoryProducts
                });
            }
        }

        // 5. Salva no Cache com expiração de 5 minutos
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)) // Expira obrigatoriamente em 5 min
            .SetPriority(CacheItemPriority.High);

        _cache.Set(HomeCacheKey, homeData, cacheOptions);

        return homeData;
    }

    // DICA: Método para limpar o cache quando um produto novo for criado (opcional)
    public void ClearHomeCache()
    {
        _cache.Remove(HomeCacheKey);
    }
}