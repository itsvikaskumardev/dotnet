using Chai_aur_Redis.SetupLocal.Models;

namespace Chai_aur_Redis.SetupLocal.Services.Interfaces
{
    public interface IDatabaseHealthService
    {
        Task<DatabaseHealthResponse> CheckRedisAsync();

        Task<DatabaseHealthResponse> CheckPostgresAsync();

        Task<(DatabaseHealthResponse Redis, DatabaseHealthResponse Postgres)> CheckAllAsync();
    }
}
