using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MarketPlacer.Business.Services;
using MarketPlacer.DAL.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace MarketPlacer.API.Controllers
{
    // DTO para simplificar a entrada de dados e limpar o Swagger
    public class ProductDTO
    {
        [Required]
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        [Required]
        public decimal Preco { get; set; }
        [Required]
        public string Categoria { get; set; } = string.Empty;
        public int Estoque { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly HomeService _homeService;

        public ProductController(ProductService productService, HomeService homeService)
        {
            _productService = productService;
            _homeService = homeService;
        }

        [HttpGet("home")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHome()
        {
            try
            {
                var vitrine = await _homeService.GetHomeDataAsync();
                return Ok(vitrine);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Erro ao carregar vitrine da home." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] string? nome, [FromQuery] string? categoria, [FromQuery] decimal? min, [FromQuery] decimal? max, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _productService.ListarProdutosAsync(nome, categoria, min, max, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            var produto = await _productService.ObterPorIdAsync(id);
            if (produto == null) return NotFound(new { error = "Produto não encontrado." });
            return Ok(produto);
        }

        // POST: Agora usando o DTO para ficar limpo no Swagger
        [HttpPost]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Criar([FromForm] ProductDTO dto, IFormFile? imagem)
        {
            try
            {
                var vendedorId = GetCurrentUserId();

                // 1. Criamos o objeto com os dados do DTO
                var produto = new Product
                {
                    Nome = dto.Nome,
                    Descricao = dto.Descricao,
                    Preco = dto.Preco,
                    Categoria = dto.Categoria,
                    Estoque = dto.Estoque,
                    VendedorId = vendedorId, // Definimos aqui para garantir
                    Ativo = true,
                    ImagemURL = "" // Valor inicial
                };

                // 2. Se a imagem veio, salvamos no disco e guardamos o caminho no objeto
                if (imagem != null && imagem.Length > 0)
                {
                    var caminhoImagem = await SalvarImagemLocal(imagem);
                    produto.ImagemURL = caminhoImagem;
                }

                // 3. PASSAMOS O OBJETO JÁ COM A IMAGEMURL PARA O SERVICE
                var novoProduto = await _productService.CriarProdutoAsync(produto, vendedorId);

                _homeService.ClearHomeCache();

                // O retorno agora deve mostrar a ImagemURL preenchida
                return CreatedAtAction(nameof(ObterPorId), new { id = novoProduto.Id }, novoProduto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: Também usando DTO
        [HttpPut("{id}")]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Atualizar(int id, [FromForm] ProductDTO dto, IFormFile? imagem)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = GetCurrentUserRole();

                // 1. Buscamos o produto atual para saber qual a imagem antiga
                var produtoExistente = await _productService.ObterPorIdAsync(id);
                if (produtoExistente == null) return NotFound(new { error = "Produto não encontrado." });

                // 2. Mapeamos as alterações do DTO
                var produtoEditado = new Product
                {
                    Id = id,
                    Nome = dto.Nome,
                    Descricao = dto.Descricao,
                    Preco = dto.Preco,
                    Categoria = dto.Categoria,
                    Estoque = dto.Estoque,
                    // Mantemos a imagem antiga como padrão
                    ImagemURL = produtoExistente.ImagemURL
                };

                // 3. Se uma NOVA imagem foi enviada, substituímos a URL
                if (imagem != null && imagem.Length > 0)
                {
                    // Salva a nova imagem e sobrescreve a URL no objeto
                    produtoEditado.ImagemURL = await SalvarImagemLocal(imagem);

                    // Opcional: Aqui você poderia deletar o arquivo antigo do disco para não ocupar espaço
                }

                // 4. Enviamos para o Service processar a atualização
                await _productService.AtualizarProdutoAsync(id, userId, role, produtoEditado);

                _homeService.ClearHomeCache();

                return Ok(new
                {
                    message = "Produto atualizado com sucesso!",
                    url = produtoEditado.ImagemURL
                });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> Remover(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var role = GetCurrentUserRole();

                await _productService.RemoverProdutoAsync(id, userId, role);
                _homeService.ClearHomeCache();

                return Ok(new { message = "Produto removido com sucesso!" });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public IActionResult GetCategories()
        {
            return Ok(MarketPlacer.Business.ProductSettings.CategoriasValidas);
        }

        // --- MÉTODOS AUXILIARES ---

        private async Task<string> SalvarImagemLocal(IFormFile file)
        {
            var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extensao = Path.GetExtension(file.FileName).ToLower();

            if (!extensoesPermitidas.Contains(extensao))
                throw new Exception("Formato de imagem inválido. Use JPG, PNG ou WEBP.");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{extensao}";
            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{fileName}";
        }

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

        // No ProductController.cs
        [HttpGet("meus-produtos")]
        [Authorize(Roles = "Vendedor,Admin")]
        public async Task<IActionResult> ListarMeusProdutos()
        {
            var vendedorId = GetCurrentUserId(); // Extrai do token
                                                 // Filtra no banco apenas onde VendedorId == vendedorId
            var produtos = await _productService.ListarProdutosPorVendedorAsync(vendedorId);
            return Ok(produtos);
        }
    }
}