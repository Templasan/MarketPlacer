using MarketPlacer.DAL.Models;
using MarketPlacer.DAL.Repositories;

namespace MarketPlacer.Business.Services;

public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Product>> ListarProdutosAsync(string? nome, string? categoria, decimal? min, decimal? max)
    {
        return await _repository.SearchAsync(nome, categoria, min, max);
    }

    public async Task<Product> CriarProdutoAsync(Product produto, int vendedorId)
    {
        // Força o ID do vendedor vindo do Token (segurança)
        produto.VendedorId = vendedorId;

        // Poderíamos validar regras aqui (ex: preço negativo)
        if (produto.Preco <= 0)
            throw new Exception("O preço deve ser maior que zero.");

        return await _repository.CreateAsync(produto);
    }

    public async Task<Product?> ObterPorIdAsync(int id) => await _repository.GetByIdAsync(id);
}