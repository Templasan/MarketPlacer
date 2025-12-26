namespace MarketPlacer.API.Dtos;

public class HomeDataDto
{
    public List<CategorySectionDto> Sections { get; set; } = new();
    public DateTime CachedAt { get; set; }
}

public class CategorySectionDto
{
    public string CategoryName { get; set; }
    public List<ProductMinDto> Products { get; set; } = new();
}

public class ProductMinDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}