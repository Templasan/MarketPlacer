using MarketPlacer.DAL.Models;
using MarketPlacer.DAL.Repositories;
using System.Text; // Necessário para formatação de texto

namespace MarketPlacer.Business.Services;

public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    // Define o nome do arquivo de log na pasta raiz da aplicação
    private readonly string _logPath = Path.Combine(Directory.GetCurrentDirectory(), "historico_pedidos.log");

    public OrderService(IOrderRepository orderRepo, IProductRepository productRepo)
    {
        _orderRepository = orderRepo;
        _productRepository = productRepo;
    }

    // =========================================================
    // MÉTODO AUXILIAR DE LOG (Escreve no TXT)
    // =========================================================
    private async Task RegistrarLogAsync(string mensagem)
    {
        try
        {
            var linhaLog = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {mensagem}{Environment.NewLine}";

            // AppendAllTextAsync cria o arquivo se não existir e adiciona ao final se existir
            await File.AppendAllTextAsync(_logPath, linhaLog, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            // Em produção, você usaria um ILogger aqui, mas como é log em arquivo simples, 
            // apenas ignoramos falhas de I/O para não travar a venda.
            Console.WriteLine($"Falha ao gravar log: {ex.Message}");
        }
    }

    // Validação de Segurança Centralizada
    private void ValidarAcesso(int usuarioLogadoId, int donoId, string role)
    {
        if (role != "Admin" && usuarioLogadoId != donoId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para acessar ou modificar este pedido.");
        }
    }

    // 1. CRIAR PEDIDO
    public async Task<Order> CriarPedidoAsync(int userId, List<int> productIds, List<int> quantities)
    {
        var order = new Order
        {
            UserId = userId,
            Status = "Pendente",
            OrderDate = DateTime.UtcNow,
            OrderItems = new List<OrderItem>()
        };

        decimal totalEstimado = 0;

        for (int i = 0; i < productIds.Count; i++)
        {
            var pId = productIds[i];
            var qtd = quantities[i];

            var produto = await _productRepository.GetByIdAsync(pId);

            if (produto == null || !produto.Ativo)
                throw new Exception($"Produto ID {pId} indisponível.");

            if (produto.Estoque < qtd)
                throw new Exception($"Estoque insuficiente para '{produto.Nome}'.");

            order.OrderItems.Add(new OrderItem
            {
                ProductId = pId,
                Quantity = qtd,
                UnitPrice = produto.Preco
            });

            totalEstimado += produto.Preco * qtd;
        }

        var pedidoCriado = await _orderRepository.CreateAsync(order);

        // LOG: Pedido criado
        await RegistrarLogAsync($"NOVO PEDIDO: ID #{pedidoCriado.Id} criado pelo Usuário {userId}. Valor Total: {totalEstimado:C}. Status: Pendente.");

        return pedidoCriado;
    }

    // 2. SIMULAR PAGAMENTO (Com Verificação de Posse)
    public async Task SimularPagamentoAsync(int orderId, int userId, string role)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);

        if (order == null) throw new Exception("Pedido não encontrado.");

        ValidarAcesso(userId, order.UserId, role);

        if (order.Status != "Pendente") throw new Exception("Este pedido não está pendente de pagamento.");

        decimal valorTotal = 0;

        foreach (var item in order.OrderItems)
        {
            var produto = await _productRepository.GetByIdAsync(item.ProductId);

            if (produto == null) throw new Exception($"Produto do pedido não existe mais.");
            if (produto.Estoque < item.Quantity) throw new Exception($"Infelizmente o estoque de '{produto.Nome}' acabou durante o pagamento.");

            produto.Estoque -= item.Quantity;
            valorTotal += item.UnitPrice * item.Quantity;
            await _productRepository.UpdateAsync(produto);
        }

        order.Status = "Pago";
        await _orderRepository.UpdateAsync(order);

        // LOG: Pagamento Confirmado
        await RegistrarLogAsync($"PAGAMENTO CONFIRMADO: Pedido #{orderId} pago pelo Usuário {userId}. Estoque atualizado. Valor: {valorTotal:C}.");
    }

    // 3. MEUS PEDIDOS (CLIENTE)
    public async Task<IEnumerable<Order>> ObterMeusPedidosAsync(int userId)
    {
        return await _orderRepository.GetByUserIdAsync(userId);
    }

    // 4. PAINEL DO VENDEDOR (VENDEDOR)
    public async Task<IEnumerable<Order>> ObterVendasDoVendedorAsync(int sellerId)
    {
        return await _orderRepository.GetOrdersForSellerAsync(sellerId);
    }

    // 5. ATUALIZAR STATUS (VENDEDOR OU ADMIN)
    public async Task AtualizarStatusPedidoAsync(int orderId, string novoStatus, int usuarioId, string role)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
        if (order == null) throw new Exception("Pedido não encontrado.");

        string statusAntigo = order.Status;

        // Lógica de Segurança
        if (role != "Admin")
        {
            if (role != "Vendedor") throw new UnauthorizedAccessException("Apenas vendedores podem atualizar o status de entrega.");

            var vendedorEhDonoDeAlgumItem = order.OrderItems.Any(i => i.Product?.VendedorId == usuarioId);

            if (!vendedorEhDonoDeAlgumItem)
                throw new UnauthorizedAccessException("Você não tem permissão para alterar este pedido pois ele não contém produtos seus.");
        }

        order.Status = novoStatus;
        await _orderRepository.UpdateAsync(order);

        // LOG: Alteração de Status
        await RegistrarLogAsync($"STATUS ALTERADO: Pedido #{orderId} mudou de '{statusAntigo}' para '{novoStatus}'. Atualizado por Usuário {usuarioId} ({role}).");
    }
}