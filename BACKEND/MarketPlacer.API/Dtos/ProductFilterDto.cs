namespace MarketPlacer.API.Dtos;

public class ProductFilterDto
{
    public string? Termo { get; set; }       // Busca por nome ou descrição
    public string? Categoria { get; set; }   // Filtro exato de categoria
    public decimal? PrecoMin { get; set; }   // Range inicial
    public decimal? PrecoMax { get; set; }   // Range final
}