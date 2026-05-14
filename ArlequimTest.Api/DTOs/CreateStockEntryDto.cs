namespace ArlequimTest.Api.DTOs;

public class CreateStockEntryDto
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public string InvoiceNumber { get; set; }
}