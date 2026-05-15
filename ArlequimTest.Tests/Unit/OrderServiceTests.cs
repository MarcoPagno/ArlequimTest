using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Exceptions;
using ArlequimTest.Api.Services;
using System.Threading.Tasks;

namespace ArlequimTest.Tests.Unit;

public class OrderServiceTests
{
    private readonly ProductService _productService;
    private readonly StockService _stockService;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _productService = new ProductService();
        _stockService = new StockService(_productService);
        _service = new OrderService(_productService, _stockService);
    }

    //Order Creation (POST) TESTS
    [Fact]
    public void Create_ShouldThrow_WhenProductNameNotFound()
    {
        var dto = new CreateOrderDto
        {
            CustomerDocument = "CustomerTest",
            SellerName = "SellerTest",
            Items = [new OrderItemDto
            {
                ProductName = "UnitOrderNameError",
                Quantity = 19,
            }]
        };

        var exception = Assert.Throws<NotFoundError>(() => _service.Create(dto));
        Assert.Equal("Product not found", exception.Message);
    }

    [Fact]
    public void Create_ShouldThrow_WhenQuantityIsNotValid()
    {
        var newProduct = new CreateProductDto
        {
            Name = "UnitStockQuantityError",
            Description = "Unit stock quantity error test Description",
            Price = 00.99m
        };
        _productService.Create(newProduct);

        var newStock = new CreateStockEntryDto
        {
            ProductName = "UnitStockQuantityError",
            Quantity = 100,
            InvoiceNumber = "2026000000141"
        };
        _stockService.AddStock(newStock);

        var dto = new CreateOrderDto
        {
            CustomerDocument = "CustomerDoc1",
            SellerName = "Seller1",
            Items = [new OrderItemDto
            {
                ProductName = "UnitStockQuantityError",
                Quantity = 101,
            }]
        };

        var exception2 = Assert.Throws<ValidationError>(() => _service.Create(dto));
        Assert.Equal($"Insufficient stock for product '{dto.Items[0].ProductName}'. Available: {newStock.Quantity}", exception2.Message);
    }

    [Fact]
    public void Create_ShouldThrow_WhenCustomerDocumentError()
    {
        var newProduct = new CreateProductDto
        {
            Name = "UnitOrderDocumentError",
            Description = "Unit document error test Description",
            Price = 00.99m
        };
        _productService.Create(newProduct);

        var newStock = new CreateStockEntryDto
        {
            ProductName = "UnitOrderDocumentError",
            Quantity = 20,
            InvoiceNumber = "2026000000142"
        };
        _stockService.AddStock(newStock);

        var dto = new CreateOrderDto
        {
            CustomerDocument = "",
            SellerName = "seller2",
            Items = [new OrderItemDto
            {
                ProductName = "UnitOrderDocumentError",
                Quantity = 19,
            }]
        };

        var exception = Assert.Throws<ValidationError>(() => _service.Create(dto));
        Assert.Equal("Customer document is required", exception.Message);

        var dto2 = new CreateOrderDto
        {
            CustomerDocument = "customerDoc2",
            SellerName = "",
            Items = [new OrderItemDto
            {
                ProductName = "UnitOrderDocumentError",
                Quantity = 19,
            }]
        };

        var exception2 = Assert.Throws<ValidationError>(() => _service.Create(dto2));
        Assert.Equal("Seller name is required", exception2.Message);
    }

    [Fact]
    public async Task Create_ShouldReturnOrder_WhenDataIsValid()
    {
        //Product 1
        var newProduct = new CreateProductDto
        {
            Name = "UnitOrderSuccess",
            Description = "Unit order test Description",
            Price = 00.99m
        };
        _productService.Create(newProduct);

        var newStock = new CreateStockEntryDto
        {
            ProductName = "UnitOrderSuccess",
            Quantity = 20,
            InvoiceNumber = "2026000000143"
        };
        _stockService.AddStock(newStock);


        //Product 2
        var newProduct2 = new CreateProductDto
        {
            Name = "UnitOrderSuccess2",
            Description = "Unit order test 2 Description",
            Price = 99.99m
        };
        _productService.Create(newProduct2);

        var newStock2 = new CreateStockEntryDto
        {
            ProductName = "UnitOrderSuccess2",
            Quantity = 9,
            InvoiceNumber = "2026000000144"
        };
        _stockService.AddStock(newStock2);

        var dto = new CreateOrderDto
        {
            CustomerDocument = "CustomerDoc03",
            SellerName = "Seller03",
            Items = [
                new OrderItemDto
                {
                    ProductName = "UnitOrderSuccess",
                    Quantity = 20,
                }, new OrderItemDto
                {
                    ProductName = "UnitOrderSuccess2",
                    Quantity = 4,
                }, new OrderItemDto
                {
                    ProductName = "UnitOrderSuccess2",
                    Quantity = 3,
                }
            ]
        };

        var result = _service.Create(dto);

        Assert.Equal("CustomerDoc03", result.CustomerDocument);
        Assert.Equal("Seller03", result.SellerName);
        Assert.Equal("UnitOrderSuccess", result.Items[0].ProductName);
        Assert.Equal(20, result.Items[0].Quantity);
        Assert.Equal("UnitOrderSuccess2", result.Items[1].ProductName);
        Assert.Equal(4, result.Items[1].Quantity);
        Assert.Equal("UnitOrderSuccess2", result.Items[2].ProductName);
        Assert.Equal(3, result.Items[2].Quantity);
    }

}

