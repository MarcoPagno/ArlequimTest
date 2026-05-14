namespace ArlequimTest.Api.Models
{
    public class StockEntry
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
