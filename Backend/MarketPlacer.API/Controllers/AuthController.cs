using Microsoft.AspNetCore.Mvc;
using MarketPlacer.Business.Services;
using MarketPlacer.DAL.Models;
using MarketPlacer.API.Dtos;

namespace MarketPlacer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Convertemos o DTO para a Entidade User antes de enviar para o Service
            var user = new User
            {
                Nome = request.Nome,
                Email = request.Email,
                Senha = request.Senha, // No futuro, você fará o Hash aqui
                Tipo = request.Tipo,
                Ativo = true
            };

            var createdUser = await _authService.RegisterAsync(user);

            // Retornamos um objeto limpo para não expor a senha no retorno
            return Ok(new
            {
                Id = createdUser.Id,
                Nome = createdUser.Nome,
                Email = createdUser.Email
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Senha);

        if (token == null)
            return Unauthorized("Usuário ou senha inválidos.");

        return Ok(new { Token = token });
    }
}

// DTO simples para o Login (pode ficar no mesmo arquivo para agilizar)
public record LoginRequest(string Email, string Senha);