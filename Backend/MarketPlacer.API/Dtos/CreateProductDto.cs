namespace MarketPlacer.API.Dtos;

public class CreateProductDto
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string ImagemURL { get; set; } = string.Empty;
}