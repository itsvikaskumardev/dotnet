using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Chai_aur_Redis.SiteBanner.Endpoints
{
    public static class SiteBannerEndpoints
    {
        private const string BannerKey = "app:banner";

        public static IEndpointRouteBuilder MapSiteBannerEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/site-banner")
                              .WithTags("2. Site Banner (Strings)");

            // Set banner in Redis
            group.MapPost("", SetBannerAsync)
                 .WithName("SetSiteBanner")
                 .WithSummary("Set banner message in Redis")
                 .WithDescription("Stores a global site banner message in Redis under key 'app:banner'.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            group.MapPost("/set-banner", SetBannerAsync)
                 .ExcludeFromDescription();

            // Get banner from Redis
            group.MapGet("", GetBannerAsync)
                 .WithName("GetSiteBanner")
                 .WithSummary("Get banner message from Redis")
                 .WithDescription("Retrieves the active site banner text stored in Redis.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            group.MapGet("/get-banner", GetBannerAsync)
                 .ExcludeFromDescription();

            // Check if banner exists in Redis
            group.MapGet("/exists", BannerExistsAsync)
                 .WithName("CheckBannerExists")
                 .WithSummary("Check if banner key exists in Redis")
                 .WithDescription("Checks if key 'app:banner' exists in Redis using KeyExists.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            group.MapGet("/banner/exists", BannerExistsAsync)
                 .ExcludeFromDescription();

            // Delete banner from Redis
            group.MapDelete("", DeleteBannerAsync)
                 .WithName("DeleteSiteBanner")
                 .WithSummary("Delete banner message from Redis")
                 .WithDescription("Deletes key 'app:banner' from Redis.")
                 .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status503ServiceUnavailable);

            group.MapDelete("/delete-banner", DeleteBannerAsync)
                 .ExcludeFromDescription();

            return routes;
        }

        private static async Task<IResult> SetBannerAsync(IConnectionMultiplexer redis, [FromBody] BannerRequest? request)
        {
            try
            {
                var db = redis.GetDatabase();
                var bannerText = !string.IsNullOrWhiteSpace(request?.Banner)
                    ? request.Banner.Trim()
                    : "This is a Default Value of this Key";

                await db.StringSetAsync(BannerKey, bannerText);

                return Results.Ok(new
                {
                    success = true,
                    message = "Banner saved successfully to Redis",
                    banner = bannerText
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    success = false,
                    error = ex.Message,
                    hint = "Ensure Redis is running (e.g., 'docker compose up -d')"
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }

        private static async Task<IResult> GetBannerAsync(IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var value = await db.StringGetAsync(BannerKey);

                if (value.IsNullOrEmpty)
                {
                    return Results.Ok(new
                    {
                        success = true,
                        exists = false,
                        message = "No banner is currently set in Redis",
                        banner = (string?)null
                    });
                }

                return Results.Ok(new
                {
                    success = true,
                    exists = true,
                    banner = value.ToString()
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    success = false,
                    error = ex.Message,
                    hint = "Ensure Redis is running (e.g., 'docker compose up -d')"
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }

        private static async Task<IResult> BannerExistsAsync(IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var exists = await db.KeyExistsAsync(BannerKey);

                return Results.Ok(new
                {
                    success = true,
                    exists,
                    key = BannerKey
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    success = false,
                    error = ex.Message,
                    hint = "Ensure Redis is running (e.g., 'docker compose up -d')"
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }

        private static async Task<IResult> DeleteBannerAsync(IConnectionMultiplexer redis)
        {
            try
            {
                var db = redis.GetDatabase();
                var deleted = await db.KeyDeleteAsync(BannerKey);

                return Results.Ok(new
                {
                    success = true,
                    message = deleted ? "Banner deleted successfully from Redis" : "Banner key did not exist in Redis",
                    deleted
                });
            }
            catch (Exception ex)
            {
                return Results.Json(new
                {
                    success = false,
                    error = ex.Message,
                    hint = "Ensure Redis is running (e.g., 'docker compose up -d')"
                }, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }
    }

    public record BannerRequest(string? Banner);
}
