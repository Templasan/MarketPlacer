using MarketPlacer.DAL.Models;
using MarketPlacer.DAL.Repositories;

namespace MarketPlacer.Business.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepo, IProductRepository productRepo)
    {
        _orderRepository = orderRepo;
        _productRepository = productRepo;
    }

    // 1. CRIAR PEDIDO (Create Order)
    public async Task<Order> CriarPedidoAsync(int userId, List<int> productIds, List<int> quantities)
    {
        var order = new Order
        {
            UserId = userId, // Corrigido de ClienteId
            Status = "Pendente",
            OrderDate = DateTime.UtcNow, // Corrigido de DataPedido
            OrderItems = new List<OrderItem>() // Corrigido de Itens
        };

        for (int i = 0; i < productIds.Count; i++)
        {
            var pId = productIds[i];
            var qtd = quantities[i];

            var produto = await _productRepository.GetByIdAsync(pId);

            if (produto == null)
                throw new Exception($"Produto ID {pId} não encontrado.");

            if (!produto.Ativo) // Corrigido de Ativo
                throw new Exception($"O produto '{produto.Nome}' não está mais disponível para venda."); // Corrigido .Nome para .Nome

            if (produto.Estoque < qtd) // Corrigido de Estoque
                throw new Exception($"Estoque insuficiente para '{produto.Nome}'. Disponível: {produto.Estoque}");

            order.OrderItems.Add(new OrderItem
            {
                ProductId = pId,
                Quantity = qtd, // Corrigido de Quantidade
                UnitPrice = produto.Preco // Corrigido de PrecoUnitario e produto.Preco
            });
        }

        return await _orderRepository.CreateAsync(order);
    }

    // 2. SIMULAR PAGAMENTO (Simulate Payment)
    public async Task SimularPagamentoAsync(int orderId)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);

        if (order == null) throw new Exception("Pedido não encontrado.");
        if (order.Status != "Pendente") throw new Exception("Este pedido já foi processado.");

        foreach (var item in order.OrderItems) // Corrigido de Itens
        {
            var produto = await _productRepository.GetByIdAsync(item.ProductId);

            if (produto == null) throw new Exception($"Produto do pedido não existe mais.");
            if (produto.Estoque < item.Quantity) throw new Exception($"Infelizmente o estoque de '{produto.Nome}' acabou.");

            produto.Estoque -= item.Quantity; // Corrigido Estoque e Quantidade
            await _productRepository.UpdateAsync(produto);
        }

        order.Status = "Pago";
        await _orderRepository.UpdateAsync(order);
    }

    // 3. MEUS PEDIDOS
    public async Task<IEnumerable<Order>> ObterMeusPedidosAsync(int userId)
    {
        return await _orderRepository.GetByUserIdAsync(userId);
    }

    // 4. PAINEL DO VENDEDOR
    public async Task<IEnumerable<Order>> ObterVendasDoVendedorAsync(int sellerId)
    {
        return await _orderRepository.GetOrdersForSellerAsync(sellerId);
    }

    // 5. ATUALIZAR STATUS
    public async Task AtualizarStatusPedidoAsync(int orderId, string status)
    {
        await _orderRepository.UpdateStatusAsync(orderId, status);
    }
}