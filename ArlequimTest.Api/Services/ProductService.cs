using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Models;
using ArlequimTest.Api.Exceptions;

namespace ArlequimTest.Api.Services;

public class ProductService
{
    private static List<Product> _products = new();

    public Product Create(CreateProductDto dto)
    {
        if (_products.Any(u => u.Name.ToLower() == dto.Name.ToLower()))
            throw new ValidationError("Product name already used");

        if (dto.Price < 0 || ((decimal.GetBits(dto.Price)[3] >> 16) & 0x7F) > 2)
            throw new ValidationError("Wrong price format");

        var product = new Product
        {
            Id = _products.Count + 1,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
        };

        _products.Add(product);
        return product;
    }

    public List<Product> List()
    {
        if (!_products.Any())
            throw new NotFoundError("Product table empty");

        return _products.ToList();
    }

    public Product FindByName(string ProductName)
    {
        var prod = _products.FirstOrDefault(a => a.Name.ToLower() == ProductName.ToLower());

        if (prod == null)
            throw new NotFoundError("Product not found");

        return prod;
    }

    public bool DeleteByName(string ProductName)
    {
        var prod = _products.FirstOrDefault(a => a.Name.ToLower() == ProductName.ToLower());

        if (prod == null)
            throw new NotFoundError("Product not found");

        return _products.Remove(prod);
    }

    public Product UpdateByName(string productName, UpdateProductDto dto)
    {
        var prod = _products.FirstOrDefault(a => a.Name.ToLower() == productName.ToLower());
        if (prod == null)
            throw new NotFoundError("Product not found");

        if (dto.Name != null)
        {
            if (_products.Any(u => u.Name.ToLower() == dto.Name.ToLower() && u.Id != prod.Id))
                throw new ValidationError("Product name already used");
            prod.Name = dto.Name;
        }

        if (dto.Description != null)
            prod.Description = dto.Description;

        if (dto.Price != null)
        {
            if (dto.Price < 0 || ((decimal.GetBits(dto.Price.Value)[3] >> 16) & 0x7F) > 2)
                throw new ValidationError("Wrong price format");
            prod.Price = dto.Price.Value;
        }

        return prod;
    }
}
