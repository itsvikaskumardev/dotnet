namespace EntityFrameworkCore.Dtos
{
    public class WmsOrderItemResponseDto
    {
        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
