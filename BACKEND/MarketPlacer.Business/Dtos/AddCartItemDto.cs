namespace MarketPlacer.Business.Dtos; // O ';' já define o escopo do arquivo todo

using System.ComponentModel.DataAnnotations;

public class AddCartItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 999, ErrorMessage = "A quantidade deve ser no mínimo 1")]
    public int Quantity { get; set; }
}