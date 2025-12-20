using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MarketPlacer.DAL.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    public string Nome { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Preco { get; set; }

    public string ImagemURL { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public int VendedorId { get; set; }

    [JsonIgnore]
    public User? Vendedor { get; set; }

    public int Estoque { get; set; } // Quantidade disponível

    public bool Ativo { get; set; } = true; // Novo campo para Soft Delete
}