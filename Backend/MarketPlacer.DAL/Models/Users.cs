using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; 

namespace MarketPlacer.DAL.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;

    [Required]
    public string Tipo { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public virtual Cart? Cart { get; set; }

    // Relacionamentos
    [JsonIgnore]
    public List<Product>? Products { get; set; }

    [JsonIgnore]
    public List<Order>? Orders { get; set; }

}