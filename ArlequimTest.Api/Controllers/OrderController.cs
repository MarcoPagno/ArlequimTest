using ArlequimTest.Api.DTOs;
using ArlequimTest.Api.Exceptions;
using ArlequimTest.Api.Models;
using ArlequimTest.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArlequimTest.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            var order = _orderService.Create(dto);
            return Created("", new
            {
                order.Id,
                order.CustomerDocument,
                order.SellerName,
                order.CreatedAt,
                items = order.Items.Select(i => new
                {
                    i.ProductName,
                    i.Quantity,
                    i.UnitPrice
                })
            });
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            var order = _orderService.List();
            return Ok(new List<Order>(order));
        }
        catch (AppException ex)
        {
            return StatusCode(ex.StatusCode, new { error = ex.Message });
        }
    }
}