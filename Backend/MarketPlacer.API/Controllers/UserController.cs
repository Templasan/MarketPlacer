using Microsoft.AspNetCore.Mvc;
using MarketPlacer.Business.Services;
using MarketPlacer.DAL.Models;
using MarketPlacer.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MarketPlacer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Garante que a pessoa está logada
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
            try
            {
                var userIdLogado = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // SEGURANÇA: Só permite ver o próprio perfil, a menos que seja ADMIN
                if (userRole != "Admin" && id != userIdLogado)
                    return Forbid();

                var user = await _userService.ObterPorIdAsync(id);
                if (user == null)
                    return NotFound(new { error = "Usuário não encontrado." });

                return Ok(new
                {
                    user.Id,
                    user.Nome,
                    user.Email,
                    user.Tipo,
                    user.Ativo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // NOVO MÉTODO: POST api/users/5/change-password
        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userIdLogado = GetCurrentUserId();

                // Segurança: Apenas o dono da conta pode trocar sua própria senha
                if (id != userIdLogado)
                    return Forbid();

                await _userService.AlterarSenhaAsync(id, dto.CurrentPassword, dto.NewPassword);

                return Ok(new { message = "Senha alterada com sucesso!" });
            }
            catch (Exception ex)
            {
                // Retorna a mensagem de erro vinda do Service (ex: "Senha atual incorreta")
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var userIdLogado = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Passa os parâmetros de segurança para o Service
                await _userService.AtualizarUsuarioAsync(id, dto.Nome, dto.Email, userIdLogado, userRole);

                return Ok(new { message = "Perfil atualizado com sucesso!" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/users/5 (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userIdLogado = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                await _userService.InativarUsuarioAsync(id, userIdLogado, userRole);

                return Ok(new { message = "Usuário desativado com sucesso." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // --- MÉTODOS AUXILIARES ---

        private int GetCurrentUserId()
        {
            // Tenta buscar pelo NameIdentifier padrão ou pela claim customizada 'id'
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id") ?? User.FindFirst("nameid");
            if (claim == null || !int.TryParse(claim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("ID do usuário não encontrado no token.");
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Comum";
        }
    }
}