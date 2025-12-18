using Microsoft.AspNetCore.Mvc;
using MarketPlacer.Business.Services;
using MarketPlacer.DAL.Models;

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
    public async Task<IActionResult> Register(User user)
    {
        try
        {
            var createdUser = await _authService.RegisterAsync(user);
            return Ok(createdUser);
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