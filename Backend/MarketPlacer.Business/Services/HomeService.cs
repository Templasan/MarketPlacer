using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using MarketPlacer.DAL.Models;
using MarketPlacer.DAL;
using MarketPlacer.Business.Dtos;
using MarketPlacer.DAL.Repositories;

namespace MarketPlacer.Business.Services; // Adicionado namespace

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
        if (_cache.TryGetValue(HomeCacheKey, out HomeDataDto? cachedHome)) // Adicionado nullability check
        {
            return cachedHome!;
        }

        // 2. Configurações (Categorias que existem no seu banco)
        var categoriesToShow = new List<string> { "Eletrônicos", "Espadas", "Poções", "Armaduras" };
        int itemsPerCategory = 5;

        var homeData = new HomeDataDto { CachedAt = DateTime.Now };

        // 3. Monta as seções
        foreach (var catName in categoriesToShow)
        {
            // CORREÇÃO AQUI: Comparação direta com p.Categoria (que é string)
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.Categoria == catName && p.Ativo)
                .OrderByDescending(p => p.Id)
                .Take(itemsPerCategory)
                .Select(p => new ProductMinDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Preco = p.Preco,
                    ImagemURL = p.ImagemURL
                })
                .ToListAsync();

            if (products.Any())
            {
                homeData.Sections.Add(new CategorySectionDto
                {
                    CategoryName = catName,
                    Products = products
                });
            }
        }

        // 4. Salva no Cache
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

        _cache.Set(HomeCacheKey, homeData, cacheOptions);

        return homeData;
    }
}