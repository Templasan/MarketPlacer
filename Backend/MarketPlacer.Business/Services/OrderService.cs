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

    public async Task<Order> CriarPedidoAsync(int clienteId, List<int> productIds, List<int> quantidades)
    {
        var order = new Order
        {
            ClienteId = clienteId,
            Status = "Pendente",
            DataPedido = DateTime.UtcNow,
            Itens = new List<OrderItem>()
        };

        for (int i = 0; i < productIds.Count; i++)
        {
            var pId = productIds[i];
            var qtd = quantidades[i];

            // Busca preço atualizado no banco
            var produto = await _productRepository.GetByIdAsync(pId);
            if (produto == null) throw new Exception($"Produto {pId} não encontrado.");

            order.Itens.Add(new OrderItem
            {
                ProductId = pId,
                Quantidade = qtd,
                PrecoUnitario = produto.Preco
            });
        }

        return await _orderRepository.CreateAsync(order);
    }

    public async Task<IEnumerable<Order>> ObterMeusPedidosAsync(int userId)
    {
        return await _orderRepository.GetByUserIdAsync(userId);
    }
}