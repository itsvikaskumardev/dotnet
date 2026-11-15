namespace EntityFrameworkCore.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
    }
}
