using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Models;
using ArlequimTest.Api.Exceptions;

namespace ArlequimTest.Api.Services;

public class StockService
{
    private static List<StockEntry> _entries = new();
    private readonly ProductService _productService;

    public StockService(ProductService productService)
    {
        _productService = productService;
    }

    public StockEntry AddStock(CreateStockEntryDto dto)
    {
        //Try finding the Product and throwing errors
        _productService.FindByName(dto.ProductName); 

        if(_entries.Any(e => e.InvoiceNumber == dto.InvoiceNumber))
            throw new ValidationError("Invoice already in database");

        if (dto.Quantity <= 0)
            throw new ValidationError("Quantity must be greater than zero");

        if (string.IsNullOrWhiteSpace(dto.InvoiceNumber))
            throw new ValidationError("Invoice number is required");

        var entry = new StockEntry
        {
            Id = _entries.Count + 1,
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            InvoiceNumber = dto.InvoiceNumber,
            CreatedAt = DateTime.UtcNow
        };

        _entries.Add(entry);
        return entry;
    }

    public int GetAvailableStock(string productName)
    {
        _productService.FindByName(productName);
        return _entries
            .Where(e => e.ProductName.ToLower() == productName.ToLower())
            .Sum(e => e.Quantity);
    }
}
