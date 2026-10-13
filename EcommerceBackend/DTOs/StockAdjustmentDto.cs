namespace EcommerceBackend.DTOs
{
    public class StockAdjustmentDto
    {
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
