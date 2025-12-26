using MarketPlacer.DAL.Models;
using MarketPlacer.DAL.Repositories;
using System.Security.Cryptography;
using System.Text;

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
        var user = await _userRepository.GetByIdAsync(id);
        if (user != null) user.Senha = string.Empty;
        return user;
    }



    public async Task AlterarSenhaAsync(int id, string senhaAtual, string novaSenha)
    {
        var usuario = await _userRepository.GetByIdAsync(id);
        if (usuario == null) throw new Exception("Usuário não encontrado.");

        // 1. Hasheia a senha enviada pelo usuário para comparar com o banco
        string senhaInformadaHasheada = GerarHash(senhaAtual.Trim());
        string senhaNoBanco = usuario.Senha.Trim();

        // DEBUG para conferir os hashes (remover em produção)
        Console.WriteLine($"DEBUG: Banco['{senhaNoBanco}'] vs Hasheada['{senhaInformadaHasheada}']");

        if (senhaNoBanco != senhaInformadaHasheada)
        {
            throw new Exception("A senha atual informada está incorreta.");
        }

        // 2. Hasheia a NOVA senha antes de salvar
        usuario.Senha = GerarHash(novaSenha.Trim());
        await _userRepository.UpdateAsync(usuario);
    }

    // Função auxiliar para gerar o Hash (deve ser a mesma usada no Cadastro/Login)
    private string GerarHash(string senha)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return Convert.ToBase64String(bytes);
        }
    }

    // 3. ATUALIZAR DADOS
    public async Task AtualizarUsuarioAsync(int id, string novoNome, string novoEmail, int usuarioLogadoId, string role)
        {
            if (role != "Admin" && id != usuarioLogadoId)
                throw new UnauthorizedAccessException("Sem permissão para alterar dados de outro usuário.");

            var usuario = await _userRepository.GetByIdAsync(id);
            if (usuario == null) throw new Exception("Usuário não encontrado.");

            usuario.Nome = novoNome?.Trim();
            usuario.Email = novoEmail?.Trim();

            await _userRepository.UpdateAsync(usuario);
        }

    // 4. SOFT DELETE
    public async Task InativarUsuarioAsync(int id, int usuarioLogadoId, string role)
    {
        if (role != "Admin" && id != usuarioLogadoId)
            throw new UnauthorizedAccessException("Ação não permitida.");

        var usuario = await _userRepository.GetByIdAsync(id);
        if (usuario == null) throw new Exception("Usuário não encontrado.");

        usuario.Ativo = false;
        await _userRepository.UpdateAsync(usuario);
    }
}