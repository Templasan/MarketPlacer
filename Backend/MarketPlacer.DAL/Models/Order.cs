using System.ComponentModel.DataAnnotations.Schema;

namespace MarketPlacer.DAL.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime DataPedido { get; set; } = DateTime.UtcNow;

    public string Status { get; set; } = "Pendente"; // Pendente, Enviado, Entregue

    public int ClienteId { get; set; }
    public User? Cliente { get; set; }

    public List<OrderItem> Itens { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantidade { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecoUnitario { get; set; }
}