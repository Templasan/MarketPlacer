// 1. Verifique se o namespace é este (o mesmo do Product e User)
namespace MarketPlacer.DAL.Models;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // O erro na linha 9 acontecia aqui
    public virtual User? User { get; set; }

    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public virtual Cart? Cart { get; set; }

    public int ProductId { get; set; }

    public virtual Product? Product { get; set; }

    public int Quantity { get; set; }
    public DateTime AddedAt { get; set; }
}