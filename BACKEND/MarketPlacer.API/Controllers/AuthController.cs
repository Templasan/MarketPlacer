using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Necessário para o [Authorize]
using MarketPlacer.Business.Services;    // Necessário para o AuthService e outros
using MarketPlacer.API.Dtos;             // Necessário para os DTOs (UpdateUserDto, etc)
using MarketPlacer.DAL.Models;           // Necessário para as Entidades (User, Product)


namespace MarketPlacer.API.Controllers
{
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
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Validação básica manual se necessário
                if (string.IsNullOrEmpty(request.Email)) return BadRequest(new { error = "E-mail é obrigatório" });

                var user = new User
                {
                    Nome = request.Nome,
                    Email = request.Email,
                    Senha = request.Senha, // Lembrete: Implementar BCrypt ou Argon2 futuramente
                    Tipo = request.Tipo,
                    Ativo = true
                };

                var createdUser = await _authService.RegisterAsync(user);

                return Ok(new
                {
                    Id = createdUser.Id,
                    Nome = createdUser.Nome,
                    Email = createdUser.Email,
                    Tipo = createdUser.Tipo
                });
            }
            catch (Exception ex)
            {
                // Retorno em formato JSON para o Angular (snackBar) ler corretamente
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.LoginAsync(request.Email, request.Senha);

            if (token == null)
                return Unauthorized(new { error = "Usuário ou senha inválidos." });

            return Ok(new { Token = token });
        }
    }

    // DTOs definidos claramente para evitar erro 400 de conversão
    public record RegisterRequest(string Nome, string Email, string Senha, string Tipo);
    public record LoginRequest(string Email, string Senha);
}
