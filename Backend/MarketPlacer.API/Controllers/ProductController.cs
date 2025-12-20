using Microsoft.AspNetCore.Mvc;
using MarketPlacer.Business.Services;
using MarketPlacer.DAL.Models;
using MarketPlacer.DAL.Repositories;
using MarketPlacer.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // Necessário para acessar ClaimTypes

namespace MarketPlacer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly IProductRepository _productRepository;
        private readonly HomeService _homeService;

        public ProductsController(
            ProductService productService,
            IProductRepository productRepository,
            HomeService homeService)
        {
            _productService = productService;
            _productRepository = productRepository;
            _homeService = homeService;
        }

        // GET: api/products/home
        [HttpGet("home")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHome()
        {
            var data = await _homeService.GetHomeDataAsync();
            return Ok(data);
        }

        // GET: api/products/search
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResult<Product>>> Search(
            [FromQuery] string? termo,
            [FromQuery] string? categoria,
            [FromQuery] decimal? min,
            [FromQuery] decimal? max,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            var resultado = await _productRepository.SearchAsync(
                termo,
                categoria,
                min,
                max,
                page,
                size);

            return Ok(resultado);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var produto = await _productRepository.GetByIdAsync(id);

            if (produto == null)
                return NotFound();

            return Ok(produto);
        }

        // POST: api/products
        [HttpPost]
        [Authorize(Roles = "Vendedor")] // Apenas usuários com Role Vendedor
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            try
            {
                // EXTRAÇÃO SEGURA DO ID DO TOKEN
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("ID do usuário não encontrado no token.");

                int vendedorIdDoToken = int.Parse(userIdClaim.Value);

                var produto = new Product
                {
                    Nome = dto.Nome,
                    Descricao = dto.Descricao,
                    Preco = dto.Preco,
                    Estoque = dto.Estoque,
                    Categoria = dto.Categoria,
                    ImagemURL = dto.ImagemURL,
                    Ativo = true
                };

                // Passamos o ID vindo do TOKEN, ignorando qualquer tentativa de fraude no DTO
                var criado = await _productService.CriarProdutoAsync(produto, vendedorIdDoToken);

                return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        [Authorize] // Precisa estar logado para deletar
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Também pegamos o ID do token aqui para garantir que o vendedor 
                // só delete os PRÓPRIOS produtos (lógica deve estar no Service)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();

                int vendedorIdDoToken = int.Parse(userIdClaim.Value);

                await _productService.RemoverProdutoAsync(id, vendedorIdDoToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}