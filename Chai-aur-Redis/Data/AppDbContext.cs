using Microsoft.EntityFrameworkCore;

namespace Chai_aur_Redis.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Define DbSets for entities here as needed
}
