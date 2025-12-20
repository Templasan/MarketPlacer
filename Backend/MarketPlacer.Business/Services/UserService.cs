using MarketPlacer.DAL.Models;
using MarketPlacer.DAL.Repositories;

namespace MarketPlacer.Business.Services;

public class UserService
{
    private readonly IGenericRepository<User> _userRepository;

    public UserService(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    // 1. OBTER POR ID
    public async Task<User?> ObterPorIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    // 2. ATUALIZAR DADOS (Sem mexer na senha)
    public async Task AtualizarUsuarioAsync(int id, string novoNome, string novoEmail)
    {
        var usuario = await _userRepository.GetByIdAsync(id);

        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        if (!usuario.Ativo)
            throw new Exception("Não é possível alterar dados de um usuário inativo.");

        // Atualiza apenas campos permitidos
        usuario.Nome = novoNome;
        usuario.Email = novoEmail;

        await _userRepository.UpdateAsync(usuario);
    }

    // 3. SOFT DELETE (Inativar)
    public async Task InativarUsuarioAsync(int id)
    {
        var usuario = await _userRepository.GetByIdAsync(id);

        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        // Ao invés de deletar do banco, marcamos como inativo
        usuario.Ativo = false;

        await _userRepository.UpdateAsync(usuario);
    }
}