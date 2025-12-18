using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketPlacer.Business.Services;
using MarketPlacer.DAL.Models;

namespace MarketPlacer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Obrigatório estar logado
public class OrderController : ControllerBase
{
    private readonly OrderService _service;

    public OrderController(OrderService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CriarPedido([FromBody] CreateOrderRequest request)
    {
        try
        {
            // Pega ID do usuário do Token
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var ids = request.Itens.Select(x => x.ProductId).ToList();
            var qts = request.Itens.Select(x => x.Quantity).ToList();

            var order = await _service.CriarPedidoAsync(userId, ids, qts);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("meus-pedidos")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var pedidos = await _service.ObterMeusPedidosAsync(userId);
        return Ok(pedidos);
    }
}

// DTOs para simplificar
public record CreateOrderRequest(List<OrderItemRequest> Itens);
public record OrderItemRequest(int ProductId, int Quantity);