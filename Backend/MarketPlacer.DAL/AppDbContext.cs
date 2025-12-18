using MarketPlacer.DAL; // Para o AppDbContext
using MarketPlacer.DAL.Repositories; // Para os Repositórios
using Microsoft.EntityFrameworkCore; // Para o UseSqlServer
using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL;

public class AppDbContext : DbContext
{
    // O construtor recebe as opções (como a string de conexão) e passa para a base
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Suas Tabelas
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // Futuramente, você adicionará aqui:
    // public DbSet<Cart> Carts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração de Preço (Decimal)
        modelBuilder.Entity<Product>()
            .Property(p => p.Preco)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.PrecoUnitario)
            .HasColumnType("decimal(18,2)");

        // --- CORREÇÃO DO ERRO AQUI ---
        // Pedido -> Itens (Se apagar o pedido, apaga os itens. OK.)
        modelBuilder.Entity<Order>()
            .HasMany(o => o.Itens)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Produto -> Itens (Se apagar o produto, NÃO apaga os itens para não dar conflito)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict); // Mudamos de Cascade (padrão) para Restrict
    }
}