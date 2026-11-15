namespace EntityFrameworkCore.Interfaces
{
    public interface IAuditableEntity
    {
        DateTime CreatedDate { get; set; }

        string? CreatedBy { get; set; }

        DateTime? ModifiedDate { get; set; }

        string? ModifiedBy { get; set; }
    }
}
