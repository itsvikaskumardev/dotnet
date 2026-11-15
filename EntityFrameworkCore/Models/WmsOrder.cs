using EntityFrameworkCore.Interfaces;

namespace EntityFrameworkCore.Models
{
    public class WmsOrder : IAuditableEntity
    {
        public Guid Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string? ModifiedBy { get; set; }
    }
}
