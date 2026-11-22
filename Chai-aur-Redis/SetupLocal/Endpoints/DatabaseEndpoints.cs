using Chai_aur_Redis.SetupLocal.Services.Interfaces;

namespace Chai_aur_Redis.SetupLocal.Endpoints
{
    public static class DatabaseEndpoints
    {
        public static IEndpointRouteBuilder MapDatabaseEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/database")
                              .WithTags("Database Health");

            // Check health of both Redis and PostgreSQL simultaneously
            group.MapGet("/health", CheckAllAsync)
                 .WithName("CheckAllDatabasesHealth")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            group.MapGet("/all", CheckAllAsync)
                 .ExcludeFromDescription();

            // Check Redis connection health
            group.MapGet("/redis", CheckRedisAsync)
                 .WithName("CheckRedisHealth")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Check PostgreSQL connection health
            group.MapGet("/postgres", CheckPostgresAsync)
                 .WithName("CheckPostgresHealth")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            // Alias for backward compatibility
            group.MapGet("/mongo", CheckPostgresAsync)
                 .ExcludeFromDescription();

            return routes;
        }

        private static async Task<IResult> CheckAllAsync(IDatabaseHealthService databaseHealthService)
        {
            var (redis, postgres) = await databaseHealthService.CheckAllAsync();

            var isHealthy = redis.Status == "connected" && postgres.Status == "connected";

            var payload = new
            {
                status = isHealthy ? "healthy" : "degraded",
                redis = new
                {
                    status = redis.Status,
                    ping = redis.Response
                },
                postgres = new
                {
                    status = postgres.Status,
                    database = postgres.Database,
                    message = postgres.Response
                }
            };

            return isHealthy
                ? Results.Ok(payload)
                : Results.Json(payload, statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        private static async Task<IResult> CheckRedisAsync(IDatabaseHealthService databaseHealthService)
        {
            try
            {
                var result = await databaseHealthService.CheckRedisAsync();

                return Results.Ok(new
                {
                    redis = result.Status,
                    ping = result.Response
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    redis = "disconnected",
                    error = ex.Message
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }

        private static async Task<IResult> CheckPostgresAsync(IDatabaseHealthService databaseHealthService)
        {
            try
            {
                var result = await databaseHealthService.CheckPostgresAsync();

                if (result.Status == "disconnected")
                {
                    return Results.Json(new
                    {
                        postgres = result.Status,
                        database = result.Database,
                        message = result.Response
                    }, statusCode: StatusCodes.Status503ServiceUnavailable);
                }

                return Results.Ok(new
                {
                    postgres = result.Status,
                    database = result.Database,
                    message = result.Response
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    postgres = "disconnected",
                    error = ex.Message
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
