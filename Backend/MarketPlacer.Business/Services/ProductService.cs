using MarketPlacer.DAL.Models;
using MarketPlacer.DAL.Repositories;
using MarketPlacer.Business;

namespace MarketPlacer.Business.Services;

public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    private void ValidarCategoria(string categoria)
    {
        if (!ProductSettings.CategoriasValidas.Contains(categoria))
        {
            throw new Exception($"Categoria '{categoria}' é inválida. Categorias aceitas: {string.Join(", ", ProductSettings.CategoriasValidas)}");
        }
    }

    // --- LEITURA GERAL (Vitrine) ---
    public async Task<PagedResult<Product>> ListarProdutosAsync(string? nome, string? categoria, decimal? min, decimal? max, int page = 1, int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        return await _repository.SearchAsync(nome, categoria, min, max, page, pageSize);
    }

    // --- LEITURA ESPECÍFICA (Dashboard do Vendedor) ---
    public async Task<IEnumerable<Product>> ListarProdutosPorVendedorAsync(int vendedorId)
    {
        // Filtra produtos ativos onde o VendedorId corresponde ao ID do usuário logado
        return await _repository.FindAsync(p => p.VendedorId == vendedorId && p.Ativo);
    }

    // --- CRIAÇÃO ---
    public async Task<Product> CriarProdutoAsync(Product produto, int vendedorId)
    {
        if (produto.Preco <= 0) throw new Exception("O preço deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(produto.Nome)) throw new Exception("O nome é obrigatório.");

        ValidarCategoria(produto.Categoria);

        produto.VendedorId = vendedorId;
        produto.Ativo = true;

        return await _repository.CreateAsync(produto);
    }

    // --- ATUALIZAÇÃO (Dono ou Admin) ---
    public async Task AtualizarProdutoAsync(int id, int usuarioLogadoId, string role, Product dadosAtualizados)
    {
        var produtoExistente = await _repository.GetByIdAsync(id);

        if (produtoExistente == null) throw new Exception("Produto não encontrado.");

        // Segurança: Somente dono ou Admin pode editar
        if (role != "Admin" && produtoExistente.VendedorId != usuarioLogadoId)
            throw new UnauthorizedAccessException("Você não tem permissão para editar este produto.");

        ValidarCategoria(dadosAtualizados.Categoria);

        produtoExistente.Nome = dadosAtualizados.Nome;
        produtoExistente.Descricao = dadosAtualizados.Descricao;
        produtoExistente.Preco = dadosAtualizados.Preco;
        produtoExistente.Estoque = dadosAtualizados.Estoque;
        produtoExistente.Categoria = dadosAtualizados.Categoria;

        if (!string.IsNullOrEmpty(dadosAtualizados.ImagemURL))
        {
            produtoExistente.ImagemURL = dadosAtualizados.ImagemURL;
        }

        await _repository.UpdateAsync(produtoExistente);
    }

    // --- REMOÇÃO (Soft Delete) ---
    public async Task RemoverProdutoAsync(int id, int usuarioLogadoId, string role)
    {
        var produto = await _repository.GetByIdAsync(id);

        if (produto == null) throw new Exception("Produto não encontrado.");

        if (role != "Admin" && produto.VendedorId != usuarioLogadoId)
            throw new UnauthorizedAccessException("Você não tem permissão para excluir este produto.");

        produto.Ativo = false;
        await _repository.UpdateAsync(produto);
    }

    public async Task<Product?> ObterPorIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}