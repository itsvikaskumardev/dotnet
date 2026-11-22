using Chai_aur_Redis.Data;
using Chai_aur_Redis.SetupLocal.Models;
using Chai_aur_Redis.SetupLocal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Chai_aur_Redis.SetupLocal.Services
{
    public class DatabaseHealthService : IDatabaseHealthService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly AppDbContext _dbContext;

        public DatabaseHealthService(
            IConnectionMultiplexer redis,
            AppDbContext dbContext)
        {
            _redis = redis;
            _dbContext = dbContext;
        }

        public async Task<DatabaseHealthResponse> CheckRedisAsync()
        {
            var database = _redis.GetDatabase();
            var reply = await database.PingAsync();

            return new DatabaseHealthResponse
            {
                Status = "connected",
                Response = reply.ToString()
            };
        }

        public async Task<DatabaseHealthResponse> CheckPostgresAsync()
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();

            if (!canConnect)
            {
                return new DatabaseHealthResponse
                {
                    Status = "disconnected",
                    Database = _dbContext.Database.GetDbConnection().Database,
                    Response = "Unable to connect to PostgreSQL database."
                };
            }

            return new DatabaseHealthResponse
            {
                Status = "connected",
                Database = _dbContext.Database.GetDbConnection().Database,
                Response = "Successfully connected to PostgreSQL database."
            };
        }

        public async Task<(DatabaseHealthResponse Redis, DatabaseHealthResponse Postgres)> CheckAllAsync()
        {
            DatabaseHealthResponse redisResult;
            DatabaseHealthResponse postgresResult;

            try
            {
                redisResult = await CheckRedisAsync();
            }
            catch (Exception ex)
            {
                redisResult = new DatabaseHealthResponse
                {
                    Status = "disconnected",
                    Response = ex.Message
                };
            }

            try
            {
                postgresResult = await CheckPostgresAsync();
            }
            catch (Exception ex)
            {
                postgresResult = new DatabaseHealthResponse
                {
                    Status = "disconnected",
                    Response = ex.Message
                };
            }

            return (redisResult, postgresResult);
        }
    }
}
