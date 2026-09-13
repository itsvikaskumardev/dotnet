namespace EntityFrameworkCore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }

        public int UserId { get; set; }
        public string? UniqueOrderId { get; set; }
        public required virtual User User { get; set; }
    }
}
