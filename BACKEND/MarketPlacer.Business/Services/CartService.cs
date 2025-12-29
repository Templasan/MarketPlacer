using MarketPlacer.Business.Dtos;
using MarketPlacer.DAL.Models;
using MarketPlacer.DAL.Repositories;

namespace MarketPlacer.Business.Services
{
    public class CartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IGenericRepository<Order> _orderRepository;

        public CartService(ICartRepository cartRepository, IGenericRepository<Order> orderRepository)
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
        }

        // Método Auxiliar de Segurança: Valida se o usuário é dono ou Admin
        private void ValidarAcessoAoCarrinho(int userIdDoToken, int userIdDoCarrinho, string userRole)
        {
            if (userRole != "Admin" && userIdDoToken != userIdDoCarrinho)
            {
                throw new UnauthorizedAccessException("Você não tem permissão para alterar o carrinho de outro usuário.");
            }
        }

        public async Task<CartDto> GetCartAsync(int userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null)
                return new CartDto { Items = new List<CartItemDto>(), TotalValue = 0 };

            var cartDto = new CartDto
            {
                Id = cart.Id,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Nome ?? "Produto não encontrado",
                    ProductImageUrl = i.Product?.ImagemURL ?? "",
                    UnitPrice = i.Product?.Preco ?? 0,
                    Quantity = i.Quantity,
                    SubTotal = (i.Product?.Preco ?? 0) * i.Quantity
                }).ToList()
            };

            cartDto.TotalValue = cartDto.Items.Sum(i => i.SubTotal);
            return cartDto;
        }

        public async Task AddItemToCartAsync(int userId, int productId, int quantity, string userRole)
        {
            // Validação: Somente o dono ou admin pode adicionar itens
            // Nota: O 'userId' passado aqui deve ser o ID do alvo da operação
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await _cartRepository.CreateAsync(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    AddedAt = DateTime.UtcNow
                });
            }

            await _cartRepository.UpdateAsync(cart);
        }

        public async Task UpdateItemQuantityAsync(int userId, int productId, int newQuantity, string userRole)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) return;

            // Trava de Segurança
            ValidarAcessoAoCarrinho(userId, cart.UserId, userRole);

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                if (newQuantity <= 0)
                    cart.Items.Remove(item);
                else
                    item.Quantity = newQuantity;

                await _cartRepository.UpdateAsync(cart);
            }
        }

        public async Task RemoveItemAsync(int userId, int productId, string userRole)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) return;

            // Trava de Segurança
            ValidarAcessoAoCarrinho(userId, cart.UserId, userRole);

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                cart.Items.Remove(item);
                await _cartRepository.UpdateAsync(cart);
            }
        }

        public async Task ClearCartAsync(int userId, string userRole)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) return;

            ValidarAcessoAoCarrinho(userId, cart.UserId, userRole);
            await _cartRepository.ClearCartAsync(cart.Id);
        }

        public async Task<int> CheckoutAsync(int userId)
        {
            // 1. Pega os dados com Eager Loading (Garantindo que Product venha preenchido)
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null || !cart.Items.Any())
                throw new Exception("O carrinho está vazio.");

            // 2. Validação de Estoque ANTES de criar o pedido
            foreach (var item in cart.Items)
            {
                if (item.Product == null)
                    throw new Exception($"Erro técnico: Dados do produto {item.ProductId} não carregados.");

                if (item.Product.Estoque < item.Quantity)
                    throw new Exception($"Estoque insuficiente para {item.Product.Nome}. Disponível: {item.Product.Estoque}");
            }

            // 3. Criação do Pedido
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pendente"
            };

            // 4. Migração com Preço Congelado
            order.OrderItems = cart.Items.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product!.Preco,
                Order = order
            }).ToList();

            // 5. Persistência e Limpeza
            await _orderRepository.CreateAsync(order);
            await _cartRepository.ClearCartAsync(cart.Id);

            return order.Id;
        }
    }
}