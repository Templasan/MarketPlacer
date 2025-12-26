using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketPlacer.Business.Services;
using MarketPlacer.DAL.Models;

namespace MarketPlacer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _service;

    public OrdersController(OrderService service)
    {
        _service = service;
    }

    // ---------------------------------------------------------
    // ÁREA DO CLIENTE
    // ---------------------------------------------------------

    [HttpPost]
    public async Task<IActionResult> CriarPedido([FromBody] CreateOrderRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();

            if (request.Itens == null || !request.Itens.Any())
                return BadRequest(new { error = "O pedido não possui itens." });

            var ids = request.Itens.Select(x => x.ProductId).ToList();
            var qts = request.Itens.Select(x => x.Quantity).ToList();

            var order = await _service.CriarPedidoAsync(userId, ids, qts);

            // MAPEAMENTO PARA EVITAR CICLO: Retornamos um objeto limpo para o Angular
            return Ok(new OrderResponseDto(
                order.Id,
                order.Status,
                order.OrderDate,
                order.OrderItems.Select(i => new OrderItemResponseDto(
                    i.ProductId,
                    i.Quantity,
                    i.UnitPrice,
                    i.Product?.Nome ?? "Produto"
                )).ToList()
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/pagar")]
    public async Task<IActionResult> PagarPedido(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            await _service.SimularPagamentoAsync(id, userId, userRole);

            return Ok(new { message = "Pagamento confirmado! Estoque atualizado e pedido aprovado." });
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

    [HttpGet("meus-pedidos")]
    public async Task<IActionResult> GetMyOrders()
    {
        try
        {
            var userId = GetCurrentUserId();
            var pedidos = await _service.ObterMeusPedidosAsync(userId);

            // Mapeando a lista para evitar o ciclo de referência no GET também
            var response = pedidos.Select(o => new OrderResponseDto(
                o.Id,
                o.Status,
                o.OrderDate,
                o.OrderItems.Select(i => new OrderItemResponseDto(
                    i.ProductId, i.Quantity, i.UnitPrice, i.Product?.Nome ?? ""
                )).ToList()
            ));

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // ---------------------------------------------------------
    // ÁREA DO VENDEDOR
    // ---------------------------------------------------------

    [HttpGet("vendas")]
    [Authorize(Roles = "Vendedor,Admin")]
    public async Task<IActionResult> GetMinhasVendas()
    {
        try
        {
            var userId = GetCurrentUserId();
            var vendas = await _service.ObterVendasDoVendedorAsync(userId);

            // Mapeando vendas para o DTO de resposta
            var response = vendas.Select(o => new OrderResponseDto(
                o.Id,
                o.Status,
                o.OrderDate,
                o.OrderItems.Select(i => new OrderItemResponseDto(
                    i.ProductId, i.Quantity, i.UnitPrice, i.Product?.Nome ?? ""
                )).ToList()
            ));

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Vendedor,Admin")]
    public async Task<IActionResult> AtualizarStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Status))
                return BadRequest(new { error = "O status deve ser informado." });

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            await _service.AtualizarStatusPedidoAsync(id, request.Status, userId, userRole);

            return Ok(new { message = $"Status do pedido #{id} atualizado para {request.Status}." });
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

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
        if (claim == null || !int.TryParse(claim.Value, out int userId))
            throw new UnauthorizedAccessException("Usuário não identificado.");
        return userId;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? "Comum";
    }
}

// ==========================================
// DTOs DE ENTRADA (REQUEST)
// ==========================================
public record CreateOrderRequest(List<OrderItemRequest> Itens);
public record OrderItemRequest(int ProductId, int Quantity);
public record UpdateStatusRequest(string Status);

// ==========================================
// DTOs DE SAÍDA (RESPONSE) - RESOLVE O ERRO DE CICLO
// ==========================================
public record OrderResponseDto(int Id, string Status, DateTime OrderDate, List<OrderItemResponseDto> OrderItems);
public record OrderItemResponseDto(int ProductId, int Quantity, decimal UnitPrice, string ProductName);