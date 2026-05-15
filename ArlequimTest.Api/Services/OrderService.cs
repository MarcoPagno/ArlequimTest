using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Exceptions;
using ArlequimTest.Api.Models;

namespace ArlequimTest.Api.Services;

public class OrderService
{
    private List<Order> _orders = new();
    private readonly ProductService _productService;
    private readonly StockService _stockService;

    public OrderService(ProductService productService, StockService stockService)
    {
        _productService = productService;
        _stockService = stockService;
    }

    public Order Create(CreateOrderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerDocument))
            throw new ValidationError("Customer document is required");

        if (string.IsNullOrWhiteSpace(dto.SellerName))
            throw new ValidationError("Seller name is required");

        if (dto.Items == null || !dto.Items.Any())
            throw new ValidationError("Order must have at least one item");

        // valida estoque de todos os itens antes de criar qualquer coisa
        foreach (var item in dto.Items)
        {
            var available = _stockService.GetAvailableStock(item.ProductName);
            if (item.Quantity > available)
                throw new ValidationError($"Insufficient stock for product '{item.ProductName}'. Available: {available}");
        }

        // tudo ok — cria o pedido
        var order = new Order
        {
            Id = _orders.Count + 1,
            CustomerDocument = dto.CustomerDocument,
            SellerName = dto.SellerName,
            CreatedAt = DateTime.UtcNow,
            Items = dto.Items.Select(item =>
            {
                var product = _productService.FindByName(item.ProductName);
                return new OrderItem
                {
                    Id = item.GetHashCode(),
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };
            }).ToList()
        };

        // dá baixa no estoque de cada item
        foreach (var item in dto.Items)
            _stockService.DeductStock(item.ProductName, item.Quantity);

        _orders.Add(order);
        return order;
    }

    public List<Order> List()
    {

        if (!_orders.Any())
            throw new NotFoundError("Order table empty");

        return _orders.ToList();
    }
}