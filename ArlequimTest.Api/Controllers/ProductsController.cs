using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Exceptions;
using ArlequimTest.Api.Models;
using ArlequimTest.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArlequimTest.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Create([FromBody] CreateProductDto dto)
    {
        try
        {
            var product = _productService.Create(dto);
            return Created("", new { product.Id, product.Name, product.Description, product.Price });
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult List()
    {
        try
        {
            var product = _productService.List();
            return Ok(new List<Product>(product));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

    [HttpGet("{name}")]
    [AllowAnonymous]
    public IActionResult Find(string name)
    {
        try
        {
            var product = _productService.FindByName(name);
            return Ok(new { product.Id, product.Name, product.Description, product.Price });
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

    [HttpDelete("{name}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(string name)
    {
        try
        {
            _productService.DeleteByName(name);
            return NoContent();
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

    [HttpPatch("{name}")]
    [Authorize(Roles = "Admin")]
    public IActionResult Update(string name, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var product = _productService.UpdateByName(name, dto);
            return Ok(new { product.Id, product.Name, product.Description, product.Price });
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

}
