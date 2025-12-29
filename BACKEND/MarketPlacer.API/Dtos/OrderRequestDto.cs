namespace MarketPlacer.Business.Dtos;

public class OrderRequestDto
{
    public List<int> ProductIds { get; set; } = new();
    public List<int> Quantities { get; set; } = new();
}