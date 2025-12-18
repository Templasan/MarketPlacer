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

    public async Task<User> RegisterAsync(User user)
    {
        // 1. Verifica se email já existe
        var existingUser = await _userRepository.GetUserByEmailAsync(user.Email);
        if (existingUser != null)
            throw new Exception("E-mail já cadastrado.");

        // 2. Hash da Senha (Simples com SHA256 para o MVP)
        user.Senha = HashPassword(user.Senha);

        // 3. Salva
        return await _userRepository.CreateAsync(user);
    }

    public async Task<string?> LoginAsync(string email, string senha)
    {
        // 1. Busca usuário
        var user = await _userRepository.GetUserByEmailAsync(email);
        if (user == null) return null; // Usuário não encontrado

        // 2. Verifica Senha
        if (user.Senha != HashPassword(senha)) return null; // Senha errada

        // 3. Gera Token JWT
        return GenerateJwtToken(user);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        // Pega a chave do appsettings ou usa uma default segura para dev
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "MinhaChaveSuperSecretaDeDesenvolvimento123456");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Nome),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Tipo) // "Vendedor" ou "Cliente"
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}