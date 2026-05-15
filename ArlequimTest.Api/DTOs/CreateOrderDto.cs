namespace ArlequimTest.Api.DTOs;

public class CreateOrderDto
{
    public string CustomerDocument { get; set; }
    public string SellerName { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

public class OrderItemDto
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
}