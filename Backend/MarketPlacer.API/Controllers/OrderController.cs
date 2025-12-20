using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketPlacer.Business.Services;
using MarketPlacer.DAL.Models;

namespace MarketPlacer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Regra Geral: Tem que estar logado para qualquer coisa aqui
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

    // 1. Criar Pedido (Status nasce "Pendente", estoque NÃO baixa aqui)
    [HttpPost]
    public async Task<IActionResult> CriarPedido([FromBody] CreateOrderRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            // Converte o DTO para listas simples
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

    // 2. Simular Pagamento (AQUI o bicho pega: Baixa Estoque e muda status para "Pago")
    [HttpPost("{id}/pagar")]
    public async Task<IActionResult> PagarPedido(int id)
    {
        try
        {
            await _service.SimularPagamentoAsync(id);
            return Ok(new { message = "Pagamento confirmado! Estoque atualizado e pedido aprovado." });
        }
        catch (Exception ex)
        {
            // Retorna erro 400 se o estoque acabou entre o pedido e o pagamento
            return BadRequest(new { error = ex.Message });
        }
    }

    // 3. Histórico de Compras do Cliente
    [HttpGet("meus-pedidos")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var pedidos = await _service.ObterMeusPedidosAsync(userId);
        return Ok(pedidos);
    }

    // ---------------------------------------------------------
    // ÁREA DO VENDEDOR
    // ---------------------------------------------------------

    // 4. Painel de Vendas (Vendedor vê pedidos que têm produtos dele)
    [HttpGet("vendas")]
    [Authorize(Roles = "Vendedor")] // <--- Só Vendedor entra
    public async Task<IActionResult> GetMinhasVendas()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var vendas = await _service.ObterVendasDoVendedorAsync(userId);
        return Ok(vendas);
    }

    // 5. Atualizar Status (Vendedor marca como "Enviado", etc)
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Vendedor")]
    public async Task<IActionResult> AtualizarStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            await _service.AtualizarStatusPedidoAsync(id, request.Status);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

// --- DTOs (Objetos de Transferência de Dados) ---
public record CreateOrderRequest(List<OrderItemRequest> Itens);
public record OrderItemRequest(int ProductId, int Quantity);
public record UpdateStatusRequest(string Status); // Ex: "Enviado"