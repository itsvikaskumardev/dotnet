namespace EntityFrameworkCore.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string SKU { get; set; } = string.Empty;

        public decimal Price { get; set; }

        // Navigation Property
        public virtual ICollection<WmsOrderItem> OrderItems { get; set; }
            = new List<WmsOrderItem>();
    }
}
