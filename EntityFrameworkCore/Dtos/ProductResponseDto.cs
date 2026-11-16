namespace EntityFrameworkCore.Dtos
{
    public class ProductResponseDto
    {
        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public decimal CurrentPrice { get; set; }
    }
}
