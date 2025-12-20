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

    // --- LEITURA (Busca com filtros) ---
    public async Task<PagedResult<Product>> ListarProdutosAsync(
        string? nome,
        string? categoria,
        decimal? min,
        decimal? max,
        int page = 1,
        int pageSize = 10)
    {
        // 1. Tratamento de Paginação (Evita valores inválidos)
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50; // Trava de segurança para não pesar o banco

        // 2. Chama o Repositório
        return await _repository.SearchAsync(nome, categoria, min, max, page, pageSize);
    }

    // --- ESCRITA (Criação) ---
    public async Task<Product> CriarProdutoAsync(Product produto, int vendedorId)
    {
        // 1. Segurança: Força o ID do vendedor vindo do Token (Controller passa isso)
        produto.VendedorId = vendedorId;

        // 2. Regras de Negócio
        if (produto.Preco <= 0)
            throw new Exception("O preço do produto deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(produto.Nome))
            throw new Exception("O nome do produto é obrigatório.");

        // Garante que nasce ativo e sem data de exclusão (se tiver esse campo)
        produto.Ativo = true;

        // 3. Persistência
        // Nota: Geralmente GenericRepository usa 'AddAsync'. Se o seu for 'CreateAsync', ajuste aqui.
        return await _repository.CreateAsync(produto);
    }

    // --- LEITURA (Unitária) ---
    public async Task<Product?> ObterPorIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    // --- REMOÇÃO (Soft Delete) ---
    public async Task RemoverProdutoAsync(int id, int vendedorId)
    {
        // 1. Busca o produto
        var produto = await _repository.GetByIdAsync(id);

        if (produto == null)
            throw new Exception("Produto não encontrado.");

        // 2. Segurança: Verifica se quem está deletando é o DONO
        if (produto.VendedorId != vendedorId)
            throw new Exception("Acesso negado: Você não pode excluir um produto que não é seu.");

        // 3. Soft Delete (Exclusão Lógica)
        // Ao invés de deletar do banco, apenas marcamos como inativo.
        produto.Ativo = false;

        // Atualizamos o registro no banco
        await _repository.UpdateAsync(produto);
    }
}