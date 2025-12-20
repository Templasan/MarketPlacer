using MarketPlacer.Business.Dtos;
using MarketPlacer.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations; // Isso resolve [Required] e [Range]

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
            var userId = GetCurrentUserId();
            var cart = await _cartService.GetCartAsync(userId);
            return Ok(cart);
        }

        // POST: api/cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
        {
            var userId = GetCurrentUserId();
            await _cartService.AddItemToCartAsync(userId, dto.ProductId, dto.Quantity);

            // Retorna o carrinho atualizado para o front sincronizar a UI imediatamente
            var updatedCart = await _cartService.GetCartAsync(userId);
            return Ok(updatedCart);
        }

        // PUT: api/cart/items/{productId}
        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdateQuantity(int productId, [FromBody] int newQuantity)
        {
            var userId = GetCurrentUserId();
            await _cartService.UpdateItemQuantityAsync(userId, productId, newQuantity);
            return NoContent();
        }

        // DELETE: api/cart/items/{productId}
        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = GetCurrentUserId();
            await _cartService.RemoveItemAsync(userId, productId);
            return NoContent();
        }

        // DELETE: api/cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            await _cartService.ClearCartAsync(userId);
            return NoContent();
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
                    Message = "Pedido finalizado com sucesso! O carrinho foi limpo."
                });
            }
            catch (Exception ex)
            {
                // Captura erros como "Carrinho Vazio" lançados no Service
                return BadRequest(new { Error = ex.Message });
            }
        }

        // ==========================================
        // MÉTODO DE EXTRAÇÃO DO JWT
        // ==========================================
        private int GetCurrentUserId()
        {
            // Tenta pegar o ID do padrão NameIdentifier ou de uma claim customizada "id"
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");

            if (claim == null)
            {
                // Se cair aqui, o token foi validado mas não contém o ID do usuário
                throw new UnauthorizedAccessException("ID do usuário não encontrado no Token JWT.");
            }

            if (!int.TryParse(claim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("ID do usuário no Token é inválido.");
            }

            return userId;
        }
    }
}

public class AddCartItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 999, ErrorMessage = "A quantidade deve ser no mínimo 1")]
    public int Quantity { get; set; }
}