using System.ComponentModel.DataAnnotations.Schema; // ESTA LINHA RESOLVE O ERRO
using System.Collections.Generic;
namespace MarketPlacer.DAL.Models;


public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow; // Era DataPedido

    public string Status { get; set; } = "Pendente";

    public int UserId { get; set; } // Era ClienteId (para bater com a classe User)
    public User? User { get; set; }  // Era Cliente

    public List<OrderItem> OrderItems { get; set; } = new(); // Era Itens
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    
    // ADICIONE ESTA LINHA ABAIXO:
    public virtual Order? Order { get; set; } 

    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; } 
}