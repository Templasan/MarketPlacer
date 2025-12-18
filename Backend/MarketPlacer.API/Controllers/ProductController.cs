using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketPlacer.Business.Services;
using MarketPlacer.DAL.Models;

namespace MarketPlacer.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly ProductService _service;

    public ProductController(ProductService service)
    {
        _service = service;
    }

    // GET: api/Product (Público)
    [HttpGet]
    public async Task<IActionResult> GetAll(string? search, string? category, decimal? minPrice, decimal? maxPrice)
    {
        var produtos = await _service.ListarProdutosAsync(search, category, minPrice, maxPrice);
        return Ok(produtos);
    }

    // GET: api/Product/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var produto = await _service.ObterPorIdAsync(id);
        if (produto == null) return NotFound();
        return Ok(produto);
    }

    // POST: api/Product (Protegido - Só Vendedor)
    [HttpPost]
    [Authorize(Roles = "Vendedor")]
    public async Task<IActionResult> Create(Product produto)
    {
        try
        {
            // Pega o ID do usuário que está dentro do Token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int vendedorId = int.Parse(userIdClaim);

            var created = await _service.CriarProdutoAsync(produto, vendedorId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}