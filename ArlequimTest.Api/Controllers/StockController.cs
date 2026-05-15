using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Exceptions;
using ArlequimTest.Api.Models;
using ArlequimTest.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArlequimTest.Api.Controllers;

[ApiController]
[Route("api/stock")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly StockService _stockService;

    public StockController(StockService stockService)
    {
        _stockService = stockService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult AddStock([FromBody] CreateStockEntryDto dto)
    {
        try
        {
            var entry = _stockService.AddStock(dto);
            return Created("", new { entry.Id, entry.ProductName, entry.Quantity, entry.InvoiceNumber,entry.CreatedAt });
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

    [HttpGet("{productName}")]
    public IActionResult GetStock(string productName)
    {
        try
        {
            var total = _stockService.GetAvailableStock(productName);
            return Ok(new { productName, availableStock = total });
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

}
