using MarketPlacer.Business.Dtos;
using MarketPlacer.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace MarketPlacer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Garante que apenas usuários com JWT válido acessem
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        // GET: api/cart
        [HttpGet]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                var cart = await _cartService.GetCartAsync(userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: api/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                await _cartService.AddItemToCartAsync(userId, dto.ProductId, dto.Quantity, userRole);

                // Retorna o carrinho atualizado para o front sincronizar a UI imediatamente
                var updatedCart = await _cartService.GetCartAsync(userId);
                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: api/cart/items/{productId}
        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdateQuantity(int productId, [FromBody] UpdateQuantityDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                await _cartService.UpdateItemQuantityAsync(userId, productId, dto.NewQuantity, userRole);

                var updatedCart = await _cartService.GetCartAsync(userId);
                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/cart/items/{productId}
        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                await _cartService.RemoveItemAsync(userId, productId, userRole);

                var updatedCart = await _cartService.GetCartAsync(userId);
                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                await _cartService.ClearCartAsync(userId, userRole);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: api/cart/checkout
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = GetCurrentUserId();
                var orderId = await _cartService.CheckoutAsync(userId);

                return Ok(new
                {
                    OrderId = orderId,
                    Message = "Pedido finalizado com sucesso! O estoque foi reservado."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ==========================================
        // MÉTODOS DE EXTRAÇÃO DO TOKEN
        // ==========================================

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
            if (claim == null || !int.TryParse(claim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Usuário não identificado.");
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Comum";
        }
    }

    // --- DTOs Necessários ---

    public class AddCartItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, 999, ErrorMessage = "A quantidade deve ser no mínimo 1")]
        public int Quantity { get; set; }
    }

    public record UpdateQuantityDto(int NewQuantity);
}