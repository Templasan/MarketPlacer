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
                    ProductName = i.Product?.Nome ?? "Produto não encontrado", // Uso de .Nome (Português)
                    ProductImageUrl = i.Product?.ImagemURL ?? "",              // Uso de .ImagemURL (Português)
                    UnitPrice = i.Product?.Preco ?? 0,                         // Uso de .Preco (Português)
                    Quantity = i.Quantity,
                    SubTotal = (i.Product?.Preco ?? 0) * i.Quantity
                }).ToList()
            };

            cartDto.TotalValue = cartDto.Items.Sum(i => i.SubTotal);

            return cartDto;
        }

        public async Task AddItemToCartAsync(int userId, int productId, int quantity)
        {
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

        public async Task UpdateItemQuantityAsync(int userId, int productId, int newQuantity)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) return;

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

        public async Task RemoveItemAsync(int userId, int productId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null) return;

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                cart.Items.Remove(item);
                await _cartRepository.UpdateAsync(cart);
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart != null)
            {
                await _cartRepository.ClearCartAsync(cart.Id);
            }
        }

        // =================================================================
        // CHECKOUT (Cálculo baseado no UnitPrice e Preco)
        // =================================================================
        public async Task<int> CheckoutAsync(int userId)
        {
            // 1. Pega os dados do carrinho
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null || !cart.Items.Any())
                throw new Exception("O carrinho está vazio.");

            // 2. Cria o objeto Pedido SEM o campo TotalAmount
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pendente"
                // REMOVIDA A LINHA DO TotalAmount QUE GERAVA O ERRO CS0117
            };

            // 3. Migra CartItems para OrderItems
            // O valor total do pedido agora é a soma desses UnitPrice * Quantity
            order.OrderItems = cart.Items.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product?.Preco ?? 0, // Congela o preço aqui
                Order = order
            }).ToList();

            // 4. Salva o pedido
            await _orderRepository.CreateAsync(order);

            // 5. Limpa o carrinho
            await _cartRepository.ClearCartAsync(cart.Id);

            return order.Id;
        }
    }
}