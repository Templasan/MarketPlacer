using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MarketPlacer.DAL.Models;
using MarketPlacer.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MarketPlacer.Business.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    // =================================================================
    // CADASTRO DE USUÁRIO
    // =================================================================
    public async Task<User> RegisterAsync(User user)
    {
        // 1. Verifica se o e-mail já existe no banco
        var existingUser = await _userRepository.GetUserByEmailAsync(user.Email);
        if (existingUser != null)
            throw new Exception("Este e-mail já está sendo utilizado por outra conta.");

        // 2. Hash da Senha (Transforma texto puro em código seguro)
        user.Senha = HashPassword(user.Senha);

        // 3. Define valores padrão de segurança
        user.Ativo = true;

        // 4. Salva o usuário no banco de dados
        return await _userRepository.CreateAsync(user);
    }

    // =================================================================
    // LOGIN (AUTENTICAÇÃO)
    // =================================================================
    public async Task<string?> LoginAsync(string email, string senha)
    {
        // 1. Busca o usuário pelo e-mail
        var user = await _userRepository.GetUserByEmailAsync(email);

        // 2. VALIDAÇÃO DE MVP:
        // - Usuário existe?
        // - A senha bate?
        // - A conta está ativa? (Importante para o Soft Delete que você fez no UsersController)
        if (user == null || user.Senha != HashPassword(senha))
        {
            return null; // Retorna nulo para "não autorizado"
        }

        if (!user.Ativo)
        {
            // Lança exceção para o Controller capturar e avisar: "Conta desativada"
            throw new Exception("Esta conta foi desativada. Entre em contato com o suporte.");
        }

        // 3. Gera o Token com as Claims (incluindo a Role que o Angular vai usar)
        return GenerateJwtToken(user);
    }

    // =================================================================
    // MÉTODOS AUXILIARES (PRIVADOS)
    // =================================================================

    private string HashPassword(string password)
    {
        // Cria um hash SHA256 para não salvar a senha em texto puro
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        // Pega a chave do appsettings.json. 
        // IMPORTANTE: Para o algoritmo HS256, a chave deve ter no mínimo 32 caracteres.
        var keyString = _configuration["Jwt:Key"] ?? "Chave_Mestra_Super_Secreta_Fatec_2025_Minimo_32_Chars!";
        var key = Encoding.ASCII.GetBytes(keyString);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nome ?? ""), // Nome do usuário no Payload
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Tipo ?? "Comum") // Perfil do usuário (Vendedor/Comum)
            }),
            Expires = DateTime.UtcNow.AddHours(8), // Token vale por 8 horas
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}