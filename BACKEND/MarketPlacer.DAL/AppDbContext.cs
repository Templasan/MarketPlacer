using Microsoft.EntityFrameworkCore;
using MarketPlacer.DAL.Models;

namespace MarketPlacer.DAL;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Tabelas do Banco de Dados
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Configurações de Tipos Decimais (Precisão Monetária) ---

        modelBuilder.Entity<Product>()
            .Property(p => p.Preco) // Corrigido de Preco para Price
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice) // Corrigido de PrecoUnitario para UnitPrice
            .HasColumnType("decimal(18,2)");


        // --- Relacionamentos de Pedido (Order) ---

        // Order -> OrderItems (1 para N)
        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderItems) // Corrigido de Itens para OrderItems
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product -> OrderItems (Restrição para não apagar histórico de vendas)
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);


        // --- Relacionamentos de Carrinho (Cart) ---

        // User -> Cart (1 para 1)
        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithOne(u => u.Cart)
            .HasForeignKey<Cart>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cart -> CartItems (1 para N)
        modelBuilder.Entity<Cart>()
            .HasMany(c => c.Items)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product -> CartItems
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}