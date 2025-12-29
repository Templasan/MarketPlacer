namespace MarketPlacer.Business.Dtos; // Ajustado para Business para o HomeService enxergar

public class HomeDataDto
{
    public List<CategorySectionDto> Sections { get; set; } = new();
    public DateTime CachedAt { get; set; }
}

public class CategorySectionDto
{
    public string CategoryName { get; set; } = string.Empty; // Valor padrão evita warning
    public List<ProductMinDto> Products { get; set; } = new();
}

public class ProductMinDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string ImagemURL { get; set; } = string.Empty;
}