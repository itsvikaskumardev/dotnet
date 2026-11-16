namespace EntityFrameworkCore.Models
{
    public class WmsOrderItem
    {
        public Guid Id { get; set; }

        public Guid WmsOrderId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        // Foreign Key Relationship
        public virtual WmsOrder WmsOrder { get; set; } = null!;

        // Navigation Property
        public virtual Product Product { get; set; } = null!;
    }
}
