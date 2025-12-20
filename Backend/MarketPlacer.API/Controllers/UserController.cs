using Microsoft.AspNetCore.Mvc;
using MarketPlacer.Business.Services;
using MarketPlacer.DAL.Models;
using MarketPlacer.API.Dtos;

namespace MarketPlacer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.ObterPorIdAsync(id);
            if (user == null) return NotFound("Usuário não encontrado.");

            return Ok(user);
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                await _userService.AtualizarUsuarioAsync(id, dto.Nome, dto.Email);
                return NoContent(); // 204: Deu certo e não precisa retornar nada
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/users/5
        // Nota: Isso fará um Soft Delete (apenas inativa)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userService.InativarUsuarioAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    // DTO (Modelo de Entrada) para não expor a senha ou campos sensíveis no Update
    public class UpdateUserDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}