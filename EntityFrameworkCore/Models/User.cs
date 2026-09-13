namespace EntityFrameworkCore.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
            = new List<Order>();
    }
}
