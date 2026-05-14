using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Exceptions;
using ArlequimTest.Api.Services;

namespace ArlequimTest.Tests.Unit;

public class StockServiceTests
{
    private readonly ProductService _productService;
    private readonly StockService _service;

    public StockServiceTests()
    {
        _productService = new ProductService();
        _service = new StockService(_productService);
    }

    //Stock Creation (POST) TESTS
    [Fact]
    public void Create_ShouldThrow_WhenProductNameNotFound()
    {
        var newProduct = new CreateProductDto
        {
            Name = "UnitStockProductNameErrorTest",
            Description = "Unit stock product name error test Description",
            Price = 00.99m
        };
        _productService.Create(newProduct);

        var dto = new CreateStockEntryDto
        {
            ProductName = "UnitWRONGStockProductNameErrorTest",
            Quantity = 20,
            InvoiceNumber = "2026000000123"
        };

        var exception = Assert.Throws<NotFoundError>(() => _service.AddStock(dto));
        Assert.Equal("Product not found", exception.Message);
    }

    [Fact]
    public void Create_ShouldThrow_WhenQuantityIsNotValid()
    {
        var newProduct = new CreateProductDto
        {
            Name = "UnitStockProductQuantityErrorTest",
            Description = "Unit stock product quantity error test Description",
            Price = 100.00m
        };
        _productService.Create(newProduct);

        var dto = new CreateStockEntryDto
        {
            ProductName = "UnitStockProductQuantityErrorTest",
            Quantity = 0,
            InvoiceNumber = "2026000000124"
        };

        var exception = Assert.Throws<ValidationError>(() => _service.AddStock(dto));
        Assert.Equal("Quantity must be greater than zero", exception.Message);

        var dto2 = new CreateStockEntryDto
        {
            ProductName = "UnitStockProductQuantityErrorTest",
            Quantity = -2,
            InvoiceNumber = "2026000000125"
        };

        var exception2 = Assert.Throws<ValidationError>(() => _service.AddStock(dto2));
        Assert.Equal("Quantity must be greater than zero", exception2.Message);
    }

    [Fact]
    public void Create_ShouldThrow_WhenInvoiceAlreadyInDatabase()
    {
        var newProduct = new CreateProductDto
        {
            Name = "UnitStockInvoiceErrorTest",
            Description = "Unit stock invoice error test Description",
            Price = 00.99m
        };
        _productService.Create(newProduct);

        var dto = new CreateStockEntryDto
        {
            ProductName = "UnitStockInvoiceErrorTest",
            Quantity = 20,
            InvoiceNumber = "2026000000123"
        };
        _service.AddStock(dto);
        var dto2 = new CreateStockEntryDto
        {
            ProductName = "UnitStockInvoiceErrorTest",
            Quantity = 450,
            InvoiceNumber = "2026000000123"
        };

        var exception = Assert.Throws<ValidationError>(() => _service.AddStock(dto2));

        Assert.Equal("Invoice already in database", exception.Message);
    }

    [Fact]
    public void Create_ShouldReturnStockEntry_WhenDataIsValid()
    {
        var newProduct = new CreateProductDto
        {
            Name = "UnitStockSuccessTest",
            Description = "Unit stock success test Description",
            Price = 100.00m
        };
        _productService.Create(newProduct);

        var dto = new CreateStockEntryDto
        {
            ProductName = "UnitStockSuccessTest",
            Quantity = 10,
            InvoiceNumber = "2026000000125"
        };

        var result = _service.AddStock(dto);

        Assert.Equal("UnitStockSuccessTest", result.ProductName);
        Assert.Equal(10, result.Quantity);
        Assert.Equal("2026000000125", result.InvoiceNumber);
    }

    //Stock Listing (GET) TESTS
    [Fact]
    public void GetStock_ShouldThrow_WhenProductNameNotFound()
    {
        var exception = Assert.Throws<NotFoundError>(() => _service.GetAvailableStock("UnitWRONGStockNameErrorTest"));
        Assert.Equal("Product not found", exception.Message);
    }

    [Fact]
    public void GetStock_ShouldReturn0_WhenStockEntryNotFound()
    {
        var newProduct = new CreateProductDto
        {
            Name = "UnitStockEntryEmptyTest",
            Description = "Unit stock entry empty Description",
            Price = 00.99m
        };
        _productService.Create(newProduct);

        var result = _service.GetAvailableStock("UnitStockEntryEmptyTest");

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetStock_ShouldReturnStockEntrySum_WhenDataIsValid()
    {
        var newProduct = new CreateProductDto
        {
            Name = "UnitStockSuccessTest2",
            Description = "Unit stock success test Description 2",
            Price = 100.00m
        };
        _productService.Create(newProduct);

        var dto = new CreateStockEntryDto
        {
            ProductName = "UnitStockSuccessTest2",
            Quantity = 30,
            InvoiceNumber = "2026000000126"
        };
        _service.AddStock(dto);

        var result = _service.GetAvailableStock(newProduct.Name);

        Assert.Equal(30, result);

        var dto2 = new CreateStockEntryDto
        {
            ProductName = "UnitStockSuccessTest2",
            Quantity = 20,
            InvoiceNumber = "2026000000127"
        };
        _service.AddStock(dto2);

        var result2 = _service.GetAvailableStock(newProduct.Name);

        Assert.Equal(50, result2);
    }

}

