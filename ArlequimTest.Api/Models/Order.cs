namespace ArlequimTest.Api.Models;

public class Order
{
    public int Id { get; set; }
    public string CustomerDocument { get; set; }
    public string SellerName { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
