namespace MarketPlacer.Business.Dtos; // Removido o erro de sintaxe e ajustado o namespace

public class CartDto
{
    public int Id { get; set; }
    public decimal TotalValue { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
}

public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal { get; set; } // Removido o => para evitar problemas de serialização simples no DTO
}